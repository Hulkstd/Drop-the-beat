using System;
using System.Collections;
using GameManager;
using UnityEngine;
using UnityEngine.Serialization;

namespace Node
{
    public class NodeCreator : Singleton<NodeCreator>
    {
        [FormerlySerializedAs("DoneLoading")] public bool _doneLoading;
        [FormerlySerializedAs("Note")] public NoteInfo _note;
        [FormerlySerializedAs("NoteBasePos")] public Transform[] _noteBasePos;
        private static Bms _bms;
        private static bool _bmsLoadingDone;

        public static float MeasureSpacing => _bmsLoadingDone ? 
            BMSCapacity.Instance._currentBeatWeight * _bms.Head.Bpm * 0.09f : 0;

        private void Start()
        {
            StartCoroutine(LoadBms());
        }

        private IEnumerator LoadBms()
        {
            yield return new WaitUntil(() => BMSCapacity.Instance.IsDone);
            
            _bms = BMSCapacity.Instance.Bms;
            _bmsLoadingDone = true;
            yield return StartCoroutine(CreateNode());
            _doneLoading = true;
        }

        private IEnumerator CreateNode()
        {
            var nodes = _bms.Data.GetNoteSection();
            var command = _bms.Data.GetCommandSection();
            var judge = JudgementManager.Instance;
            var count = 0;
            var bpm = _bms.Head.Bpm;
            var offsetY = 0f;
            var timingOffset = 0f;
            var beat = 0f;
            var currentMeasure = 0;

            var lastNote = new NoteInfo[7];
            
            foreach (var measure in nodes)
            {
                var bpmEx = "";

                if(currentMeasure < measure.Key)
                {
                    for (var i = currentMeasure; i < measure.Key; i++)
                    {
                        if (!command.ContainsKey(i)) continue;
                        
                        if (command[i].ContainsKey(Bms.DataSection.EventChannel.Measure))
                        {
                            beat = float.Parse(command[i][Bms.DataSection.EventChannel.Measure][0]);
                        }
                        if (command[i].ContainsKey(Bms.DataSection.EventChannel.Bpm))
                        {
                            bpm = Convert.ToInt32(command[i][Bms.DataSection.EventChannel.Bpm][0], 16);
                        }
                        if (command[i].ContainsKey(Bms.DataSection.EventChannel.ExpendBpm))
                        {
                            bpmEx = command[i][Bms.DataSection.EventChannel.ExpendBpm][0];

                            for (var j = 0; i < bpmEx.Length / 2; i++)
                            {
                                var t = GetHex(bpmEx, j * 2);

                                if (t != "00")
                                    bpm = _bms.Head.BpmCommand[t];
                                    
                                offsetY += beat * bpm * 0.09f / (bpmEx.Length * 0.5f);
                                timingOffset += 60 / (1 / beat / 4 * bpm);
                            }
                            currentMeasure = measure.Key;
                        }
                        else
                        {
                            offsetY += beat * bpm * 0.09f;
                            timingOffset += (60 * (1 / beat)) / (1 / beat / 4 * bpm);
                            currentMeasure = measure.Key;
                        }
                    }
                }

                bpmEx = "";

                if (command.ContainsKey(measure.Key))
                {
                    if (command[measure.Key].ContainsKey(Bms.DataSection.EventChannel.Bpm))
                    {
                        bpm = Convert.ToInt32(command[measure.Key][Bms.DataSection.EventChannel.Bpm][0], 16);
                    }
                    if (command[measure.Key].ContainsKey(Bms.DataSection.EventChannel.ExpendBpm))
                    {
                        bpmEx = command[measure.Key][Bms.DataSection.EventChannel.ExpendBpm][0];
                    }
                    if (command[measure.Key].ContainsKey(Bms.DataSection.EventChannel.Measure))
                    {
                        beat = float.Parse(command[measure.Key][Bms.DataSection.EventChannel.Measure][0]);
                    }
                }

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
                        default:
                            continue;
                    }
                    
                    var localYOffset = 0f;
                    var localTimingOffset = 0f;

                    foreach (var seq in node.Value)
                    {
                        var noteCount = seq.Length / 2;
                        var bpmExCount = bpmEx.Length / 2;

                        for (var i = 0; i < noteCount; i++)
                        {
                            var hex = GetHex(seq, i * 2);
                            var bpmHex = "00";
                            if (bpmExCount != 0)
                            {
                                bpmHex = GetHex(bpmEx,Map(i, 0, noteCount - 1, 0, bpmExCount) * 2);
                            }
                            if (bpmHex != "00")
                                bpm = _bms.Head.BpmCommand[bpmHex];

                            if (hex == "00")
                            {
                                localYOffset += (beat * bpm * 0.09f / noteCount);
                                localTimingOffset += (60 * (1 / beat)) / (1 / beat / 4 * bpm) / noteCount;
                                continue;
                            }

                            #region 롱노트
                            if (_bms.Head.LongNote.GetNoteType() == "LNOBJ")
                            {
                                if (_bms.Head.LongNote.GetLongNoteList()[0] == hex)
                                {
                                    var index = GetIndex(node.Key);

                                    var prev = lastNote[index];

                                    pos.y = (MeasureSpacing / noteCount) * i + measure.Key * MeasureSpacing;
                                    var transform1 = prev.transform;
                                    var transform1LocalScale = transform1.localScale;
                                    var position = transform1.position;
                                    
                                    transform1LocalScale.y = (position.y - pos.y) * transform1LocalScale.y;
                                    position = (position + ToVec3(pos)) * 0.5f;
                                    
                                    transform1.position = position;
                                    transform1.localScale = transform1LocalScale;
                                    prev._longNote = true;

                                    continue;
                                }
                            }   
                            else if (_bms.Head.LongNote.GetNoteType() == "1")
                            {
                            }
                            else if (_bms.Head.LongNote.GetNoteType() == "2")
                            {
                            }
                            #endregion

                            var note = Instantiate(_note);

                            count++;
                            pos.y = localYOffset + offsetY;
                            note._timing = timingOffset + localTimingOffset;
                            localYOffset += (beat * bpm * 0.09f / noteCount);
                            localTimingOffset += (60 * (1 / beat)) / (1 / beat / 4 * bpm) / noteCount;
                            
                            note.name = measure.Key + " Note " + i;

                            judge.Notes[GetIndex(node.Key) + 1].Push(note);
                            lastNote[GetIndex(node.Key)] = note;
                            try
                            {
                                note._audio = _bms.Head.WavFiles[hex];
                            }
                            catch (Exception e)
                            {
                                Debug.Log(hex);
                            }
                            
                            note.transform.position = pos;
                        }
                            
                    }
                }

                if (command.ContainsKey(measure.Key))
                {
                    if (command[measure.Key].ContainsKey(Bms.DataSection.EventChannel.Bpm))
                    {
                        bpm = Convert.ToInt32(command[measure.Key][Bms.DataSection.EventChannel.Bpm][0], 16);
                    }
                    if (command[measure.Key].ContainsKey(Bms.DataSection.EventChannel.ExpendBpm))
                    {
                        bpmEx = command[measure.Key][Bms.DataSection.EventChannel.ExpendBpm][0];
                    }
                }

                if (bpmEx.Length != 0)
                {
                    for (var i = 0; i < bpmEx.Length / 2; i++)
                    {
                        if (GetHex(bpmEx, i * 2) != "00")
                            bpm = _bms.Head.BpmCommand[bpmEx.Substring(i * 2, 2)];

                        timingOffset += 60 / (1 / beat / 4 * bpm);
                        offsetY += beat * bpm * 0.09f / (bpmEx.Length * 0.5f);
                    }
                }
                else
                {
                    timingOffset += (60 * (1 / beat)) / (1 / beat / 4 * bpm);
                    offsetY += beat * bpm * 0.09f;
                }

                currentMeasure++;
            }
            Debug.Log($"spawn {count} nodes");
            yield return null;
        }

        private static int Map(int val, int inMin, int inMax, int outMin, int outMax)
        {
            return (val - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
        }
        
        private static Vector3 ToVec3(Vector2 pos)
        {
            return new Vector3(pos.x, pos.y);
        }

        private static int GetIndex(Bms.DataSection.EventChannel c)
        {
            switch (c)
            {
                case Bms.DataSection.EventChannel.P1SideKey1:
                    return 0;
                case Bms.DataSection.EventChannel.P1SideKey2:
                    return 1;
                case Bms.DataSection.EventChannel.P1SideKey3:
                    return 2;
                case Bms.DataSection.EventChannel.P1SideKey4:
                    return 3;
                case Bms.DataSection.EventChannel.P1SideKey5:
                    return 4;
                case Bms.DataSection.EventChannel.P1SideKey6:
                    return 5;
                case Bms.DataSection.EventChannel.P1SideKey7:
                    return 6;
                default:
                    return 0;
            }
        }

        private static string GetHex(string str, int i) => str.Substring(i, 2);
    }
}
