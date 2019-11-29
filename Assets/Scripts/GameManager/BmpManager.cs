using System;
using System.Collections;
using Node;
using UI.Animation;
using UnityEngine;
using UnityEngine.Video;

namespace GameManager
{
    public class BmpManager : Singleton<BmpManager>
    {
        private static BMSCapacity _bms;
        [SerializeField]private SpriteRenderer _sprite;
        [SerializeField]private VideoPlayer _videoPlayer;

        private void Start()
        {
            StartCoroutine(LoadBms());
        }

        private static IEnumerator LoadBms()
        {
            yield return new WaitUntil(() => BMSCapacity.Instance.IsDone);

            _bms = BMSCapacity.Instance;
        }

        private void Update()
        {
            if (!NodeCreator.Instance._doneLoading) return;
            
            while (_bms.Bmps.Length != 0 && Judgement.Judge((float)_bms.Bmps.Top.Timing) == JudgementText.Judgement.Excellent)
            {
                if (_bms.Bmps.Top.VideoClip != null)
                {
                    _videoPlayer.source = VideoSource.Url;
                    _videoPlayer.url = _bms.Bmps.Top.VideoClip;
                    _videoPlayer.Play();
                }
                else
                {
                    _sprite.sprite = _bms.Bmps.Top.Sprite;
                }
                
                _bms.Bmps.Pop();
            }
        }
    }
}
