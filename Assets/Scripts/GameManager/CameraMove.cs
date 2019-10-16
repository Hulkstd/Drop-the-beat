using System;
using System.Collections;
using Node;
using UnityEngine;

namespace GameManager
{
    public class CameraMove : MonoBehaviour
    {
        [NonSerialized]public bool IsDone;
        private BMSCapacity _bms;
        [SerializeField]private float _beat = 1;
        public float currentMeasureSpacing;
        public float currentBPM;
        public float calculateResult;
        public float moveDistance;
        public float totalTime;
        
        private void Start()
        {
            StartCoroutine(LoadBms());
        }

        private void FixedUpdate()
        {
            if (!IsDone) return;
            currentMeasureSpacing = NodeCreator.MeasureSpacing;
            currentBPM = _bms.Bms.Head.Bpm;
            calculateResult = (60 * (1f / _beat)) / ((1f / _beat) / 4 * _bms.Bms.Head.Bpm);
            moveDistance = (60 * (1f / _beat)) / ((1f / _beat) / 4 * _bms.Bms.Head.Bpm) * NodeCreator.MeasureSpacing;
            totalTime = calculateResult * _bms.Bms.Data.TotalBar;

            if (_bms.Bms.Data.GetCommandSection().ContainsKey(BMSCapacity.Instance._currentBar))
            {
                if (_bms.Bms.Data.GetCommandSection()[BMSCapacity.Instance._currentBar]
                    .ContainsKey(Bms.DataSection.EventChannel.Measure))
                {
                    float.TryParse(_bms.Bms.Data.GetCommandSection()[BMSCapacity.Instance._currentBar]
                        [Bms.DataSection.EventChannel.Measure][0], out _beat);

                    BMSCapacity.Instance._currentBeatWeight = _beat;
                }
            }
//            transform.Translate((60 * 1f / _beat) / (1f / _beat / 4 * _bms.Head.Bpm) * NodeCreator.MeasureSpacing *
//                                Time.fixedDeltaTime * Vector2.up);
            transform.position =
                (Time.timeSinceLevelLoad /
                 (60 * (1f / _beat) / ((1f / _beat) / 4 * _bms.Bms.Head.Bpm) * _bms.Bms.Data.TotalBar)) *
                NodeCreator.MeasureSpacing * _bms.Bms.Data.TotalBar * Vector3.up + Vector3.up * 3.302f + Vector3.back * 10;
        }

        private IEnumerator LoadBms()
        {
            while (!NodeCreator.Instance._doneLoading)
                yield return null;

            _bms = BMSCapacity.Instance;
            IsDone = true;
        }
    }
}
