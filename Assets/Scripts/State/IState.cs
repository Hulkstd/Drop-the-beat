using Bencodex.Types;

namespace State
{
    public interface IState
    {
        IValue Serialize();
    }
}