using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Node;
using TMPro;
using UI.Animation;
using UnityEngine;
using UnityEngine.Serialization;
using Utility;

namespace GameManager
{
    public class JudgementManager : Singleton<JudgementManager>
    {
//        public Dictionary<int, Utility.SortQueue<Node.NoteInfo>> Notes = (new int[] { 1, 2, 3, 4, 5, 6, 7 }).OfType<int>().
//            ToDictionary<int,int , Utility.SortQueue<Node.NoteInfo>>(x => x, x => new Utility.SortQueue<Node.NoteInfo>(NoteInfo.Comparision));

        public readonly Dictionary<int, KeyCode> InputKey = new Dictionary<int, KeyCode>();
        [FormerlySerializedAs("isAuto")] public bool _isAuto = false;
        [FormerlySerializedAs("ScoreText")] public TextMeshProUGUI _scoreText;
        [FormerlySerializedAs("Score")] public long _score;
        private KeyStatus[] _keyStatuses;
        private KeyStatus[] _prevKeyStatuses;
        private BMSCapacity _bms;
        private IEnumerable<int> _saveCurrentKeyStatuses;
        private IEnumerable<int> _keyDownDetect;
        private IEnumerable<int> _keyPressDetect;
        private IEnumerable<int> _keyUpDetect;
        private IEnumerable<Note>[] _keysNotes = new IEnumerable<Note>[7];

        private enum KeyStatus
        {
            Up = 0,
            Press = 1,
            Down = 2,
        }
        
        private void Start()
        {
            KeyboardManager.LoadAll();
            _keyStatuses = new KeyStatus[InputKey.Count];
            _bms = BMSCapacity.Instance;
            for (var i = 1; i <= 7; i++)
            {
                InputKey.Add(i, KeyboardManager.GetKeyCode(i));
                Debug.Log(KeyboardManager.GetKeyCode(i));
            }
            
            _keyDownDetect = InputKey.Where(x => Input.GetKeyDown(x.Value)).Select(x => x.Key);
            _keyPressDetect = InputKey.Where(x => Input.GetKey(x.Value)).Select(x => x.Key);
            _keyUpDetect = InputKey.Where(x => Input.GetKeyUp(x.Value)).Select(x => x.Key);
            _keysNotes[0] = _bms.Notes.Where(x => x.Index == 0);
            _keysNotes[1] = _bms.Notes.Where(x => x.Index == 1);
            _keysNotes[2] = _bms.Notes.Where(x => x.Index == 2);
            _keysNotes[3] = _bms.Notes.Where(x => x.Index == 3);
            _keysNotes[4] = _bms.Notes.Where(x => x.Index == 4);
            _keysNotes[5] = _bms.Notes.Where(x => x.Index == 5);
            _keysNotes[6] = _bms.Notes.Where(x => x.Index == 6);
        }
        
        private void Update()
        {
            if (!NodeCreator.Instance._doneLoading) return;
            if (!_isAuto) return;
            
            if (_bms.Notes.Length == 0)
                return;

            while (_bms.Notes.Length != 0 && Judgement.Judge((float)_bms.Notes.Top.Timing) == JudgementText.Judgement.Excelent)
            {
                ScoreUpdate(Judgement.Judge((float)_bms.Notes.Top.Timing));
                SoundManager.Instance.AddPlaySound(0, _bms.Bms.GetAudioClip(_bms.Notes.Top.Sound));
                
//                Debug.Log($"Judge {Mathf.Abs(Timer.PlayingTime - (float)_bms.Notes.Top.Timing)}");     
//                Debug.Break();
                _bms.Notes.Pop();
            }
        }

        private void FixedUpdate()
        {
            if (!NodeCreator.Instance._doneLoading) return;
            if (_isAuto) return;

            for (var i = 0; i < _keyStatuses.Length; i++)
            {
                _prevKeyStatuses[i] = _keyStatuses[i];
            }
            foreach (var down in _keyDownDetect)
                _keyStatuses[down] = KeyStatus.Down;
            foreach (var press in _keyPressDetect)
                _keyStatuses[press] = KeyStatus.Press;
            foreach (var up in _keyUpDetect)
                _keyStatuses[up] = KeyStatus.Up;

            foreach (var keyNotes in _keysNotes)
            {
                foreach (var note in keyNotes.Where(x => Judgement.Judge((float)x.Timing) != JudgementText.Judgement.Ignore))
                {
                    if (_keyStatuses[note.Index] == KeyStatus.Down)
                    {
                        ScoreUpdate(Judgement.Judge((float)note.Timing));
                        SoundManager.Instance.AddPlaySound(0, _bms.Bms.GetAudioClip(_bms.Notes.Top.Sound));
                    }
                    break;
                }
            }
        }
        
        private void ScoreUpdate(UI.Animation.JudgementText.Judgement judge)
        {
            switch (judge)
            {
                case JudgementText.Judgement.Excelent:
                    _score += 5;
                    break;
                case JudgementText.Judgement.Great:
                    _score += 3;
                    break;
                case JudgementText.Judgement.Good:
                    _score += 1;
                    break;
                case JudgementText.Judgement.Bad:
                    break;
                case JudgementText.Judgement.Poor:
                    
                    break;
                case JudgementText.Judgement.Ignore:
                    
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(judge), judge, null);
            }

            _scoreText.text = _score.ToString("D9");
            JudgementText.Instance.Judge(judge);
            ComboText.Instance.GainCombo(1);
        }
    }

    public static class Judgement
    {
        public static UI.Animation.JudgementText.Judgement Judge(float timing)
        {
            var d = Math.Abs(timing - Timer.PlayingTime) * 1000;

            if (d <= 21.0f)
                return UI.Animation.JudgementText.Judgement.Excelent;
            if (d <= 60.0f)
                return UI.Animation.JudgementText.Judgement.Great;
            if (d <= 150.0f)
                return UI.Animation.JudgementText.Judgement.Good;
            if (d <= 220.0f)
                return UI.Animation.JudgementText.Judgement.Bad;
            return timing < Timer.PlayingTime ? 
                UI.Animation.JudgementText.Judgement.Poor : UI.Animation.JudgementText.Judgement.Ignore;
        }
    }
}
