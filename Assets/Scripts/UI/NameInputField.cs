using System;
using System.Collections;
using GameManager;
using UnityEngine;

namespace UI
{
    public class NameInputField : MonoBehaviour
    {
        public GameObject canvas;

        public void SetActive(bool flag)
            => canvas.SetActive(flag);
    }
}
