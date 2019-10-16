using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using UnityEngine;

namespace GameManager
{
    public class GCManager : Singleton<GCManager>
    {
        [NonSerialized] public Dictionary<float, object> Waitfor;

        private void Start()
        {
            Waitfor = new Dictionary<float, object>();
        }

        public object PushDataOnWaitfor(float a, object data)
        {
            Waitfor.Add(a, data);

            return data;
        }
    }
}
