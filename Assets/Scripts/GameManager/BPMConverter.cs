using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using UnityEngine;
using Utility;

namespace GameManager
{
    public class BPMConverter : MonoBehaviour
    {
        private BMSCapacity _bms;
        private SortedDictionary<int, Dictionary<Bms.DataSection.EventChannel, List<string>>> _command;
        private int _prevBar = -1;

        private void Start()
        {
            StartCoroutine(LoadBms());
        }
        
        private IEnumerator LoadBms()
        {
            while (!BMSCapacity.Instance.IsDone)
                yield return null;

            _bms = BMSCapacity.Instance;
            _command = _bms.Bms.Data.GetCommandSection();
            
            StartCoroutine(ConvertBGM());
        }

        private IEnumerator ConvertBGM()
        {
            while (true)
            {
                while (Mathf.Abs(_bms.BPMs.Top.Timing - Timer.PlayingTime) < 0.1f)
                {
                    StartCoroutine(ConvertBGM(0, _bms.BPMs.Top.Bpm));
                    _bms.BPMs.Pop();
                    Debug.Log("BPM");
                }

                while (Mathf.Abs(_bms.Stops.Top.Timing - Timer.PlayingTime) < 0.1f)
                {
                    StartCoroutine(StopAction(0, _bms.Stops.Top.Time));
                    _bms.Stops.Pop();
                    Debug.Log($"STOP{_bms.Stops.Top.Time / 192 / _bms.Bms.Head.Bpm * 240}");
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
            timeScale /= _bms.Bms.Head.Bpm;
            timeScale *= 240;

            Timer.Instance._curBeatStopTime += timeScale;
//            Debug.Log(timeScale);
            yield return GCManager.Instance.Waitfor.ContainsKey(timeScale + "wfsr")
                ? (WaitForSecondsRealtime) GCManager.Instance.Waitfor[timeScale + "wfsr"]
                : (WaitForSecondsRealtime) GCManager.Instance.PushDataOnWaitfor(timeScale + "wfsr", new WaitForSecondsRealtime(timeScale));

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
