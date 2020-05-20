using System;
using System.Collections;
using System.Collections.Generic;
using Action;
using GameManager.SaveData;
using Libplanet;
using LibplanetUnity;
using LibplanetUnity.Action;
using State;
using UI;
using UI.ListBox;
using UniRx.Async;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = System.Object;

namespace GameManager
{
    [RequireComponent(typeof(Canvas), typeof(Image))]
    public class SceneChanger : Singleton<SceneChanger>
    {
        private static Canvas _canvas;
        private static Image _curtain;
        private static readonly Color AlphaZero = new Color(0, 0, 0, 0);
        private static readonly Color AlphaOne = Color.black;
        public static string SongPath;
        public static bool AutoPlay;
        public static MusicList.MusicInfo.KeyStatus KeyStatus;
        public static Address Address;
        public static AgentState AgentState;

        protected override bool Awake()
        {
            if (!base.Awake())
            {
                FindObjectOfType<NameInputField>().SetActive(false);
                return false;
            }
            Agent.Initialize();
            var agent = Agent.instance;

            var state = agent.GetState(agent.Address);

            Address = agent.Address;
            AgentState = state is null
                ? new AgentState(agent.Address)
                : new AgentState((Bencodex.Types.Dictionary) state);
            FindObjectOfType<NameInputField>().SetActive(AgentState.Name == string.Empty);

            if (_canvas == null)
                _canvas = GetComponent<Canvas>();
            if (_curtain == null)
                _curtain = GetComponent<Image>();
            
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = short.MaxValue;
            _curtain.color = AlphaZero;
            DontDestroyOnLoad(gameObject);

            return true;
        }

        public void SetName(string nickname)
        {
            AgentState.Name = nickname;
            
            var list = new List<ActionBase>();
            list.Add
            (
                new Rename()
                {
                    Name = nickname,
                }
            );
            
            Agent.instance.MakeTransaction(list);
        }

        public void SceneChange(string sceneName, bool useFade)
        {
            if (useFade)
            {
                StartCoroutine(LoadSceneUseFade(sceneName));
            }
            else
            {
                SceneManager.LoadScene(sceneName);
            }
        }

        public void SceneChange(string sceneName, bool useFade, string songPath, bool isAuto, MusicList.MusicInfo.KeyStatus keyStatus)
        {
            SongPath = songPath;
            AutoPlay = isAuto;
            KeyStatus = keyStatus;
            
            if (useFade)
            {
                StartCoroutine(LoadSceneUseFade(sceneName));
            }
            else
            {
                SceneManager.LoadScene(sceneName);
            }
        }

        private static IEnumerator LoadSceneUseFade(string sceneName)
        {
            var routine = FadeOut();
            while (!routine.Current)
            {
                routine.MoveNext();
                yield return null;
            }
            
            SceneManager.LoadScene(sceneName);
            
            routine = FadeIn();
            while (!routine.Current)
            {
                routine.MoveNext();
                yield return null;
            }
        }

        private static IEnumerator<bool> FadeOut()
        {
            for (var i = 1; i <= 30; i++)
            {
                _curtain.color = Color.Lerp(AlphaZero, AlphaOne, i / 30.0f);
                yield return false;
            }

            yield return true;
        }
        
        private static IEnumerator<bool> FadeIn()
        {
            for (var i = 1; i <= 30; i++)
            {
                _curtain.color = Color.Lerp(AlphaOne, AlphaZero, i / 30.0f);
                yield return false;
            }

            yield return true;
        }
    }
}
