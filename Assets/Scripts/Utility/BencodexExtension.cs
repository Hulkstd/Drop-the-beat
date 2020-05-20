using System.Globalization;
using System.Linq;
using Bencodex.Types;
using Libplanet;

namespace Utility
{
    public static class BencodexExtension
    {
        #region Address

        public static Address ToAddress(this IValue value)
            => new Address(((Binary)value).Value);

        public static IValue Serialize(this Address value)
            => (Bencodex.Types.Binary) value.ToByteArray();    

        #endregion

        #region String

        public static string ToString_(this IValue value)
            => ((Bencodex.Types.Text) value).Value;

        public static IValue Serialize(this string value)
            => (Bencodex.Types.Text) value;

        #endregion

        #region Long

        public static long ToLong(this IValue value)
            => long.Parse(((Bencodex.Types.Text) value).Value);

        public static IValue Serialize(this long value)
            => (Bencodex.Types.Text) value.ToString(CultureInfo.InvariantCulture);

        #endregion
    }
}