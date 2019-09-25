using System;
using System.Collections;
using Node;
using UnityEngine;

namespace GameManager
{
    public class CameraMove : MonoBehaviour
    {
        private bool _isDone;
        private Bms _bms;
        private float _beat = 1;
        
        private void Start()
        {
            StartCoroutine(LoadBms());
        }

        private void FixedUpdate()
        {
            if (!_isDone) return;
            
            if(_bms.Data.GetCommandSection().ContainsKey(BMSCapacity.Instance._currentBar))
                if (_bms.Data.GetCommandSection()[BMSCapacity.Instance._currentBar].ContainsKey(Bms.DataSection.EventChannel.Measure))
                    float.TryParse(_bms.Data.GetCommandSection()[BMSCapacity.Instance._currentBar][Bms.DataSection.EventChannel.Measure][0], out _beat);
            transform.Translate(60 / (186 * 4 * _beat) * Time.fixedDeltaTime * NodeCreator.MeasureSpacing * Vector2.up);
        }

        private IEnumerator LoadBms()
        {
            while (!BMSCapacity.Instance.IsDone)
                yield return null;

            _bms = BMSCapacity.Instance.Bms;
            _isDone = true;
        }
    }
}
