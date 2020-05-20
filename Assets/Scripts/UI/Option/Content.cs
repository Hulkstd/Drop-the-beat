using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using GameManager;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Option
{
    public class Content<T> : Singleton<T> where T : MonoBehaviour, IInitialize, IFinalization, IPointerDownHandler
    {
        [SerializeField] protected List<string> _contentNames;
        [SerializeField] protected List<ContentType> _contentTypes;
        [SerializeField] protected GameObject _textBoxPrefab;
        [SerializeField] protected GameObject _sliderPrefab;
        [SerializeField] protected Vector3 _origin;
        [SerializeField] protected Vector3 _offset;
        [SerializeField] protected string _lastInput;
        public bool _isClickInputWindow;
        public Graphic _selectedText;
        public bool _isShowing;

        [SerializeField] protected List<GameObject> _spawnObject = new List<GameObject>();

        protected enum ContentType
        {
            TextBox = 1,
            Slider = 2,
        }
        
        public virtual void Initialize()
        {
            _isShowing = true;
            PreDraw();
        }

        protected virtual void PreDraw()
        {
            _isClickInputWindow = false;
            _selectedText = null;
        }

        public virtual void Finalization()
        {
            _isShowing = false;
            _spawnObject.ForEach(Destroy);
            _spawnObject.Clear();
        }

        public virtual void OnPointerDown(PointerEventData pointerEventData)
        {
            if (!_isShowing) return;
            if (!pointerEventData.pointerCurrentRaycast.gameObject.CompareTag("InputBox")) return;

            _isClickInputWindow = true;
            _selectedText = pointerEventData.pointerCurrentRaycast.gameObject.GetComponent<TextMeshProUGUI>();
        }

        protected virtual void Update()
        {
            if (!_isClickInputWindow) return;
            if (!Input.anyKeyDown || Input.GetKeyDown(KeyCode.Mouse0)) return;

            _lastInput = Input.inputString.ToUpper()
                .Replace(" ", "Space")
                .Replace(";", "Semicolon");

            if (!Regex.IsMatch(_lastInput, "[A-Za-z]"))
                _lastInput = "";

            _isClickInputWindow = false;
        }
    }
}
