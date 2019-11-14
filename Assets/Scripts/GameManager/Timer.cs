using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using Node;
using UnityEngine;
using UnityEngine.Serialization;
using Utility;

namespace GameManager
{
    public class Timer : Singleton<Timer>
    {
        [FormerlySerializedAs("CurrentTime")] public float _currentTime;
        [FormerlySerializedAs("CurBeatStopTime")] public float _curBeatStopTime = 0f;
        private GCManager _gcManager;
        private BMSCapacity _bms;
        [SerializeField]private float _secondPerBar;
        public static float PlayingTime => Time.timeSinceLevelLoad - _startTime;
        private static float _startTime = 0f; 

        private void Start()
        {
            StartCoroutine(TimerStart());
        }

        private IEnumerator TimerStart()
        {
            yield return new WaitUntil(() => NodeCreator.Instance._doneLoading);
            StartGame();
        }

        private void StartGame()
        {
            _startTime = Time.timeSinceLevelLoad;
            //StartCoroutine(_Timer());
        }
        
        private IEnumerator _Timer()
        {
            yield return null;
        }

        private static string GetHex(string str, int i) => Utility.Utility.GetHex(str, i);
        
        private static SortQueue<KeyValuePair<float, float>> MakeBpmList(List<string> bpmC, List<string> bpmExC,
            Dictionary<string, float> dic) => Utility.Utility.MakeBpmList(bpmC, bpmExC, dic);
    }
}
