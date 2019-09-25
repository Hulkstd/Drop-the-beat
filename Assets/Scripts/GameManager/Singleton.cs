﻿using UnityEngine;

namespace GameManager
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    var Object = GameObject.FindObjectsOfType(typeof(T));

                    if (Object.Length > 1)
                    {
                        Debug.LogError("Singleton cannot be applied to a multi-segmented class.");
                        return null;
                    }

                    _instance = (T)Object[0];
                    if(_instance == null)
                        Debug.LogError($"Class {typeof(T)} has not been created.");
                }

                return _instance;
            }
        }
        private static T _instance;
    }
}
