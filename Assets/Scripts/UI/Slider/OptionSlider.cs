using System;
using System.Collections;
using GameManager;
using UI.Option;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Slider
{
    public class OptionSlider : SlideEvent<OptionSlider>, IPointerUpHandler, IPointerDownHandler
    {
        private const float CanvasGroupOpenAlpha = 0;
        private const float CanvasGroupCloseAlpha = 1;

        [SerializeField] private CanvasGroup _backGround;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Transform _audioTransform;
        [SerializeField] private Vector3 _audioPosWhenOptionOpen;
        [SerializeField] private Vector3 _audioPosWhenOptionClose;
        [SerializeField] private GraphicRaycaster _mainUIRaycaster;
        [SerializeField] private GraphicRaycaster _optionUIRaycaster;
        
        public void OpenOptionWindow()
        {
            _mainUIRaycaster.enabled = false;
            _optionUIRaycaster.enabled = true;
            OptionSubject.Instance.Initialize();
            StartCoroutine(AudioTransformMove(true));
            StartCoroutine(BackgroundAlphaMod(true));
            StartCoroutine(CanvasGroupAlpha(true));
        }

        public void CloseOptionWindow()
        {
            _mainUIRaycaster.enabled = true;
            _optionUIRaycaster.enabled = false;
            OptionSubject.Instance.ResetSetting();
            StartCoroutine(AudioTransformMove(false));
            StartCoroutine(BackgroundAlphaMod(false));
            StartCoroutine(CanvasGroupAlpha(false));
        }

        private void Update()
        {
            if(_optionUIRaycaster.enabled && Input.GetKeyDown(KeyCode.Escape))
                CloseOptionWindow();
        }

        private IEnumerator AudioTransformMove(bool isOpen)
        {    
            var from = isOpen ? _audioPosWhenOptionClose : _audioPosWhenOptionOpen;
            var to = !isOpen ? _audioPosWhenOptionClose : _audioPosWhenOptionOpen;

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
