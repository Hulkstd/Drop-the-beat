using System.Collections;
using Node;
using UnityEngine;

namespace GameManager
{
    public class Timer : Singleton<Timer>
    {
        public static float PlayingTime => Time.timeSinceLevelLoad - _startTime;
        private static float _startTime; 

        private void Start()
        {
            StartCoroutine(TimerStart());
        }

        private static IEnumerator TimerStart()
        {
            yield return new WaitUntil(() => NodeCreator.Instance._doneLoading);
            StartGame();
        }

        private static void StartGame()
        {
            _startTime = Time.timeSinceLevelLoad;
        }
    }
}
