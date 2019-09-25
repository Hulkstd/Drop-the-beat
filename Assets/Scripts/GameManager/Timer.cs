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

        public void StartGame()
        {
            StartCoroutine(_Timer());
        }
        
        private IEnumerator _Timer()
        {
            _gcManager = GCManager.Instance;
            _bms = BMSCapacity.Instance;
            while (!_bms.IsGameDone)
            {
                yield return _gcManager.Waitfor.ContainsKey(_bms._currentBeat * _bms._currentBeatWeight) ? 
                    (WaitForSeconds)_gcManager.Waitfor[_bms._currentBeat * _bms._currentBeatWeight] : 
                    (WaitForSeconds)_gcManager.PushDataOnWaitfor(_bms._currentBeat * _bms._currentBeatWeight, 
                        new WaitForSeconds(_bms._currentBeat * _bms._currentBeatWeight));
                
                _currentTime += _bms._currentBeat * _bms._currentBeatWeight;
            }

            yield return null;
        }
    }
}
