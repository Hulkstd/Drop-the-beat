using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Node;
using TMPro;
using UI.Animation;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.SocialPlatforms.Impl;
using Utility;

namespace GameManager
{
    public class JudgementManager : Singleton<JudgementManager>
    {
//        public Dictionary<int, Utility.SortQueue<Node.NoteInfo>> Notes = (new int[] { 1, 2, 3, 4, 5, 6, 7 }).OfType<int>().
//            ToDictionary<int,int , Utility.SortQueue<Node.NoteInfo>>(x => x, x => new Utility.SortQueue<Node.NoteInfo>(NoteInfo.Comparision));

        private readonly Dictionary<int, KeyCode> _inputKey = new Dictionary<int, KeyCode>();
        [FormerlySerializedAs("isAuto")] public bool _isAuto = false;
        private KeyStatus[] _keyStatuses;
        private KeyStatus[] _prevKeyStatuses;
        private BMSCapacity _bms;
        private IEnumerable<int> _saveCurrentKeyStatuses;
        private IEnumerable<int> _keyDownDetect;
        private IEnumerable<int> _keyPressDetect;
        private IEnumerable<int> _keyUpDetect;
        private IEnumerable<Note>[] _keysNotes = new IEnumerable<Note>[7];
        private List<Note> _removeList;

        private enum KeyStatus
        {
            Up = 0,
            Press = 1,
            Down = 2,
        }
        
        private void Start()
        {
            KeyboardManager.LoadAll();
            _keyStatuses = new KeyStatus[_inputKey.Count];
            _bms = BMSCapacity.Instance;
            for (var i = 1; i <= 7; i++)
            {
                _inputKey.Add(i, KeyboardManager.GetKeyCode(i));
                Debug.Log(KeyboardManager.GetKeyCode(i));
            }
            
            _keyDownDetect = _inputKey.Where(x => Input.GetKeyDown(x.Value)).Select(x => x.Key);
            _keyPressDetect = _inputKey.Where(x => Input.GetKey(x.Value)).Select(x => x.Key);
            _keyUpDetect = _inputKey.Where(x => Input.GetKeyUp(x.Value)).Select(x => x.Key);
            _keysNotes[0] = _bms.Notes.Where(x => x.Index == 0);
            _keysNotes[1] = _bms.Notes.Where(x => x.Index == 1);
            _keysNotes[2] = _bms.Notes.Where(x => x.Index == 2);
            _keysNotes[3] = _bms.Notes.Where(x => x.Index == 3);
            _keysNotes[4] = _bms.Notes.Where(x => x.Index == 4);
            _keysNotes[5] = _bms.Notes.Where(x => x.Index == 5);
            _keysNotes[6] = _bms.Notes.Where(x => x.Index == 6);
            _removeList = new List<Note>();
        }
        
        private void Update()
        {
            if (!NodeCreator.Instance._doneLoading) return;
            if (!_isAuto) return;
            
            if (_bms.Notes.Length == 0)
                return;
            
            foreach (var keyNotes in _keysNotes)
            {
                foreach (var note in keyNotes.Where(x => Judgement.Judge((float)x.Timing, (float)x.ToTiming, x.IsLongNote) != JudgementText.Judgement.Ignore))
                {
                    if (note.IsLongNote && note.ToTiming < Timer.PlayingTime)
                    {
                        _removeList.Add(note);
                    }
                    else if (Judgement.Judge((float)note.Timing, (float)note.ToTiming, note.IsLongNote) == JudgementText.Judgement.Excellent)
                    {
                        ScoreUpdate(Judgement.Judge((float)note.Timing, (float)note.ToTiming, note.IsLongNote));
                        SoundManager.Instance.AddPlaySound(0, _bms.Bms.GetAudioClip(_bms.Notes.Top.Sound));

                        if (!note.IsLongNote)
                            _removeList.Add(note);
                        else
                        {
                            note.Sound = "";
                        }
                    }
                    break;
                }

                foreach (var note in keyNotes.Where(x => Judgement.Judge((float)x.Timing, (float)x.ToTiming, x.IsLongNote) == JudgementText.Judgement.Poor))
                {
                    ScoreUpdate(JudgementText.Judgement.Poor);
                    _removeList.Add(note);
                }
                
                if (_removeList.Count == 0) continue;

                foreach (var note in _removeList)
                {
                    _bms.Notes.Pop(note);
                }
                _removeList.Clear();
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
                foreach (var note in keyNotes.Where(x =>
                    Judgement.Judge((float) x.Timing) != JudgementText.Judgement.Ignore))
                {
                    if (note.IsLongNote && note.Timing < Timer.PlayingTime && note.ToTiming > Timer.PlayingTime &&
                        _keyStatuses[note.Index] == KeyStatus.Press && _prevKeyStatuses[note.Index] != KeyStatus.Up)
                    {
                        ScoreUpdate(JudgementText.Judgement.Excellent);
                    }
                    else if (_keyStatuses[note.Index] == KeyStatus.Down)
                    {
                        ScoreUpdate(Judgement.Judge((float) note.Timing));
                        SoundManager.Instance.AddPlaySound(0, _bms.Bms.GetAudioClip(_bms.Notes.Top.Sound));
                    }

                    break;
                }

                foreach (var note in keyNotes.Where(x =>
                    Judgement.Judge((float) x.Timing) == JudgementText.Judgement.Poor))
                {
                    _removeList.Add(note);
                }

                if (_removeList.Count == 0) continue;
                
                foreach (var note in _removeList)
                {
                    _bms.Notes.Pop(note);
                }

                _removeList.Clear();
                
            }
        }
        
        private void ScoreUpdate(UI.Animation.JudgementText.Judgement judge)
        {
            switch (judge)
            {
                case JudgementText.Judgement.Excellent:
                    ScoreText.Instance.GainScore(5);
                    break;
                case JudgementText.Judgement.Great:
                    ScoreText.Instance.GainScore(3);
                    break;
                case JudgementText.Judgement.Good:
                    ScoreText.Instance.GainScore(1);
                    break;
                case JudgementText.Judgement.Bad:
                    ComboText.Instance.ResetCombo();
                    JudgementText.Instance.Judge(judge);
                    return;
                case JudgementText.Judgement.Poor:
                    ComboText.Instance.ResetCombo();
                    JudgementText.Instance.Judge(judge);
                    return;
                case JudgementText.Judgement.Ignore:
                    
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(judge), judge, null);
            }

            JudgementText.Instance.Judge(judge);
            ComboText.Instance.GainCombo(1);
        }
    }

    public static class Judgement
    {
        public static UI.Animation.JudgementText.Judgement Judge(float timing, float toTiming = 0.0f, bool isLongNote = false)
        {
            var d = Math.Abs(timing - Timer.PlayingTime) * 1000;

            if (isLongNote && timing < Timer.PlayingTime)
            {
                return JudgementText.Judgement.Excellent;
            }

            if (d <= 21.0f)
                return UI.Animation.JudgementText.Judgement.Excellent;
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
