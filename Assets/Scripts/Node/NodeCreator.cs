using System;
using System.Collections;
using GameManager;
using UnityEngine;
using UnityEngine.Serialization;

namespace Node
{
    public class NodeCreator : MonoBehaviour
    {
        [FormerlySerializedAs("DoneLoading")] public bool _doneLoading;
        [FormerlySerializedAs("Note")] public NoteInfo _note;
        [FormerlySerializedAs("NoteBasePos")] public Transform[] _noteBasePos;
        private Bms _bms;
        public static float MeasureSpacing { get; private set; } = 4f;

        private void Start()
        {
            StartCoroutine(LoadBms());
        }

        private IEnumerator LoadBms()
        {
            while (!BMSCapacity.Instance.IsDone)
                yield return null;
                
            
            _bms = BMSCapacity.Instance.Bms;
            MeasureSpacing = _bms.Head.Bpm * 0.09f;
            
            yield return StartCoroutine(CreateNode());
            _doneLoading = true;
        }

        private IEnumerator CreateNode()
        {
            var nodes = _bms.Data.GetNoteSection();
            var command = _bms.Data.GetCommandSection();
            var count = 0;

            foreach (var measure in nodes)
            {
                foreach (var node in measure.Value)
                {
                    var pos = Vector2.zero;
                    
                    switch (node.Key)
                    {
                        case Bms.DataSection.EventChannel.P1SideKey1:
                            pos.x = _noteBasePos[0].position.x;
                            break;
                        
                        case Bms.DataSection.EventChannel.P1SideKey2:
                            pos.x = _noteBasePos[1].position.x;
                            break;
                        
                        case Bms.DataSection.EventChannel.P1SideKey3:
                            pos.x = _noteBasePos[2].position.x;
                            break;
                        
                        case Bms.DataSection.EventChannel.P1SideKey4:
                            pos.x = _noteBasePos[3].position.x;
                            break;
                        
                        case Bms.DataSection.EventChannel.P1SideKey5:
                            pos.x = _noteBasePos[4].position.x;
                            break;
                        
                        case Bms.DataSection.EventChannel.P1SideKey6:
                            pos.x = _noteBasePos[5].position.x;
                            break;
                        
                        case Bms.DataSection.EventChannel.P1SideKey7:
                            pos.x = _noteBasePos[6].position.x;
                            break;
                    }

                    foreach (var seq in node.Value)
                    {
                        var noteCount = seq.Length / 2;

                        for (var i = 0; i < noteCount; i++)
                        {
                            var hex = seq.Substring(i * 2, 2);

                            if (hex == "00")
                                continue;

                            var note = Instantiate(_note);
                            count++;
                            pos.y = MeasureSpacing / noteCount * i + measure.Key * MeasureSpacing;
                            note._audio = _bms.Head.WavFiles[hex];
                            note.transform.position = pos;
                        }
                            
                    }
                }
            }
            Debug.Log($"spawn {count} nodes");
            yield return null;
        }
    }
}
