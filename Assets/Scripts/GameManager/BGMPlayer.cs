using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Boo.Lang;
using GameManager;
using UnityEngine;

namespace GameManager
{
    public class BGMPlayer : Singleton<BGMPlayer>
    {
        [NonSerialized] public bool IsDone;
        private BMSCapacity _bms;
        private float _secondPerBar;
        private int _prevBar = -1;

        private Dictionary<int, System.Collections.Generic.List<KeyValuePair<AudioClip, KeyValuePair<int, int>>>> _playList;
        
        private void Start()
        {
            StartCoroutine(LoadBms());
        }
        
        private IEnumerator LoadBms()
        {
            while (!BMSCapacity.Instance.IsDone)
                yield return null;

            _bms = BMSCapacity.Instance;
            _playList = new Dictionary<int, System.Collections.Generic.List<KeyValuePair<AudioClip, KeyValuePair<int, int>>>>();

            StartCoroutine(PreChange());
            StartCoroutine(PlayBGM());
        }

        private IEnumerator PreChange()
        {
            var data = _bms.Bms.Data.GetCommandSection();

            foreach (var measure in data)
            {
                foreach (var node in measure.Value)
                {
                    if(node.Key != Bms.DataSection.EventChannel.BackgroundMusic)
                        continue;

                    foreach (var command in node.Value)
                    {
                        var commandCount = command.Length / 2;

                        for (var i = 0; i < commandCount; i++)
                        {
                            var hex = command.Substring(i * 2, 2);

                            if (hex == "00")
                                continue;

                            if (!_playList.ContainsKey(measure.Key))
                                _playList.Add(measure.Key,
                                    new System.Collections.Generic.List<KeyValuePair<AudioClip, KeyValuePair<int, int>>>());
                            try
                            {
                                _playList[measure.Key]
                                    .Add(new KeyValuePair<AudioClip, KeyValuePair<int, int>>(_bms.Bms.Head.WavFiles[hex],
                                        new KeyValuePair<int, int>(i, commandCount)));
                            }
                            catch (Exception e)
                            {
                                Debug.Log(e.Message);
                                Debug.Log($"_playList.ContainsKey(measure.Key) ? {_playList.ContainsKey(measure.Key)}");
                                Debug.Log($"_playList[measure.Key] ? {_playList[measure.Key]}");
                            }
                        }
                    }
                }
            }
            
            yield return null;
        }

        private IEnumerator PlayBGM()
        {
            while (true)
            {
                yield return new WaitUntil(() =>  _prevBar != _bms._currentBar );
                _secondPerBar = (60 * (1f / _bms._currentBeatWeight)) / ((1f / _bms._currentBeatWeight) / 4 * _bms.Bms.Head.Bpm);
                _prevBar = _bms._currentBar;
                if (!_playList.ContainsKey(_prevBar)) continue;
                
                foreach(var command in _playList[_prevBar])
                {
                    //Debug.Log(_secondPerBar / command.Value.Value * command.Value.Key);
                    SoundManager.Instance.AddPlaySound((_secondPerBar / command.Value.Value) * command.Value.Key,
                        command.Key);
                }
            }

            yield return null;
        }
    }
}
