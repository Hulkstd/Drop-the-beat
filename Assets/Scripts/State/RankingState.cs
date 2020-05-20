using System;
using System.Collections.Generic;
using System.Linq;
using Bencodex.Types;
using Libplanet;
using UnityEngine;
using Utility;

namespace State
{
    public class RankingState : State
    {
        public static readonly Address Address = new Address(new byte[]
        {
            0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x1
        });

        public class RankingInfo : IState
        {
            public Address address;
            public long score;

            public static RankingInfo Max(RankingInfo a, RankingInfo b)
                => a.score < b.score ? b : a;

            public RankingInfo(Address address, long score)
            {
                this.address = address;
                this.score = score;
            }

            public RankingInfo(Bencodex.Types.Dictionary value)
            {
                address = value["address"].ToAddress();
                score = value["score"].ToLong();
            }

            public IValue Serialize() =>
                new Dictionary(new Dictionary<IKey, IValue>
                {
                    [(Bencodex.Types.Text) "address"] = address.Serialize(),
                    [(Bencodex.Types.Text) "score"] = score.Serialize(),
                });
        }


        private readonly Dictionary<string, Dictionary<Address, RankingInfo>> _map;

        public RankingState() : base(Address)
        {
            _map = new Dictionary<string, Dictionary<Address, RankingInfo>>();
        }

        public RankingState(Bencodex.Types.Dictionary serialized) : base(serialized)
        {
            _map = ((Bencodex.Types.Dictionary) serialized["map"]).ToDictionary(
                kv1 => kv1.Key.ToString_(),
                kv1 => ((Bencodex.Types.Dictionary) kv1.Value).ToDictionary(
                    kv2 => kv2.Key.ToAddress(),
                    kv2 => new RankingInfo((Bencodex.Types.Dictionary) kv2.Value)
                )
            );
        }

        public override IValue Serialize() =>
            new Dictionary(new Dictionary<IKey, IValue>
            {
                [(Bencodex.Types.Text) "map"] = new Dictionary(_map.Select(kv1 =>
                    new KeyValuePair<IKey, IValue>(
                        (Bencodex.Types.Text) kv1.Key.Serialize(),
                        new Dictionary(kv1.Value.Select(kv2 =>
                                new KeyValuePair<IKey, IValue>(
                                    (Bencodex.Types.Binary) kv2.Key.Serialize(),
                                    kv2.Value.Serialize()
                                )
                            )
                        )
                    )
                )),
            }.Union((Bencodex.Types.Dictionary)base.Serialize()));
        
        public void Update(string title, Address playerAddress, long score)
        {
            if(!_map.ContainsKey(title))
                _map[title] = new Dictionary<Address, RankingInfo>();
            if(!_map[title].ContainsKey(playerAddress))
                _map[title][playerAddress] = new RankingInfo(address, 0);

            _map[title][playerAddress] = RankingInfo.Max(_map[title][playerAddress], new RankingInfo(playerAddress, score));
        }

        public Dictionary<Address, RankingInfo> GetRanking(string title)
        {
            if (!_map.ContainsKey(title))
                _map[title] = new Dictionary<Address, RankingInfo>();

            return _map[title];
        }
    }
}