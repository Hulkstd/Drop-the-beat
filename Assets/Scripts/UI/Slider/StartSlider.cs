using System.Collections;
using GameManager;
using UI.Option;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI.Slider
{
    public class StartSlider : SlideEvent<StartSlider>, IPointerUpHandler, IPointerDownHandler
    {        
        private const float CanvasGroupOpenAlpha = 0;
        private const float CanvasGroupCloseAlpha = 1;

        [SerializeField] private CanvasGroup _backGround;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Transform _audioTransform;
        [SerializeField] private Vector3 _audioPosWhenSelectMusicOpen;
        [SerializeField] private Vector3 _audioPosWhenSelectMusicClose;
        [SerializeField] private GraphicRaycaster _mainUIRaycaster;
        [SerializeField] private GraphicRaycaster _selectMusicUIRaycaster;

        public void OpenSelectMusicWindow()
        {
            _mainUIRaycaster.enabled = false;
            _selectMusicUIRaycaster.enabled = true;
            OptionSubject.Instance.Initialize();
            StartCoroutine(AudioTransformMove(true));
            StartCoroutine(BackgroundAlphaMod(true));
            StartCoroutine(CanvasGroupAlpha(true));
        }

        public void CloseSelectMusicWindow()
        {
            _mainUIRaycaster.enabled = true;
            _selectMusicUIRaycaster.enabled = false;
            OptionSubject.Instance.ResetSetting();
            StartCoroutine(AudioTransformMove(false));
            StartCoroutine(BackgroundAlphaMod(false));
            StartCoroutine(CanvasGroupAlpha(false));
        }
        
        private void Update()
        {
            if(!MusicManager.Instance._enterDirectory && _selectMusicUIRaycaster.enabled && Input.GetKeyDown(KeyCode.Escape))
                CloseSelectMusicWindow();
        }

        private IEnumerator AudioTransformMove(bool isOpen)
        {
            var from = isOpen ? _audioPosWhenSelectMusicClose : _audioPosWhenSelectMusicOpen;
            var to = !isOpen ? _audioPosWhenSelectMusicClose : _audioPosWhenSelectMusicOpen;

            for (var i = 1; i <= 10; i++)
            {
                _audioTransform.position = Vector3.Lerp(from, to, i / 10.0f);
                yield return null;
            }

            _audioTransform.position = to;
            yield return null;
        }
        
        private IEnumerator BackgroundAlphaMod(bool isOpen)
        {
            var from = isOpen ? CanvasGroupOpenAlpha : CanvasGroupCloseAlpha;
            var to = !isOpen ? CanvasGroupOpenAlpha : CanvasGroupCloseAlpha;

            for (var i = 1; i <= 19; i++)
            {
                _backGround.alpha = Mathf.Lerp(from, to, i / 19.0f);
                yield return null;
            }

            _backGround.alpha = to;
            yield return null;
        }
        
        private IEnumerator CanvasGroupAlpha(bool isOpen)
        {
            var from = isOpen ? CanvasGroupCloseAlpha : CanvasGroupOpenAlpha;
            var to = !isOpen ? CanvasGroupCloseAlpha : CanvasGroupOpenAlpha;

            for (var i = 1; i <= 10; i++)
            {
                _canvasGroup.alpha = Mathf.Lerp(from, to, i / 10.0f);
                yield return null;
            }

            _canvasGroup.alpha = to;
            yield return null;
        }
    }
}
