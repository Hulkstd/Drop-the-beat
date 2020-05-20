using System;
using System.Collections.Generic;
using System.Linq;
using GameManager.SaveData;
using Node;
using UI.Animation;
using UI.ListBox;
using UnityEngine;
using UnityEngine.Serialization;
using Utility;

namespace GameManager
{
    public class JudgementManager : Singleton<JudgementManager>
    {
        private readonly Dictionary<int, KeyCode> _inputKey = new Dictionary<int, KeyCode>();
        [FormerlySerializedAs("isAuto")] public bool _isAuto;
        [FormerlySerializedAs("AutoJudgement")] public JudgementText.Judgement _autoJudgement;
        private KeyStatus[] _keyStatuses;
        private BMSCapacity _bms;
        private IEnumerable<int> _saveCurrentKeyStatuses;
        private IEnumerable<int> _keyDownDetect;
        private IEnumerable<int> _keyPressDetect;
        private IEnumerable<int> _keyUpDetect;
        private readonly IEnumerable<Note>[] _keysNotes = new IEnumerable<Note>[7];
        private List<Note> _removeList;

        private delegate void UpdateData(JudgementText.Judgement judge);
        private static UpdateData _updateData;
        
        private enum KeyStatus
        {
            Up = 0,
            Press = 1,
            Down = 2,
        }
        
        private void Start()
        {
            KeyboardManager.LoadAll();
            _bms = BMSCapacity.Instance;

            if (SceneChanger.KeyStatus == MusicList.MusicInfo.KeyStatus.Key7)
            {
                for (var i = 1; i <= 7; i++)
                {
                    _inputKey.Add(i, KeyboardManager.GetKeyCode(i));
                }
            }
            else
            {
                for (var i = 2; i <= 6; i++)
                {
                    _inputKey.Add(i-1, KeyboardManager.GetKeyCode(i));
                }
            }

            _keyStatuses = new KeyStatus[_inputKey.Count];
            
            _keyDownDetect = _inputKey.Where(x => Input.GetKeyDown(x.Value)).Select(x => x.Key - 1);
            _keyPressDetect = _inputKey.Where(x => Input.GetKey(x.Value)).Select(x => x.Key - 1);
            _keyUpDetect = _inputKey.Where(x => (!Input.GetKeyDown(x.Value) && !Input.GetKey(x.Value)))
                .Select(x => x.Key - 1);
            _keysNotes[0] = _bms.Notes.Where(x => x.Index == 0);
            _keysNotes[1] = _bms.Notes.Where(x => x.Index == 1);
            _keysNotes[2] = _bms.Notes.Where(x => x.Index == 2);
            _keysNotes[3] = _bms.Notes.Where(x => x.Index == 3);
            _keysNotes[4] = _bms.Notes.Where(x => x.Index == 4);
            _keysNotes[5] = _bms.Notes.Where(x => x.Index == 5);
            _keysNotes[6] = _bms.Notes.Where(x => x.Index == 6);

                _removeList = new List<Note>();

            _updateData = null;
            _updateData += ScoreUpdate; 
            _updateData += CollectPercentText.Instance.UpdateData;

            _isAuto = SceneChanger.AutoPlay;
        }
        
        private void FixedUpdate()
        {
            if (!NodeCreator.Instance._doneLoading) return;
            if (!_isAuto) return;

            if (_bms.Notes.Length == 0)
                return;

            foreach (var keyNotes in _keysNotes.Select(x => x.ToList()))
            {
                foreach (var note in keyNotes.Where(x => Judgement.Judge((float)x.Timing, (float)x.ToTiming, x.IsLongNote) != JudgementText.Judgement.Ignore))
                {
                    if (note.IsLongNote && note.ToTiming < Timer.PlayingTime)
                    {
                        _removeList.Add(note);
                    }
                    else if (Judgement.Judge((float)note.Timing, (float)note.ToTiming, note.IsLongNote) == _autoJudgement)
                    {
                        _updateData(Judgement.Judge((float)note.Timing, (float)note.ToTiming, note.IsLongNote));
                        SoundManager.Instance.AddPlaySound(0, _bms.Bms.GetAudioClip(note.Sound));

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
                    _updateData(JudgementText.Judgement.Poor);
                    SoundManager.Instance.AddPlaySound(0, _bms.Bms.GetAudioClip(note.Sound), 30) ;
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

        private void Update()
        {
            if (!NodeCreator.Instance._doneLoading) return;
            if (_isAuto) return;
            
            foreach (var press in _keyPressDetect)
                _keyStatuses[press] = KeyStatus.Press;
            foreach (var down in _keyDownDetect)
                _keyStatuses[down] = KeyStatus.Down;
            foreach (var up in _keyUpDetect)
                _keyStatuses[up] = KeyStatus.Up;

            foreach (var keyNotes in _keysNotes.Select(x => x.ToList()))
            {
                foreach (var note in keyNotes.Where(x => Judgement.Judge((float)x.Timing, (float)x.ToTiming, x.IsLongNote) != JudgementText.Judgement.Ignore))
                {
                    if (note.IsLongNote)
                    {
                        if (note.Sound == "")
                        {
                            switch (_keyStatuses[note.Index])
                            {
                                case KeyStatus.Up:
                                    _removeList.Add(note);
                                    _updateData(JudgementText.Judgement.Poor);
                                    break;
                                case KeyStatus.Press:
                                    _updateData(JudgementText.Judgement.Excellent);
                                    break;
                            }
                        }
                        else
                        {
                            if (_keyStatuses[note.Index] == KeyStatus.Down)
                            {
                                _updateData(Judgement.Judge((float)note.Timing, (float)note.ToTiming, note.IsLongNote));
                                SoundManager.Instance.AddPlaySound(0, _bms.Bms.GetAudioClip(note.Sound));

                                note.Sound = "";
                            }
                        }
                    }
                    else
                    {
                        if (_keyStatuses[note.Index] == KeyStatus.Down)
                        {
                            _updateData(Judgement.Judge((float)note.Timing, (float)note.ToTiming, note.IsLongNote));
                            SoundManager.Instance.AddPlaySound(0, _bms.Bms.GetAudioClip(note.Sound));

                            _removeList.Add(note);
                        }
                    }
                    break;
                }

                foreach (var note in keyNotes.Where(x => Judgement.Judge((float)x.Timing, (float)x.ToTiming, x.IsLongNote) == JudgementText.Judgement.Poor))
                {
                    _updateData(JudgementText.Judgement.Poor);
                    SoundManager.Instance.AddPlaySound(0, _bms.Bms.GetAudioClip(note.Sound), 30);
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
                    break;
                case JudgementText.Judgement.Poor:
                    ComboText.Instance.ResetCombo();
                    break;
                case JudgementText.Judgement.Ignore:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(judge), judge, null);
            }

            JudgementText.Instance.Judge(judge);
            ComboText.Instance.GainCombo(judge != JudgementText.Judgement.Bad && 
                                         judge != JudgementText.Judgement.Poor ? 1 : 0);
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
                return JudgementText.Judgement.Excellent;
            if (d <= 60.0f)
                return JudgementText.Judgement.Great;
            if (d <= 150.0f)
                return JudgementText.Judgement.Good;
            if (d <= 220.0f)
                return JudgementText.Judgement.Bad;
            return timing < Timer.PlayingTime ? 
                JudgementText.Judgement.Poor : JudgementText.Judgement.Ignore;
        }
    }
}
