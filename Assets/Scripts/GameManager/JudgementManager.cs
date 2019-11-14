using System;
using System.Collections.Generic;
using System.Linq;
using Node;
using UI.Animation;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace GameManager
{
    public class JudgementManager : Singleton<JudgementManager>
    {
//        public Dictionary<int, Utility.SortQueue<Node.NoteInfo>> Notes = (new int[] { 1, 2, 3, 4, 5, 6, 7 }).OfType<int>().
//            ToDictionary<int,int , Utility.SortQueue<Node.NoteInfo>>(x => x, x => new Utility.SortQueue<Node.NoteInfo>(NoteInfo.Comparision));

        public readonly Dictionary<int, KeyCode> InputKey = new Dictionary<int, KeyCode>();

        [FormerlySerializedAs("isAuto")] public bool _isAuto = false;
        private BMSCapacity _bms;

        private void Start()
        {
            KeyboardManager.LoadAll();
            _bms = BMSCapacity.Instance;
            for (var i = 1; i <= 7; i++)
            {
                InputKey.Add(i, KeyboardManager.GetKeyCode(i));
            }
        }
        
        private void Update()
        {
            if (!NodeCreator.Instance._doneLoading) return;
            if (!_isAuto) return;

            while (Judgement.Judge(_bms.Notes.Top.Timing) == JudgementText.Judgement.Excelent)
            {
                SoundManager.Instance.AddPlaySound(0, _bms.Bms.Head.WavFiles[_bms.Notes.Top.Sound]);
                
                Debug.Log(Mathf.Abs(Time.timeSinceLevelLoad - _bms.Notes.Top.Timing));
                Debug.Log("Judge");                
                _bms.Notes.Pop();
            }
//            foreach (var note in )
//            {
////                Debug.Log(Mathf.Abs(Time.timeSinceLevelLoad - note.Top._timing));
//                SoundManager.Instance.AddPlaySound(0, note.Top._audio);
//                
//                note.Pop();
//                Debug.Log("Judge");
////                Debug.Break();
//            }
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
            return timing < Time.timeSinceLevelLoad ? 
                UI.Animation.JudgementText.Judgement.Poor : UI.Animation.JudgementText.Judgement.Miss;
        }
    }
}
