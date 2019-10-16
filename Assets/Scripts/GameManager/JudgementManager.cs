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
        public Dictionary<int, Utility.SortQueue<Node.NoteInfo>> Notes = (new int[] { 1, 2, 3, 4, 5, 6, 7 }).OfType<int>().
            ToDictionary<int,int , Utility.SortQueue<Node.NoteInfo>>(x => x, x => new Utility.SortQueue<Node.NoteInfo>(NoteInfo.Comparision));

        public readonly Dictionary<int, KeyCode> InputKey = new Dictionary<int, KeyCode>();

        [FormerlySerializedAs("isAuto")] public bool _isAuto = false;

        private void Start()
        {
            KeyboardManager.LoadAll();

            for (var i = 1; i <= 7; i++)
            {
                InputKey.Add(i, KeyboardManager.GetKeyCode(i));
            }
        }
        
        private void Update()
        {
            if (!NodeCreator.Instance._doneLoading) return;
            if (!_isAuto) return;

            foreach (var note in Notes.Values.Where(note => note.Length != 0 && Judgement.Judge(note.Top._timing) == JudgementText.Judgement.Excelent))
            {
                //Debug.Log(Mathf.Abs(Time.timeSinceLevelLoad - note.Top._timing));
                SoundManager.Instance.AddPlaySound(0, note.Top._audio);
                note.Pop();
            }
        }
    }

    public static class Judgement
    {
        public static UI.Animation.JudgementText.Judgement Judge(float timing)
        {
            var d = Math.Abs(timing - Time.timeSinceLevelLoad) * 1000;

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
