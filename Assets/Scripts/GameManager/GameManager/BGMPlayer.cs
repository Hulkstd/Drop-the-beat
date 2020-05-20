using System;
using System.Collections;
using Node;
using UI.Animation;
using UnityEngine;

namespace GameManager
{
    public class BGMPlayer : Singleton<BGMPlayer>
    {
        [NonSerialized] public bool IsDone;
        private BMSCapacity _bms;
        private float _secondPerBar;
        
        private void Start()
        {
            StartCoroutine(LoadBms());
        }

        private void Update()
        {
            if (!NodeCreator.Instance._doneLoading)
                return;

            if (_bms.BGMs.Length == 0)
                return;
            
            //Mathf.Abs((float)_bms.BGMs.Top.Timing - Timer.PlayingTime) * 1000 < 21f
            while (_bms.BGMs.Length != 0 && Judgement.Judge((float)_bms.BGMs.Top.Timing) == JudgementText.Judgement.Excellent)
            {
                SoundManager.Instance.AddPlaySound(0, _bms.Bms.GetAudioClip(_bms.BGMs.Top.Sound));
                _bms.BGMs.Pop();
//                Debug.Log($"BGM {(float)_bms.BGMs.Top.Timing - Timer.PlayingTime}");
            }
            while (_bms.BGMs.Length != 0 && Judgement.Judge((float)_bms.BGMs.Top.Timing) == JudgementText.Judgement.Great)
            {
                SoundManager.Instance.AddPlaySound(0, _bms.Bms.GetAudioClip(_bms.BGMs.Top.Sound));
                _bms.BGMs.Pop();
//                Debug.Log($"BGM {(float)_bms.BGMs.Top.Timing - Timer.PlayingTime}");
            }
        }

        private IEnumerator LoadBms()
        {
            yield return new WaitUntil(() => BMSCapacity.Instance.IsDone);

            _bms = BMSCapacity.Instance;
        }
    }
}
