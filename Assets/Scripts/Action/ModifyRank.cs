using Bencodex.Types;
using Libplanet;
using Libplanet.Action;
using LibplanetUnity.Action;
using State;
using Utility;

namespace Action
{
    [ActionType("modify_rank")]
    public class ModifyRank : ActionBase
    {
        public string Title;
        public long Score;

        public override IValue PlainValue =>
            Bencodex.Types.Dictionary.Empty
                .SetItem("title", Title.Serialize())
                .SetItem("score", Score.Serialize());

        public override void LoadPlainValue(IValue plainValue)
        {
            var dic = (Bencodex.Types.Dictionary) plainValue;

            Title = dic["title"].ToString_();
            Score = dic["score"].ToLong();
        }

        public override IAccountStateDelta Execute(IActionContext ctx)
        {
            var states = ctx.PreviousStates;
            var rankingAddress = RankingState.Address;

            if (ctx.Rehearsal)
            {
                return states.SetState(ctx.Signer, MarkChanged)
                    .SetState(rankingAddress, MarkChanged);
            }

            var rankingState = states.TryGetState(rankingAddress, out Bencodex.Types.Dictionary bdict)
                ? new RankingState(bdict)
                : new RankingState();

            rankingState.Update(Title, ctx.Signer, Score);

            return states.SetState(rankingAddress, rankingState.Serialize());
        }

        public override void Render(IActionContext ctx, IAccountStateDelta nextStates)
        {
        }
        
        public override void Unrender(IActionContext ctx, IAccountStateDelta nextStates)
        {
        }

    }
}