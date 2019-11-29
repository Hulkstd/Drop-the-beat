using System;
using GameManager;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI.Animation
{
    public class TextChangedEvent<T> : Singleton<T> where T : MonoBehaviour
    {
        [FormerlySerializedAs("detectableTextClass")] [Tooltip("Only Support TextMesh and Text")] [SerializeField]
        protected MaskableGraphic _detectableTextClass;
        protected TextMeshProUGUI TextMesh;
        private Text _text;
        private bool _isDetectResultIsMesh = false;
        
        [FormerlySerializedAs("onTextChanged")] [SerializeField]
        protected UnityEvent _onTextChanged;

        public UnityEvent OnTextChanged
        {
            get => _onTextChanged;
            set => _onTextChanged = value;
        }
        
        protected string Text
        {
            get => _isDetectResultIsMesh ? TextMesh.text : _text.text;
            set
            {
                if (TextMesh == null && _text == null)
                {
                    switch (_detectableTextClass)
                    {
                        case Text t:
                            _text = t;
                            break;
                        case TextMeshProUGUI m:
                            TextMesh = m;
                            _isDetectResultIsMesh = true;
                            break;
                    }
                }

                if (_isDetectResultIsMesh)
                {
                    TextMesh.text = value;
                }
                else
                {
                    _text.text = value;
                }
                _onTextChanged.Invoke();
            }
        }
    }
}
