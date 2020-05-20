using System;
using System.Collections.Generic;
using System.Linq;
using Bencodex.Types;
using Libplanet;
using Libplanet.Serialization;
using Utility;

namespace State
{
    public class AgentState : State
    {
        public string Name;

        public AgentState(Address address) : base(address)
        {
            Name = string.Empty;
        }

        public AgentState(Bencodex.Types.Dictionary serialized) : base(serialized)
        {
            Name = serialized["name"].ToString();
        }

        public override IValue Serialize() =>
        new Bencodex.Types.Dictionary( 
            new Dictionary<IKey, IValue> {
                
                [(Bencodex.Types.Text)"name"] = Name.Serialize(),    
                    
            }.Union((Bencodex.Types.Dictionary)base.Serialize()));
    }
}