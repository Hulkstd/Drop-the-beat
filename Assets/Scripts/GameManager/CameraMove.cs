using System;
using System.Collections;
using Node;
using UnityEngine;
using UnityEngine.Serialization;

namespace GameManager
{
    public class CameraMove : Singleton<CameraMove>
    {
        [NonSerialized]public bool IsDone;
        [FormerlySerializedAs("IsStop")] public bool _isStop = false;
        private BMSCapacity _bms;
        private float _offsetY;
        
        private void Start()
        {
            StartCoroutine(LoadBms());
        }

        private void Update()
        {
            if (!IsDone) return;
            //if (_isStop) return;
            
            
        }

        private IEnumerator LoadBms()
        {
            while (!NodeCreator.Instance._doneLoading)
                yield return null;

            _bms = BMSCapacity.Instance;
        }
    }
}
