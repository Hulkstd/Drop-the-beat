using System.Collections.Generic;
using System.Linq;
using Libplanet;
using LibplanetUnity;
using State;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace GameManager
{
    public class RankingManager : Singleton<RankingManager>
    {
        private static Agent Agent => Agent.instance;
        private static readonly Address RankStateAddress = RankingState.Address;
        private static readonly System.Collections.Generic.Dictionary<Address, AgentState> AgentMap =
            new Dictionary<Address, AgentState>();
        
        public RankContent _rankContent;
        public ScrollRect _rankList;

        public void Initialize(string title)
        {
            RemoveAll();
            
            var value = Agent.GetState(RankStateAddress);
            var rankState = (value is null ? new RankingState() : new RankingState((Bencodex.Types.Dictionary)value));

            var rankingInfos = rankState.GetRanking(title);
            var rank = rankingInfos.Select(kv => kv.Value).OrderByDescending(info => info.score);

            _rankContent.gameObject.SetActive(true);

            var i = 1;
            var bgImage = true;
            RankContent prevContent = null; 
            foreach (var info in rank)
            {
                if (!AgentMap.ContainsKey(info.address))
                    AgentMap[info.address] = new AgentState((Bencodex.Types.Dictionary) Agent.GetState(info.address));
                                
                
                var content = Instantiate(_rankContent, _rankList.content);
                if(prevContent != null && prevContent.Score == info.score)
                    content.Initialize(prevContent.Rank, AgentMap[info.address].Name, AgentMap[info.address].address.ToHex(), info.score, bgImage);
                else
                    content.Initialize(i, AgentMap[info.address].Name, AgentMap[info.address].address.ToHex(), info.score, bgImage);
                
                i++;
                bgImage = !bgImage;
                prevContent = content;
            }
            _rankContent.gameObject.SetActive(false);
        }

        public void RemoveAll()
        {
            foreach (Transform val in _rankList.content.transform)
            {
                Destroy(val.gameObject);
            }
        }
        
    }
}
