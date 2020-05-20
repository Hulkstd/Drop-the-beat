using System.Collections.Generic;

namespace GameManager
{
    public static class GCManager
    {
        public static readonly Dictionary<string, object> Waitfor = new Dictionary<string, object>();

        public static object PushDataOnWaitfor(string a, object data)
        {
            Waitfor.Add(a, data);

            return data;
        }
    }
}
