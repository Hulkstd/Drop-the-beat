using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using UnityEngine;

namespace GameManager
{
    public class GCManager : Singleton<GCManager>
    {
        [NonSerialized] public Dictionary<string, object> Waitfor;

        private void Start()
        {
            Waitfor = new Dictionary<string, object>();
        }

        public object PushDataOnWaitfor(string a, object data)
        {
            Waitfor.Add(a, data);

            return data;
        }
    }
}
