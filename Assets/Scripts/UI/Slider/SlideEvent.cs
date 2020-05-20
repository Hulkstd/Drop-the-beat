using System.Collections;
using GameManager;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace UI.Slider
{
    public class SlideEvent<T> : Singleton<T> where T : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
    {
        [SerializeField] [FormerlySerializedAs("Slider")] protected RectTransform _slider;
        [SerializeField] [FormerlySerializedAs("EventTriggerPos")] protected Vector3 _eventTriggerPos;
        [FormerlySerializedAs("OnSliderOverPos")] public UnityEvent _onSliderOverPos;
        [SerializeField] private bool _isPointerDown;
        [SerializeField] private bool _isCoolTime;

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            if (_isCoolTime) return;
            if (eventData.button != PointerEventData.InputButton.Left) return;
            
            _isPointerDown = true;
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            if (_isCoolTime) return;
            if (!_isPointerDown) return;
            if (eventData.button != PointerEventData.InputButton.Left) return; 
            _isPointerDown = false;
            _isCoolTime = true;
            
            if (_eventTriggerPos.x < _slider.anchoredPosition.x)
                _onSliderOverPos.Invoke();
            StartCoroutine(BackToOrigin());
        }

        private void FixedUpdate()
        {
            if (!_isPointerDown)
                return;

            var sliderPosition = _slider.position;
            sliderPosition.x = Input.mousePosition.x;
            _slider.position = sliderPosition;
        }

        private IEnumerator BackToOrigin()
        {
            var from = _slider.anchoredPosition;
            var to = from;
            to.x = 20;

            for (var i = 0; i <= 30; i++)
            {
                _slider.anchoredPosition = Vector3.Lerp(from, to, i / 30.0f);
                yield return null;
            }
            _slider.anchoredPosition = to;
            _isCoolTime = false;
            
            yield return null;
        }
    }
}
