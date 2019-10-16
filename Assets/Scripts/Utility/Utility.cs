using System.Collections;
using System.Collections.Generic;
using GameManager;
using UnityEngine;

namespace Utility
{
    public class ObjectPooling<T> where T : Component
    {
        private readonly T _originalPrefabs;
        private readonly Transform _parent;
        private readonly Queue<T> _objects;
        public ObjectPooling(T prefabs, Transform parent = null)
        {
            _originalPrefabs = prefabs;
            _parent = parent;
            _objects = new Queue<T>();
        }

        public GameObject PopGameObject()
        {
            if (_objects.Count == 0 || _objects.Peek().gameObject.activeSelf)
            {
                var returnObject = Object.Instantiate(_originalPrefabs);
                if(_parent)
                    returnObject.transform.SetParent(_parent);
                _objects.Enqueue(returnObject);

                return returnObject.gameObject;
            }

            var returnValue = _objects.Peek();
            _objects.Enqueue(_objects.Dequeue());

            return returnValue.gameObject;
        }
        
        public T PopObject() 
        {
            if (_objects.Count == 0 || _objects.Peek().gameObject.activeSelf)
            {
                var returnObject = Object.Instantiate(_originalPrefabs);          
                if(_parent)
                    returnObject.transform.SetParent(_parent);
                _objects.Enqueue(returnObject);
                if(_objects.Count != 0) _objects.Enqueue(_objects.Dequeue());

                return returnObject;
            }
            
            var returnValue = _objects.Peek();
            _objects.Enqueue(_objects.Dequeue());
            returnValue.gameObject.SetActive(true);

            return returnValue;
        }
    }

    public static class Utility
    {
        public static IEnumerator SetActive(float time, GameObject obj)
        {
            yield return GCManager.Instance.Waitfor.ContainsKey(time)
                ? (WaitForSeconds) GCManager.Instance.Waitfor[time]
                : (WaitForSeconds) GCManager.Instance.PushDataOnWaitfor(time, new WaitForSeconds(time));
            
            obj.SetActive(false);
        }
    }

    public class SortQueue<T>
    {
        private readonly List<T> _list;
        private readonly System.Comparison<T> _comp;

        public SortQueue()
        {
            _list = new List<T>();
            _comp = null;
        }
        public SortQueue(System.Comparison<T> comparison)
        {
            _list = new List<T>();
            _comp = comparison;
        }

        public int Count => _list.Count;

        public T Top => _list[0];

        public int Length => _list.Count;

        public void Push(T value)
        {
            _list.Add(value);

            if (_comp == null)
                _list.Sort();
            else
                _list.Sort(_comp);
        }

        public T Pop()
        {
            if (Count <= 0) throw new System.Exception("SortQueue empty");
            T ret = Top;
            _list.RemoveAt(0);

            return ret;
        }
    }
}
