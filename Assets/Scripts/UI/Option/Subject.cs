using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using GameManager;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.Option
{
    public interface IInitialize
    {
        void Initialize();
    }

    public interface IFinalization
    {
        void Finalization();
    }
    
    public class Subject<T> : Singleton<T> where T : MonoBehaviour, IInitialize, IPointerDownHandler
    {
        [SerializeField] protected List<string> _subjectNames; 
        [SerializeField] protected List<MonoBehaviour> _subjectContents;
        [SerializeField] protected int _index;
        [SerializeField] protected Vector3 _origin;
        [SerializeField] protected Vector3 _offset;
        [SerializeField] protected GameObject _textPrefab;

        [SerializeField] private List<GameObject> _spawnObject = new List<GameObject>();
        
        public virtual int Index
        {
            get => _index;
            set
            {
                (_subjectContents[_index] as IFinalization)?.Finalization();
                _index = value;
                (_subjectContents[_index] as IInitialize)?.Initialize();
            }
        }

        public virtual void Initialize()
        {
            ResetSetting();

            var pos = _origin;

            foreach (var subject in _subjectNames)
            {
                var text = Instantiate(_textPrefab, transform);
                text.GetComponent<RectTransform>().anchoredPosition = pos;
                pos += _offset;
                _spawnObject.Add(text);
                
                text.GetComponent<TextMeshProUGUI>().text = subject;
            }
            Index = 0;
        }

        public virtual void ResetSetting()
        {
            _index = 0;
            _spawnObject.ForEach(Destroy);
            _spawnObject.Clear();
            _subjectContents.ForEach((x) => (x as IFinalization)?.Finalization());
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            if (!eventData.pointerCurrentRaycast.gameObject.CompareTag("SubjectText")) return;
            
            var title = eventData.pointerCurrentRaycast.gameObject.GetComponent<TextMeshProUGUI>().text;
            Index = FindIndex(title);
        }

        protected Vector3 CalculateOffset(int i)
        {
            return _origin + _offset * i;
        }

        private int FindIndex(string title)
            => _subjectNames.IndexOf(title);
        
        public KeyValuePair<string, IInitialize> this[int index] 
            => new KeyValuePair<string, IInitialize>(_subjectNames[index], _subjectContents[index] as IInitialize);
    }
}
