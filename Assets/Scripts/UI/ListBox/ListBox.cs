using System;
using System.Collections.Generic;
using GameManager;
using UnityEngine;
using UnityEngine.UI;

namespace UI.ListBox
{
    public class ListBox<T1, T2, T3> : Singleton<T1> where T1 : MonoBehaviour where T3 : ListBox<T1, T2, T3>.ListBoxItem
    {
        [Serializable]
        public class ListBoxItem : MonoBehaviour
        {
        }
        
        [SerializeField] protected RectTransform _template;
        [SerializeField] protected List<T2> _list = new List<T2>();
        [Space]
        [SerializeField] protected GameObject _itemTransform;
        [SerializeField] protected Graphic _titleText;
        [SerializeField] protected Graphic _keyText;
        protected T3 Item;

        protected RectTransform ListBoxTransform;
        protected List<T3> Items = new List<T3>();
        
        public void AddOption(T2 value)
        {
            _list.Add(value);
            RefreshDropdown();
        }

        public void AddOptions(List<T2> values)
        {
            values.ForEach(_list.Add);
            RefreshDropdown();
        }

        public void RemoveOption(int index)
        {
            _list.RemoveAt(index);
            RefreshDropdown();
        }

        public void RemoveAll()
        {
            _list.Clear();
            RefreshDropdown();
        }
        
        protected virtual void Start()
        {
            _template.gameObject.SetActive(false);
        }

        protected virtual void RefreshDropdown()
        {
            if (ListBoxTransform != null)
            {
                Destroy(ListBoxTransform.gameObject);
                ListBoxTransform = null;
            }
            
            _itemTransform.SetActive(true);
            ListBoxTransform = Instantiate(_template.gameObject, _template.parent, false).transform as RectTransform;
            _itemTransform.SetActive(false);
            ListBoxTransform.gameObject.SetActive(true);
            ListBoxTransform.name = "Item List";
            Items.Clear();
        }
    }
}
