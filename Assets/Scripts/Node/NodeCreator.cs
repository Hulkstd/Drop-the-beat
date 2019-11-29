using System;
using System.Collections;
using System.Collections.Generic;
using GameManager;
using UnityEngine;
using UnityEngine.Serialization;
using Utility;

namespace Node
{
    public class NodeCreator : Singleton<NodeCreator>
    {
        [FormerlySerializedAs("DoneLoading")] public bool _doneLoading;
        [FormerlySerializedAs("Note")] public NoteInfo _note;
        [FormerlySerializedAs("NoteBasePos")] public Transform[] _noteBasePos;
        [FormerlySerializedAs("BeatLine")] public GameObject _beatLine;
        [FormerlySerializedAs("NoteSpeed")] public float _noteSpeed = 1;
        private static BMSCapacity _bms;

        private void Start()
        {
            StartCoroutine(LoadBms());
        }

        private IEnumerator LoadBms()
        {
            yield return new WaitUntil(() => BMSCapacity.Instance.IsDone);
            
            _bms = BMSCapacity.Instance;
            yield return StartCoroutine(CreateNode());
            _doneLoading = true;
        }

        private IEnumerator CreateNode()
        {
            foreach (var note in _bms.Notes)
            {
                var pos = _noteBasePos[note.Index].position;

                pos.y = (float)note.Beat;

                var n = Instantiate(_note, pos, Quaternion.Euler(0, 0, 0));
                n._timing = (float)note.Timing;
                n._audio = _bms.Bms.GetAudioClip(note.Sound);
                n._longNote = note.IsLongNote;

                if (!n._longNote) continue;
                
                var scale = n.transform.localScale;
                scale.y = (float)(note.ToTiming - note.Timing) * 0.2f;
                n.transform.localScale = scale;
            }

            yield return null;
        }
    }
}
