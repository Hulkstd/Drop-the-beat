using Bencodex.Types;
using Libplanet.Action;
using LibplanetUnity.Action;
using State;
using UI;
using Utility;

namespace Action
{
    [ActionType("rename")]
    public class Rename : ActionBase
    {
        public string Name;

        public override IValue PlainValue
            => Bencodex.Types.Dictionary.Empty
                .SetItem("name", Name.Serialize());
        
        public override void LoadPlainValue(IValue plainValue)
        {
            Name = ((Bencodex.Types.Dictionary) plainValue)["name"].ToString_();
        }

        public override IAccountStateDelta Execute(IActionContext ctx)
        {
            var states = ctx.PreviousStates;

            if (ctx.Rehearsal)
            {
                return states.SetState(ctx.Signer, MarkChanged);
            }

            var agentState = new AgentState(ctx.Signer)
            {
                Name = Name
            };

            return states.SetState(ctx.Signer, agentState.Serialize());
        }

        public override void Render(IActionContext context, IAccountStateDelta nextStates)
        {
        }
        
        public override void Unrender(IActionContext context, IAccountStateDelta nextStates)
        {
        }

    }
}