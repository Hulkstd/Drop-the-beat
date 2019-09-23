using System;
using System.Collections;
using JetBrains.Annotations;
using UnityEngine;

namespace GameManager
{
    public class StaticClassCoroutineManager : Singleton<StaticClassCoroutineManager>
    {
        public void Coroutine(IEnumerator coroutine)
        {
            StartCoroutine(coroutine);
        }
    }
}
