using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameManager.SaveData
{
    public static class KeyboardManager
    {
        private abstract class Prefs
        {
            public static void SetKey(int i, KeyCode k) => PlayerPrefs.SetInt(i.ToString(), (int) k);

            public static int LoadKey(int i) => PlayerPrefs.GetInt(i.ToString(), -1);

            public static void Save() => PlayerPrefs.Save();
        }

        private static Dictionary<int, KeyCode> _keys;
        private static readonly Dictionary<int, KeyCode> Default = new Dictionary<int, KeyCode>()
        {
            {1, KeyCode.A},
            {2, KeyCode.S},
            {3, KeyCode.D},
            {4, KeyCode.Space},
            {5, KeyCode.K},
            {6, KeyCode.L},
            {7, KeyCode.Semicolon},
        };

        public static KeyCode GetKeyCode(int n) => GetKeys().ContainsKey(n) ? _keys[n] : LoadN(n);

        public static void SetKey(int n, KeyCode k)
        {
            if (GetKeys().ContainsKey(n))
            {
                _keys[n] = k;
            }
            else
            {
                _keys.Add(n, k);
            }

            Prefs.SetKey(n, k);
            Prefs.Save();
        }
        
        public static void ReplaceKey(KeyCode from, KeyCode to)
        {
            var index = 0;
            
            foreach (var keyCode in GetKeys().Where(keyCode => keyCode.Value == from))
            {
                index = keyCode.Key;
            }
            SetKey(index, to);
        }

        public static bool ContainKeyCode(KeyCode code)
            => GetKeys().ContainsValue(code);

        private static Dictionary<int, KeyCode> GetKeys() => _keys = _keys ?? new Dictionary<int, KeyCode>();

        private static KeyCode LoadN(int n)
        {
            var key = Prefs.LoadKey(n);
        
            if(key == -1)
                SetKey(n, Default[n]);
            else
                SetKey(n, (KeyCode)key);
        
            return (KeyCode)key;
        }
        public static void LoadAll()
        {
            for (var i = 1; i <= 7; i++)
                LoadN(i);
        }
    }
}
