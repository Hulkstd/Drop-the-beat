using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using Node;
using UI.Animation;
using UnityEngine;
using Utility;

namespace GameManager
{
    public class BPMConverter : MonoBehaviour
    {
        private BMSCapacity _bms;
        private int _prevBar = -1;

        private void Start()
        {
            StartCoroutine(LoadBms());
        }
        
        private IEnumerator LoadBms()
        {
            yield return new WaitUntil(() => BMSCapacity.Instance.IsDone);

            _bms = BMSCapacity.Instance;
            
            StartCoroutine(ConvertBGM());
        }

        private IEnumerator ConvertBGM()
        {
            yield return new WaitUntil(() => NodeCreator.Instance._doneLoading);
            while (true)
            {
                
                if (_bms.BPMs.Length == 0)
                    goto Stop;
                
                while (_bms.BPMs.Length != 0 && Judgement.Judge((float)_bms.BPMs.Top.Timing) == JudgementText.Judgement.Excelent)
                {
                    StartCoroutine(ConvertBGM(0, (float)_bms.BPMs.Top.Bpm));
                    _bms.BPMs.Pop();
//                    Debug.Log("BPM");
                }

                Stop:

                while (_bms.Stops.Length != 0 && Judgement.Judge((float)_bms.Stops.Top.Timing) == JudgementText.Judgement.Excelent)
                {
                    StartCoroutine(StopAction(0, _bms.Stops.Top.Time));
                    _bms.Stops.Pop();
                    Debug.Log($"STOP");
                }
                
                yield return null;
            }

            yield return null;
        }

        private IEnumerator ConvertBGM(float time, float bpm)
        {
            yield return GCManager.Instance.Waitfor.ContainsKey(time + "wfs")
                ? (WaitForSeconds) GCManager.Instance.Waitfor[time + "wfs"]
                : (WaitForSeconds) GCManager.Instance.PushDataOnWaitfor(time + "wfs", new WaitForSeconds(time));
            
         //   Debug.Log($"bpm to {bpm} in {Time.timeSinceLevelLoad}");
            _bms.Bms.SetBpm(bpm);
        }

        private IEnumerator StopAction(float time, float timeScale)
        {
            yield return GCManager.Instance.Waitfor.ContainsKey(time + "wfs")
                ? (WaitForSeconds) GCManager.Instance.Waitfor[time + "wfs"]
                : (WaitForSeconds) GCManager.Instance.PushDataOnWaitfor(time + "wfs", new WaitForSeconds(time));

            CameraMove.Instance._isStop = true;
            Time.timeScale = 0;
            timeScale /= 192;
            timeScale /= (float)_bms.Bms.Head.Bpm;
            timeScale *= 240;
            Debug.Log($"Before Stop {Timer.PlayingTime}");
            Timer.Instance._curBeatStopTime += timeScale;
            Debug.Log(timeScale);
            yield return GCManager.Instance.Waitfor.ContainsKey(timeScale + "wfsr")
                ? (WaitForSecondsRealtime) GCManager.Instance.Waitfor[timeScale + "wfsr"]
                : (WaitForSecondsRealtime) GCManager.Instance.PushDataOnWaitfor(timeScale + "wfsr", new WaitForSecondsRealtime(timeScale));

            Debug.Log($"After Stop {Timer.PlayingTime}");
            Time.timeScale = 1;
            CameraMove.Instance._isStop = false;
        }

        private static string GetHex(string str, int i) => Utility.Utility.GetHex(str, i);

        private static int Map(int val, int inMin, int inMax, int outMin, int outMax) =>
            Utility.Utility.Map(val, inMin, inMax, outMin, outMax);
        
        private static SortQueue<KeyValuePair<float, float>> MakeBpmList(List<string> bpmC, List<string> bpmExC,
            Dictionary<string, float> dic) => Utility.Utility.MakeBpmList(bpmC, bpmExC, dic);
        
    }
}
