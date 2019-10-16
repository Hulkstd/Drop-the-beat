using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace GameManager
{
    public class Timer : Singleton<Timer>
    {
        [FormerlySerializedAs("CurrentTime")] public float _currentTime;
        private GCManager _gcManager;
        private BMSCapacity _bms;
        [SerializeField]private float _secondPerBar;

        private void Start()
        {
            StartCoroutine(TimerStart());
        }

        private IEnumerator TimerStart()
        {
            yield return new WaitUntil(() => BMSCapacity.Instance.IsDone);
            StartGame();
        }

        public void StartGame()
        {
            StartCoroutine(_Timer());
        }
        
        private IEnumerator _Timer()
        {
            _gcManager = GCManager.Instance;
            _bms = BMSCapacity.Instance;
            _secondPerBar = (60 * (1f / BMSCapacity.Instance._currentBeatWeight)) / ((1f / BMSCapacity.Instance._currentBeatWeight) / 4 * _bms.Bms.Head.Bpm);
            var c = 0;
            while (!_bms.IsGameDone)
            {
                yield return _gcManager.Waitfor.ContainsKey(_secondPerBar)
                    ? (WaitForSeconds) _gcManager.Waitfor[_secondPerBar]
                    : (WaitForSeconds) _gcManager.PushDataOnWaitfor(_secondPerBar,
                        new WaitForSeconds(_secondPerBar));

                _currentTime += _secondPerBar;
                _secondPerBar = (60 * (1f / BMSCapacity.Instance._currentBeatWeight)) / ((1f / BMSCapacity.Instance._currentBeatWeight) / 4 * _bms.Bms.Head.Bpm);
                BMSCapacity.Instance._currentBar++;
            }

            yield return null;
        }
    }
}
