using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Libplanet;
using Libplanet.Crypto;
using Libplanet.Net;
using NetMQ;
using UnityEngine;
using LibplanetUnity.Helper;
using LibplanetUnity.Action;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Bencodex.Types;
using Libplanet.Action;
using Libplanet.Blockchain;
using Libplanet.Blockchain.Policies;
using Libplanet.Store;
using Libplanet.Tx;
using Libplanet.Blocks;

namespace LibplanetUnity
{
    public class Agent : MonoSingleton<Agent>
    {
        private static readonly TimeSpan BlockInterval = TimeSpan.FromSeconds(10);

        private const int SwarmDialTimeout = 5000;

        private const string PlayerPrefsKeyOfAgentPrivateKey = "private_key_agent";

        private const string AgentStoreDirName = "planetarium";

        private static readonly string CommandLineOptionsJsonPath = Path.Combine(Application.streamingAssetsPath, "clo.json");

        private static readonly string GenesisBlockPath = Path.Combine(Application.streamingAssetsPath, "genesis");

        private static readonly string DefaultStoragePath =
            Path.Combine(Application.persistentDataPath, AgentStoreDirName);

        private static IEnumerator _miner;

        private static IEnumerator _swarmRunner;

        private readonly ConcurrentQueue<System.Action> _actions = new ConcurrentQueue<System.Action>();

        private PrivateKey PrivateKey { get; set; }

        private BlockChain<PolymorphicAction<ActionBase>> _blocks;

        private Swarm<PolymorphicAction<ActionBase>> _swarm;

        private DefaultStore _store;

        private ImmutableList<Peer> _seedPeers;

        private IImmutableSet<Address> _trustedPeers;

        private CancellationTokenSource _cancellationTokenSource;

        public Address Address { get; private set; }

        public static void Initialize()
        {
            instance.InitAgent();
        }

        public IValue GetState(Address address)
        {
            return _blocks.GetState(address);
        }
        
        public void GetState(Address address, out IValue value)
        {
            value = _blocks.GetState(address);
        }

        public void MakeTransaction(IEnumerable<ActionBase> gameActions)
        {
            var actions = gameActions.Select(gameAction => (PolymorphicAction<ActionBase>) gameAction).ToList();
            Task.Run(() => MakeTransaction(actions, true));
        }

        private void InitAgent()
        {
            var options = GetOptions(CommandLineOptionsJsonPath);
            var privateKey = GetPrivateKey(options);
            var peers = options.Peers.Select(LoadPeer).ToImmutableList();
            var iceServers = options.IceServers.Select(LoadIceServer).ToImmutableList();
            var host = options.Host;
            int? port = options.Port;
            var storagePath = options.StoragePath ?? DefaultStoragePath;

            Init(privateKey, storagePath, peers, iceServers, host, port);

            _miner = options.NoMiner ? null : CoMiner();

            StartSystemCoroutines();
            StartNullableCoroutine(_miner);
        }

        private void Init(PrivateKey privateKey, string path, IEnumerable<Peer> peers,
            IEnumerable<IceServer> iceServers, string host, int? port)
        {
            var policy = new BlockPolicy<PolymorphicAction<ActionBase>>(
                null,
                BlockInterval,
                100000,
                2048);
            PrivateKey = privateKey;
            Address = privateKey.PublicKey.ToAddress();
            _store = new DefaultStore(path, flush: false);
            Block<PolymorphicAction<ActionBase>> genesis = 
                Block<PolymorphicAction<ActionBase>>.FromBencodex(
                    File.ReadAllBytes(GenesisBlockPath)
                );
            _blocks = new BlockChain<PolymorphicAction<ActionBase>>(
                policy, 
                _store,
                genesis
            );

            if (!(host is null) || iceServers.Any()) 
            {
                _swarm = new Swarm<PolymorphicAction<ActionBase>>(
                    _blocks,
                    privateKey,
                    appProtocolVersion: 1,
                    host: host,
                    listenPort: port,
                    iceServers: iceServers,
                    differentVersionPeerEncountered: DifferentAppProtocolVersionPeerEncountered
                );
                
                _seedPeers = peers.Where(peer => peer.PublicKey != privateKey.PublicKey).ToImmutableList();
                _trustedPeers = _seedPeers.Select(peer => peer.Address).ToImmutableHashSet();
            }
            _cancellationTokenSource = new CancellationTokenSource();
        }

        private static Options GetOptions(string jsonPath)
        {
            if (File.Exists(jsonPath))
            {
                return JsonUtility.FromJson<Options>(
                    File.ReadAllText(jsonPath)
                );
            }
            else
            {
                return CommnadLineParser.GetCommandLineOptions() ?? new Options();
            }
        }

        public void RunOnMainThread(System.Action action)
        {
            _actions.Enqueue(action);
        }

        private static PrivateKey GetPrivateKey(Options options)
        {
            PrivateKey privateKey;
            var privateKeyHex = options.PrivateKey ?? PlayerPrefs.GetString(PlayerPrefsKeyOfAgentPrivateKey, "");

            if (string.IsNullOrEmpty(privateKeyHex))
            {
                privateKey = new PrivateKey();
                PlayerPrefs.SetString(PlayerPrefsKeyOfAgentPrivateKey, ByteUtil.Hex(privateKey.ByteArray));
            }
            else
            {
                privateKey = new PrivateKey(ByteUtil.ParseHex(privateKeyHex));
            }

            return privateKey;
        }

        private static BoundPeer LoadPeer(string peerInfo)
        {
            string[] tokens = peerInfo.Split(',');
            var pubKey = new PublicKey(ByteUtil.ParseHex(tokens[0]));
            string host = tokens[1];
            var port = int.Parse(tokens[2]);

            return new BoundPeer(pubKey, new DnsEndPoint(host, port), 0);
        }

        private static IceServer LoadIceServer(string iceServerInfo)
        {
            var uri = new Uri(iceServerInfo);
            string[] userInfo = uri.UserInfo.Split(':');

            return new IceServer(new[] {uri}, userInfo[0], userInfo[1]);
        }

        #region Mono

        protected override void OnDestroy()
        {
            NetMQConfig.Cleanup(false);

            base.OnDestroy();
            _store.Dispose();
        }

        #endregion

        private void StartSystemCoroutines()
        {
            _swarmRunner = CoSwarmRunner();

            StartNullableCoroutine(_swarmRunner);
            StartCoroutine(CoProcessActions());
        }

        private Coroutine StartNullableCoroutine(IEnumerator routine)
        {
            return ReferenceEquals(routine, null) ? null : StartCoroutine(routine);
        }

        private IEnumerator CoSwarmRunner()
        {
            if (_swarm is null)
            {
                yield break;
            }
            
            var bootstrapTask = Task.Run(async () =>
            {
                try
                {
                    await _swarm.BootstrapAsync(
                        _seedPeers,
                        5000,
                        5000,
                        cancellationToken: _cancellationTokenSource.Token
                    );
                }
                catch (Exception e)
                {
                    Debug.LogFormat("Exception occurred during bootstrap {0}", e);
                }
            });

            yield return new WaitUntil(() => bootstrapTask.IsCompleted);

            Debug.Log("PreloadingStarted event was invoked");

            DateTimeOffset started = DateTimeOffset.UtcNow;
            long existingBlocks = _blocks?.Tip?.Index ?? 0;
            Debug.Log("Preloading starts");

            var swarmPreloadTask = Task.Run(async () =>
            {
                await _swarm.PreloadAsync(
                    TimeSpan.FromMilliseconds(SwarmDialTimeout),
                    null,
                    trustedStateValidators: _trustedPeers,
                    cancellationToken: _cancellationTokenSource.Token
                );
            });

            yield return new WaitUntil(() => swarmPreloadTask.IsCompleted);
            DateTimeOffset ended = DateTimeOffset.UtcNow;

            if (swarmPreloadTask.Exception is Exception exc)
            {
                Debug.LogErrorFormat(
                    "Preloading terminated with an exception: {0}",
                    exc
                );
                throw exc;
            }

            var index = _blocks?.Tip?.Index ?? 0;
            Debug.LogFormat(
                "Preloading finished; elapsed time: {0}; blocks: {1}",
                ended - started,
                index - existingBlocks
            );

            var swarmStartTask = Task.Run(async () =>
            {
                try
                {
                    await _swarm.StartAsync();
                }
                catch (TaskCanceledException)
                {
                }
                catch (Exception e)
                {
                    Debug.LogErrorFormat(
                        "Swarm terminated with an exception: {0}",
                        e
                    );
                    throw;
                }
            });

            Task.Run(async () =>
            {
                await _swarm.WaitForRunningAsync();

                Debug.LogFormat(
                    "The address of this node: {0},{1},{2}",
                    ByteUtil.Hex(PrivateKey.PublicKey.Format(true)),
                    _swarm.EndPoint.Host,
                    _swarm.EndPoint.Port
                );
            });

            yield return new WaitUntil(() => swarmStartTask.IsCompleted);
        }

        private void DifferentAppProtocolVersionPeerEncountered(object sender, DifferentProtocolVersionEventArgs e)
        {
            Debug.LogWarningFormat("Different Version Encountered Expected: {0} Actual : {1}",
                e.ExpectedVersion, e.ActualVersion);
        }

        private IEnumerator CoProcessActions()
        {
            while(true)
            {
                if (_actions.TryDequeue(out System.Action action))
                {
                    action();
                }
                yield return new WaitForSeconds(0.1f);
            }
        }

        private static bool WantsToQuit()
        {
            return true;
        }

        [RuntimeInitializeOnLoadMethod]
        private static void RunOnStart()
        {
            Application.wantsToQuit += WantsToQuit;
        }

        private Transaction<PolymorphicAction<ActionBase>> MakeTransaction(
                    IEnumerable<PolymorphicAction<ActionBase>> actions, bool broadcast)
        {
            var polymorphicActions = actions.ToArray();
            Debug.LogFormat("Make Transaction with Actions: `{0}`",
                string.Join(",", polymorphicActions.Select(i => i.InnerAction)));
            return _blocks.MakeTransaction(PrivateKey, polymorphicActions);
        }

        private IEnumerator CoMiner()
        {
            while (true)
            {
                var txs = new HashSet<Transaction<PolymorphicAction<ActionBase>>>();

                var task = Task.Run(async () =>
                {
                    var block = await _blocks.MineBlock(Address);

                    if (_swarm?.Running ?? false)
                    {
                        _swarm.BroadcastBlocks(new[] {block});
                    }

                    return block;
                });
                yield return new WaitUntil(() => task.IsCompleted);

                if (!task.IsCanceled && !task.IsFaulted)
                {
                    var block = task.Result;
                    Debug.Log($"created block index: {block.Index}, difficulty: {block.Difficulty}");
                }
                else
                {
                    var invalidTxs = txs;
                    var retryActions = new HashSet<IImmutableList<PolymorphicAction<ActionBase>>>();

                    if (task.IsFaulted)
                    {
                        foreach (var ex in task.Exception.InnerExceptions)
                        {
                            if (ex is InvalidTxNonceException invalidTxNonceException)
                            {
                                var invalidNonceTx = _store.GetTransaction<PolymorphicAction<ActionBase>>(invalidTxNonceException.TxId);

                                if (invalidNonceTx.Signer == Address)
                                {
                                    Debug.Log($"Tx[{invalidTxNonceException.TxId}] nonce is invalid. Retry it.");
                                    retryActions.Add(invalidNonceTx.Actions);
                                }
                            }

                            if (ex is InvalidTxException invalidTxException)
                            {
                                Debug.Log($"Tx[{invalidTxException.TxId}] is invalid. mark to unstage.");
                                invalidTxs.Add(_store.GetTransaction<PolymorphicAction<ActionBase>>(invalidTxException.TxId));
                            }

                            Debug.LogException(ex);
                        }
                    }
                    _blocks.UnstageTransactions(invalidTxs);

                    foreach (var retryAction in retryActions)
                    {
                        MakeTransaction(retryAction, true);
                    }
                }
            }
        }
    }
}
