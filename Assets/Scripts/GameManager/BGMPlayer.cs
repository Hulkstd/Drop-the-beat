using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameManager;
using Node;
using UI.Animation;
using UnityEngine;
using Utility;

namespace GameManager
{
    public class BGMPlayer : Singleton<BGMPlayer>
    {
        [NonSerialized] public bool IsDone;
        private BMSCapacity _bms;
        private float _secondPerBar;
        private int _prevBar = -1;
        
        
        private void Start()
        {
            StartCoroutine(LoadBms());
        }

        private void Update()
        {
            if (!NodeCreator.Instance._doneLoading)
                return;

            while (Mathf.Abs(_bms.BGMs.Top.Timing - Timer.PlayingTime) < 0.1f)
            {
                SoundManager.Instance.AddPlaySound(0, _bms.Bms.Head.WavFiles[_bms.BGMs.Top.Sound]);
                _bms.BGMs.Pop();
                Debug.Log("BGM");
            }
        }

        private IEnumerator LoadBms()
        {
            while (!BMSCapacity.Instance.IsDone)
                yield return null;

            _bms = BMSCapacity.Instance;
        }

        private static string GetHex(string str, int i) => Utility.Utility.GetHex(str, i);

        private static SortQueue<KeyValuePair<float, float>> MakeBpmList(List<string> bpmC, List<string> bpmExC,
            Dictionary<string, float> dic) => Utility.Utility.MakeBpmList(bpmC, bpmExC, dic);
        
        private static int Map(int val, int inMin, int inMax, int outMin, int outMax) =>
            Utility.Utility.Map(val, inMin, inMax, outMin, outMax);
    }
}
