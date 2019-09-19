using System;
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
        protected TextMeshProUGUI textMesh;
        protected Text text;
        protected bool isDetectResultIsMesh = false;
        
        [FormerlySerializedAs("onTextChanged")]  [SerializeField]
        protected UnityEvent _onTextChanged;

        public UnityEvent OnTextChanged
        {
            get => _onTextChanged;
            set => _onTextChanged = value;
        }
        
        public string Text
        {
            get => isDetectResultIsMesh ? textMesh.text : text.text;
            set
            {
                if (textMesh == null && text == null)
                {
                    switch (_detectableTextClass)
                    {
                        case Text t:
                            text = t;
                            break;
                        case TextMeshProUGUI m:
                            textMesh = m;
                            isDetectResultIsMesh = true;
                            break;
                    }
                }

                if (isDetectResultIsMesh)
                {
                    textMesh.text = value;
                }
                else
                {
                    text.text = value;
                }
                _onTextChanged.Invoke();
            }
        }
    }
}
