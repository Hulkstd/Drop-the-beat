using System;
using System.Collections;
using Node;
using UnityEngine;
using UnityEngine.Serialization;
using Utility;

namespace GameManager
{
    public class CameraMove : Singleton<CameraMove>
    {
        public double _scrollTiming;
        public bool _isStop = false;
        
        private bool _isDone;
        private SortQueue<BPM> _bpms;
        private BMSCapacity _bms;
        private float _offsetY = 3.302f;
        
        private void Start()
        {
            StartCoroutine(LoadBms());
        }

        private void Update()
        {
            if (!_isDone) return;
            //if (_isStop) return;

            double dt = Time.deltaTime;
            var mov = _bms.Bms.Head.Bpm * dt;

            if (_bpms.Length != 0 && _bpms.Top.Timing < _scrollTiming + dt)
            {
                mov = 0;
                var prev = _scrollTiming;
                var curBpm = _bms.Bms.Head.Bpm;
                
                while (_bpms.Length != 0 && _bpms.Top.Timing < _scrollTiming + dt)
                {
                    mov += curBpm * (_bpms.Top.Timing - prev);

                    prev = _bpms.Top.Timing;
                    curBpm = _bpms.Top.Bpm;

                    _bpms.Pop();
                }

                mov += curBpm * (_scrollTiming + dt - prev);
            }

            _offsetY += (float)mov / 60 * NodeCreator.Instance._noteSpeed;
            _scrollTiming += dt;
            
            transform.position = Vector3.up * _offsetY + Vector3.back * 10;
        }

        private IEnumerator LoadBms()
        {
            yield return new WaitUntil(() => BMSCapacity.Instance.IsDone);
            
            _bms = BMSCapacity.Instance;
            _bpms = (SortQueue<BPM>)_bms.BPMs.Clone();
            
            while (!NodeCreator.Instance._doneLoading)
                yield return null;

            _isDone = true;
        }
    }
}
