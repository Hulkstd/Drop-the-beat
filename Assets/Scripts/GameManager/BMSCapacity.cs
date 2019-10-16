using System;
using System.Collections;
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
        [FormerlySerializedAs("currentBeat")] public float _currentBeatWeight = 1;
        [FormerlySerializedAs("path")] public string _path;
        
        private void Start()
        {
            StartCoroutine(StartParsing());
        }

        private IEnumerator StartParsing()
        {
            Bms = Parser.Parse(_path);

            yield return new WaitUntil(() => Bms.Head.WavFileCount == Bms.Head.WavFiles.Count);

            IsDone = true;
        }
    }
}
