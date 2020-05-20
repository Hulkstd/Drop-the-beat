using System;
using System.Collections.Generic;
using Bencodex.Types;
using Libplanet;
using UnityEngine;
using Utility;

namespace State
{
    public abstract class State : IState
    {
        public Address address;

        protected State(Address address)
        {
            if (address == null)
            {
                throw new ArgumentNullException();
            }

            this.address = address;
        }

        protected State(Bencodex.Types.Dictionary serialized) :
            this(serialized["address"].ToAddress())
        {
        }

        public virtual IValue Serialize() =>
            new Dictionary(new Dictionary<IKey, IValue>
                {
                    [(Text)"address"] = address.Serialize(),
                });
    }
}
