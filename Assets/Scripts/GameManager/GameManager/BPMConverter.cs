using System.Collections;
using Node;
using UI.Animation;
using UnityEngine;

namespace GameManager
{
    public class BPMConverter : MonoBehaviour
    {
        private BMSCapacity _bms;

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
            while (!_bms.IsGameDone)
            {
                
                if (_bms.BPMs.Length == 0)
                    goto Stop;
                
                while (_bms.BPMs.Length != 0 && Judgement.Judge((float)_bms.BPMs.Top.Timing) == JudgementText.Judgement.Excellent)
                {
                    StartCoroutine(ConvertBGM(0, (float)_bms.BPMs.Top.Bpm));
                    _bms.BPMs.Pop();
//                    Debug.Log("BPM");
                }

                Stop:

                while (_bms.Stops.Length != 0 && Judgement.Judge((float)_bms.Stops.Top.Timing) == JudgementText.Judgement.Excellent)
                {
                    StartCoroutine(StopAction(0, _bms.Stops.Top.Time));
                    _bms.Stops.Pop();
//                    Debug.Log($"STOP");
                }
                
                yield return null;
            }

            yield return null;
        }

        private IEnumerator ConvertBGM(float time, float bpm)
        {
            yield return GCManager.Waitfor.ContainsKey(time + "wfs")
                ? (WaitForSeconds) GCManager.Waitfor[time + "wfs"]
                : (WaitForSeconds) GCManager.PushDataOnWaitfor(time + "wfs", new WaitForSeconds(time));
            
            _bms.Bms.SetBpm(bpm);
        }

        private IEnumerator StopAction(float time, float timeScale)
        {
            yield return GCManager.Waitfor.ContainsKey(time + "wfs")
                ? (WaitForSeconds) GCManager.Waitfor[time + "wfs"]
                : (WaitForSeconds) GCManager.PushDataOnWaitfor(time + "wfs", new WaitForSeconds(time));

            CameraMove.Instance._isStop = true;
            Time.timeScale = 0;
            timeScale /= 192;
            timeScale /= (float)_bms.Bms.Head.Bpm;
            timeScale *= 240;
            yield return GCManager.Waitfor.ContainsKey(timeScale + "wfsr")
                ? (WaitForSecondsRealtime) GCManager.Waitfor[timeScale + "wfsr"]
                : (WaitForSecondsRealtime) GCManager.PushDataOnWaitfor(timeScale + "wfsr", new WaitForSecondsRealtime(timeScale));

            Time.timeScale = 1;
            CameraMove.Instance._isStop = false;
        }
    }
}
