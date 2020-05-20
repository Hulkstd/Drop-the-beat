using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using GameManager;
using GameManager;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;
using Object = UnityEngine.Object;

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
            yield return GCManager.Waitfor.ContainsKey(time + "wfs")
                ? (WaitForSeconds) GCManager.Waitfor[time + "wfs"]
                : (WaitForSeconds) GCManager.PushDataOnWaitfor(time + "wfs", new WaitForSeconds(time));

            obj.SetActive(false);
        }

        public static string GetHex(string str, int i) => str.Substring(i, 2);

        public static int Map(int val, int inMin, int inMax, int outMin, int outMax)
        {
            var a = val - inMin;
            var b = outMax - outMin;
            var c = inMax - inMin == 0 ? 1 : inMax - inMin;

            return a * b / c + outMin;
        }

        public static float Map(float val, float inMin, float inMax, float outMin, float outMax)
        {
            var a = val - inMin;
            var b = outMax - outMin;
            var c = inMax - inMin == 0 ? 1 : inMax - inMin;

            return a * b / c + outMin;
        }

        public static SortQueue<KeyValuePair<float, float>> MakeBpmList(List<string> bpmCs, List<string> bpmExCs,
            Dictionary<string, float> bpmDictionary)
        {
            var list = new SortQueue<KeyValuePair<float, float>>((a, b) => a.Value.CompareTo(b.Value));

            if (bpmCs != null)
            {
                foreach (var bpmC in bpmCs)
                {
                    var bpmCCount = bpmC.Length / 2;
                    for (var i = 0; i < bpmCCount; i++)
                    {
                        var hex = GetHex(bpmC, i * 2);

                        if (hex == "00")
                            continue;

                        list.Push(
                            new KeyValuePair<float, float>(Convert.ToInt32(hex, 16), (float) i / bpmCCount * 1000));
                    }
                }
            }

            if (bpmExCs != null)
            {
                foreach (var bpmExC in bpmExCs)
                {
                    var bpmExCCount = bpmExC.Length / 2;
                    for (var i = 0; i < bpmExCCount; i++)
                    {
                        var hex = GetHex(bpmExC, i * 2);

                        if (hex == "00")
                            continue;

                        list.Push(new KeyValuePair<float, float>(bpmDictionary[hex], (float) i / bpmExCCount * 1000));
                    }
                }
            }

            return list.Length == 0 ? null : list;
        }

        public static object GetValue<T>(int index, List<T> list)
        {
            if (index < 0)
                index = list.Count + index;

            if (index < 0)
                return null;
            if (list.Count <= index)
                return null;
            
            return list[index];

        }

    }

    public class SortQueue<T> : ICloneable, IEnumerable<T>
    {
        public readonly List<T> List;
        private readonly System.Comparison<T> _comp;

        public SortQueue()
        {
            List = new List<T>();
            _comp = null;
        }

        public SortQueue(System.Comparison<T> comparison)
        {
            List = new List<T>();
            _comp = comparison;
        }

        private SortQueue(List<T> list, System.Comparison<T> comparison = null)
        {
            List = list.Count != 0 ? list.GetRange(0, list.Count) : new List<T>();

            _comp = comparison;
        }

        public T Top => List[0];

        public int Length => List.Count;

        public void Push(T value)
        {
            List.Add(value);

            if (_comp == null)
                List.Sort();
            else
                List.Sort(_comp);
        }

        public T Pop()
        {
            if (Length <= 0) throw new System.Exception("SortQueue empty");
            var ret = Top;
            List.RemoveAt(0);

            return ret;
        }

        public void Pop(T val)
        {
            if(Length <= 0) throw new System.Exception("SortQueue empty");

            List.Remove(val);
        }

        public void Sort() => List.Sort();

        public T this[int index] => List[index];

        public object Clone()
        {
            return new SortQueue<T>(List, _comp);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return List.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class LObject : IComparable<LObject>, IEquatable<LObject>
    {
        public int Bar;
        public double Beat;
        public double Timing;

        public LObject()
        {
            Bar = 0;
            Beat = 0;
            Timing = 0;
        }

        protected LObject(int bar, double beat, double beatLen)
        {
            Bar = bar;
            Beat = beat / beatLen * 4f;
            Timing = 0;
        }

        public int CompareTo(LObject other)
        {
            return Beat.CompareTo(other.Beat);
        }

        public bool Equals(LObject other)
        {
            return other != null && (Bar == other.Bar && Beat == other.Beat && Timing == other.Timing);
        }
    }
    
    public class BPM : LObject
    {
        public double Bpm;

        public BPM(double bpm, int bar, double beat, double beatLen) : base(bar, beat, beatLen)
        {
            Bpm = bpm;
        }
    }

    public class Stop : LObject
    {
        public float Time;

        public Stop(int stop, int bar, double beat, double beatLen) : base(bar, beat, beatLen)
        {
            Time = (float)stop / 192;
        }
    }

    public class Note : LObject, IEquatable<Note>
    {
        public string Sound;
        public bool IsLongNote;
        public int Index;
        public int ToBar;
        public double ToBeat;
        public double ToTiming;
        
        public Note(string key, bool isLongNote, int index, int bar, double beat, double beatLen) : base(bar, beat, beatLen)
        {
            Sound = key;
            IsLongNote = isLongNote;
            Index = index;
        }

        public bool Equals(Note other)
        {
            return other != null && (Bar == other.Bar && Beat == other.Beat && Timing == other.Timing) &&
                   Sound == other.Sound && Index == other.Index;
        }
    }

    public class Bmp : LObject
    {
        public string VideoClip;
        public Sprite Sprite;

        public Bmp(string path, int bar, double beat, double beatLen) : base(bar, beat, beatLen)
        {
            if (path.EndsWith(".mpg"))
            {
                VideoClip = path;
            }
            else
            {
                BMSCapacity.Instance.StartCoroutine(ParseSprite(path, (x) => Sprite = x));
            }
        }

        private static IEnumerator ParseSprite(string path, Action<Sprite> action)
        {
            var www = UnityWebRequestTexture.GetTexture(path);

            yield return www.SendWebRequest();

            action(Sprite.Create(DownloadHandlerTexture.GetContent(www), new Rect(), Vector2.one * 0.5f));
        }
    }
}
