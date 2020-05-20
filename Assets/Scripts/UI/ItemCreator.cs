using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using GameManager;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI
{
    [ExecuteInEditMode]
    public class ItemCreator : Singleton<ItemCreator>
    {
        [Range(0, 360)] public int _itemCount;
        [SerializeField] private GameObject _prefab;
        [SerializeField] private Vector3 _offset;
        
        private static int _prevItemCount = 0;
        private static readonly List<GameObject> PrevItemPrefab = new List<GameObject>();

        public void CreateObject(int itemCount)
        {
            _itemCount = itemCount;
            Initialize();
        }

        private void Initialize()
        {
            if (!_prefab) return;
            if (_itemCount == _prevItemCount) return;
            _prevItemCount = _itemCount;

            PrevItemPrefab.ForEach((obj) =>
            {
                if(obj != null)
                    DestroyImmediate(obj);
            });
            PrevItemPrefab.Clear();

            for (var i = 0; i < _itemCount; i++)
            {
                transform.rotation = Quaternion.Euler(0, 0, 360f / _itemCount * i);

                var obj = Instantiate(_prefab, transform.parent);
                obj.GetComponent<RectTransform>().anchoredPosition = _offset;
                obj.transform.SetParent(transform);
                PrevItemPrefab.Add(obj);
            }

            transform.rotation = Quaternion.Euler(0, 0, 0);
        }

#if UNITY_EDITOR

        private void Update()
        {
            Initialize();
        }

        private void OnDisable()  
        {
            PrevItemPrefab.ForEach(DestroyImmediate);
            PrevItemPrefab.Clear();
        }

        private void OnApplicationQuit()
        {
            PrevItemPrefab.ForEach(DestroyImmediate);
            PrevItemPrefab.Clear();
        }
#endif
        public static void CopyList(ref List<GameObject> list)
            => PrevItemPrefab.ForEach(list.Add);
    }
}
