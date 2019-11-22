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
        private static bool _bmsLoadingDone;

        private void Start()
        {
            StartCoroutine(LoadBms());
        }

        private IEnumerator LoadBms()
        {
            yield return new WaitUntil(() => BMSCapacity.Instance.IsDone);
            
            _bms = BMSCapacity.Instance;
            _bmsLoadingDone = true;
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

        private static int Map(int val, int inMin, int inMax, int outMin, int outMax) =>
            Utility.Utility.Map(val, inMin, inMax, outMin, outMax);

        private static Vector3 ToVec3(Vector2 pos)
        {
            return new Vector3(pos.x, pos.y);
        }

        private static int GetIndex(Bms.DataSection.EventChannel c)
        {
            switch (c)
            {
                case Bms.DataSection.EventChannel.P1SideLongNote1:
                case Bms.DataSection.EventChannel.P1SideKey1:
                    return 0;
                case Bms.DataSection.EventChannel.P1SideLongNote2:
                case Bms.DataSection.EventChannel.P1SideKey2:
                    return 1;
                case Bms.DataSection.EventChannel.P1SideLongNote3:
                case Bms.DataSection.EventChannel.P1SideKey3:
                    return 2;
                case Bms.DataSection.EventChannel.P1SideLongNote4:
                case Bms.DataSection.EventChannel.P1SideKey4:
                    return 3;
                case Bms.DataSection.EventChannel.P1SideLongNote5:
                case Bms.DataSection.EventChannel.P1SideKey5:
                    return 4;
                case Bms.DataSection.EventChannel.P1SideLongNote6:
                case Bms.DataSection.EventChannel.P1SideKey6:
                    return 5;
                case Bms.DataSection.EventChannel.P1SideLongNote7:
                case Bms.DataSection.EventChannel.P1SideKey7:
                    return 6;
                default:
                    return 0;
            }
        }

        private static string GetHex(string str, int i) => Utility.Utility.GetHex(str, i);
        
        private static SortQueue<KeyValuePair<float, float>> MakeBpmList(List<string> bpmC, List<string> bpmExC,
            Dictionary<string, float> dic) => Utility.Utility.MakeBpmList(bpmC, bpmExC, dic);
    }
}
