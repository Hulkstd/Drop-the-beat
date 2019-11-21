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
        [NonSerialized]public bool IsDone;
        [FormerlySerializedAs("IsStop")] public bool _isStop = false;
        [NonSerialized] public SortQueue<BPM> BPMs;
        [FormerlySerializedAs("ScrollTiming")] public double _scrollTiming;
        private BMSCapacity _bms;
        private float _offsetY = 3.302f;
        
        private void Start()
        {
            StartCoroutine(LoadBms());
        }

        private void Update()
        {
            if (!IsDone) return;
            //if (_isStop) return;

            double dt = Time.deltaTime;
            var mov = _bms.Bms.Head.Bpm * dt;

            if (BPMs.Length != 0 && BPMs.Top.Timing < _scrollTiming + dt)
            {
                mov = 0;
                var prev = _scrollTiming;
                var curBpm = _bms.Bms.Head.Bpm;
                
                while (BPMs.Length != 0 && BPMs.Top.Timing < _scrollTiming + dt)
                {
                    mov += curBpm * (BPMs.Top.Timing - prev);

                    prev = BPMs.Top.Timing;
                    curBpm = BPMs.Top.Bpm;

                    BPMs.Pop();
                }

                mov += curBpm * (_scrollTiming + dt - prev);
            }

            _offsetY += (float)mov / 60;
            _scrollTiming += dt;
            
            transform.position = Vector3.up * _offsetY + Vector3.back * 10;
        }

        private IEnumerator LoadBms()
        {
            yield return new WaitUntil(() => BMSCapacity.Instance.IsDone);
            
            _bms = BMSCapacity.Instance;
            BPMs = (SortQueue<BPM>)_bms.BPMs.Clone();
            
            while (!NodeCreator.Instance._doneLoading)
                yield return null;

            IsDone = true;
        }
    }
}
