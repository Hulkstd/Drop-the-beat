using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace GameManager
{
    public class BMSCapacity : Singleton<BMSCapacity>
    {
        [NonSerialized] public bool IsDone;
        [NonSerialized] public Bms Bms;
        [NonSerialized] public bool IsGameDone;
        [FormerlySerializedAs("currentBar")] public int _currentBar = 0;
        [FormerlySerializedAs("currentBeat")] public int _currentBeat = 0;
        [FormerlySerializedAs("currentBeat")] public int _currentBeatWeight = 1;
        [FormerlySerializedAs("path")] public string _path;
        private void Start()
        {
            Bms = Parser.Parse(_path);
            
            IsDone = true;
        }
    }
}
