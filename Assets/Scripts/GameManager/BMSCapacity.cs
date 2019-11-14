using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Utility;

namespace GameManager
{
    public class BMSCapacity : Singleton<BMSCapacity>
    {
        [NonSerialized] public bool IsDone;
        [NonSerialized] public Bms Bms;
        [NonSerialized] public bool IsGameDone;
        [NonSerialized] public Dictionary<int, float> MeasureOffset;
        [NonSerialized] public Dictionary<int, float> Beats;
        [NonSerialized] public SortQueue<BPM> BPMs;
        [NonSerialized] public SortQueue<Stop> Stops;
        [NonSerialized] public SortQueue<Note> Notes;
        [NonSerialized] public SortQueue<Note> BGMs;
        [FormerlySerializedAs("currentBar")] public int _currentBar = 0;
        [FormerlySerializedAs("currentBeat")] public float _currentBeatWeight = 1;
        [FormerlySerializedAs("path")] public string _path;

        private void Start()
        {
            MeasureOffset = new Dictionary<int, float>();
            Beats = new Dictionary<int, float>();
            BPMs = new SortQueue<BPM>();
            Stops = new SortQueue<Stop>();
            Notes = new SortQueue<Note>();
            BGMs = new SortQueue<Note>();

            StartCoroutine(StartParsing());
        }

        private IEnumerator StartParsing()
        {
            Bms = Parser.Parse(_path);

            yield return new WaitUntil(() => Bms.Head.WavFileCount == Bms.Head.WavFiles.Count);

            yield return StartCoroutine(PreProcess());
            
            IsDone = true;
        }

        private IEnumerator PreProcess()
        {
            yield return StartCoroutine(ProcessBeat());
            yield return StartCoroutine(ProcessBPM());
            yield return StartCoroutine(ProcessStop());
            yield return StartCoroutine(ProcessNote());
            yield return StartCoroutine(ProcessBGM());
            yield return StartCoroutine(ProcessTiming());
        }

        private IEnumerator ProcessBeat()
        {
            var command = Bms.Data.GetCommandSection();

            foreach (var bar in command)
            {
                foreach (var beat in bar.Value.Where(beats => beats.Key == Bms.DataSection.EventChannel.Measure).SelectMany(beats => beats.Value))
                {
                    Beats.Add(bar.Key, float.Parse(beat));
                }
            }
            
            yield return null;
        }

        private IEnumerator ProcessBPM()
        {
            var command = Bms.Data.GetCommandSection();
            
            foreach (var bar in command)
            {
                foreach (var bpm in bar.Value.Where(bpms => bpms.Key == Bms.DataSection.EventChannel.ExpendBpm).SelectMany(bpms => bpms.Value))
                {
                    var count = bpm.Length / 2;

                    for (var i = 0; i < count; i++)
                    {
                        var hex = GetHex(bpm, i * 2);
                        
                        if (hex == "00")
                            continue;

                        BPMs.Push(new BPM(Bms.Head.BpmCommand[hex], bar.Key, i, count));
                    }
                }
            }

            command = Bms.Data.GetCommandSection();

            foreach (var bar in command)
            {
                foreach (var bpm in bar.Value.Where(bpms => bpms.Key == Bms.DataSection.EventChannel.Bpm).SelectMany(bpms => bpms.Value))
                {
                    var count = bpm.Length / 2;

                    for (var i = 0; i < count; i++)
                    {
                        var hex = GetHex(bpm, i * 2);
                        
                        if(hex == "00")
                            continue;

                        BPMs.Push(new BPM(Convert.ToInt32(hex, 16), bar.Key, i, count));
                    }
                }
            }
            
            yield return null;
        }

        private IEnumerator ProcessStop()
        {
            var command = Bms.Data.GetCommandSection();

            foreach (var bar in command)
            {
                foreach (var stop in bar.Value.Where(stops => stops.Key == Bms.DataSection.EventChannel.SequenceStop).SelectMany(pair => pair.Value))
                {
                    var count = stop.Length / 2;

                    for (var i = 0; i < count; i++)
                    {
                        var hex = GetHex(stop, i * 2);
                        
                        if (hex == "00")
                            continue;
                        
                        Stops.Push(new Stop((int)Bms.Head.StopCommand[hex], bar.Key, i, count));
                    }
                }
            }

            yield return null;
        }

        private IEnumerator ProcessNote()
        {
            var command = Bms.Data.GetNoteSection();
                
            var lastNote = new Note[7]
            {
                null,
                null,
                null,
                null,
                null,
                null,
                null
            };
            
            foreach (var bar in command)
            {
                foreach (var notes in bar.Value.Where(pair => pair.Key == Bms.DataSection.EventChannel.P1SideKey1 ||
                                                              pair.Key == Bms.DataSection.EventChannel.P1SideKey2 ||
                                                              pair.Key == Bms.DataSection.EventChannel.P1SideKey3 ||
                                                              pair.Key == Bms.DataSection.EventChannel.P1SideKey4 ||
                                                              pair.Key == Bms.DataSection.EventChannel.P1SideKey5 ||
                                                              pair.Key == Bms.DataSection.EventChannel.P1SideKey6 ||
                                                              pair.Key == Bms.DataSection.EventChannel.P1SideKey7))
                {
                    var index = GetIndex(notes.Key);

                    foreach (var note in notes.Value)
                    {
                        var count = note.Length / 2;

                        for (var i = 0; i < count; i++)
                        {
                            var hex = GetHex(note, i * 2);

                            if (hex == "00")
                                continue;

                            if (Bms.Head.LongNote.GetNoteType() == "LNOBJ")
                            {
                                if (Bms.Head.LongNote.GetLongNoteList().Any(val => val == hex))
                                {
                                    lastNote[index].IsLongNote = true;
                                    lastNote[index].ToBar = bar.Key;
                                    lastNote[index].ToBeat = (i / count) * 1000;
                                }
                            }

                            var tmp = new Note(hex, false, index, bar.Key, i, count);
                            
                            Notes.Push(tmp);
                            lastNote[index] = tmp;
                        }
                    }
                }
            }

            command = Bms.Data.GetNoteSection();
            
            lastNote = new Note[7]
            {
                null,
                null,
                null,
                null,
                null,
                null,
                null
            };
            
            if (Bms.Head.LongNote.GetNoteType() == "1")
            {
                foreach (var bar in command)
                {
                    foreach (var notes in bar.Value.Where(y => y.Key == Bms.DataSection.EventChannel.P1SideLongNote1 ||
                                                               y.Key == Bms.DataSection.EventChannel.P1SideLongNote2 ||
                                                               y.Key == Bms.DataSection.EventChannel.P1SideLongNote3 ||
                                                               y.Key == Bms.DataSection.EventChannel.P1SideLongNote4 ||
                                                               y.Key == Bms.DataSection.EventChannel.P1SideLongNote5 ||
                                                               y.Key == Bms.DataSection.EventChannel.P1SideLongNote6 ||
                                                               y.Key == Bms.DataSection.EventChannel.P1SideLongNote7 )) 
                    {
                        var index = GetIndex(notes.Key);

                        foreach (var note in notes.Value)
                        {
                            var count = note.Length / 2;

                            for (var i = 0; i < count; i++)
                            {
                                var hex = GetHex(note, i * 2);

                                if (hex == "00")
                                    continue;

                                if (lastNote[index] == null)
                                {
                                    var tmp = new Note(hex, true, index, bar.Key, i, count);

                                    lastNote[index] = tmp;
                                }
                                else
                                {
                                    lastNote[index].ToBar = bar.Key;
                                    lastNote[index].ToBeat = (i / count) * 1000;
                                    lastNote[index] = null;
                                }
                            }
                        }
                    }
                }
            }

            else if (Bms.Head.LongNote.GetNoteType() == "2")
            {
                foreach (var bar in command)
                {
                    foreach (var notes in bar.Value.Where(y => y.Key == Bms.DataSection.EventChannel.P1SideLongNote1 ||
                                                               y.Key == Bms.DataSection.EventChannel.P1SideLongNote2 ||
                                                               y.Key == Bms.DataSection.EventChannel.P1SideLongNote3 ||
                                                               y.Key == Bms.DataSection.EventChannel.P1SideLongNote4 ||
                                                               y.Key == Bms.DataSection.EventChannel.P1SideLongNote5 ||
                                                               y.Key == Bms.DataSection.EventChannel.P1SideLongNote6 ||
                                                               y.Key == Bms.DataSection.EventChannel.P1SideLongNote7 ))
                    {
                        var index = GetIndex(notes.Key);
                        
                        foreach (var note in notes.Value)
                        {
                            var count = note.Length / 2;

                            for (var i = 0; i < count; i++)
                            {
                                var hex = GetHex(note, i * 2);

                                if (hex == "00")
                                {
                                    if (lastNote[index] != null)
                                    {
                                        lastNote[index].ToBar = bar.Key;
                                        lastNote[index].ToBeat = (i / count) * 1000;
                                    }

                                    continue;
                                }

                                if (lastNote[index] != null)
                                {
                                    if (lastNote[index].Sound != hex)
                                    {
                                        lastNote[index].ToBar = bar.Key;
                                        lastNote[index].ToBeat = (i / count) * 1000;

                                        continue;
                                    }
                                }

                                var tmp = new Note(hex, true, index, bar.Key, i, count);

                                lastNote[index] = tmp;
                            }
                        }
                    }
                }
            }
            
            yield return null;
        }
        
        private IEnumerator ProcessBGM()
        {
            var command = Bms.Data.GetCommandSection();

            foreach (var bar in command)
            {
                foreach (var bgm in bar.Value.Where(y => y.Key == Bms.DataSection.EventChannel.BackgroundMusic).SelectMany(pair => pair.Value))
                {
                    var count = bgm.Length / 2;

                    for (var i = 0; i < count; i++)
                    {
                        var hex = GetHex(bgm, i * 2);
                        
                        if(hex == "00")
                            continue;

                        BGMs.Push(new Note(hex, false, 0, bar.Key, i, count));
                    }
                }
            }
            
            yield return null;
        }

        private IEnumerator ProcessTiming()
        {
            foreach (var bpm in BPMs)
                bpm.Beat = GetTotalBeatUntil(bpm.Bar) + bpm.Beat * GetBeat(bpm.Bar);
            BPMs.Sort();

            if (BPMs.Length == 0 || BPMs.Top.Beat != 0)
                BPMs.Push(new BPM(Bms.Head.Bpm, 0, 0, 1));

            BPMs.Top.Timing = 0;

            for (var i = 1; i < BPMs.Length; i++)
            {
                BPMs[i].Timing = BPMs[i].Timing + (BPMs[i].Beat - BPMs[i - 1].Beat) / (BPMs[i - 1].Bpm / 60);
            }

            foreach (var stop in Stops)
            {
                stop.Beat = GetTotalBeatUntil(stop.Bar) + stop.Beat * GetBeat(stop.Bar);
                stop.Timing = CalculateTiming(stop);
            }
            Stops.Sort();

            foreach (var note in Notes)
            {
                note.Beat = GetTotalBeatUntil(note.Bar) + note.Beat * GetBeat(note.Bar);
                note.Timing = CalculateTiming(note);
            }
            Notes.Sort();

            foreach (var bgm in BGMs)
            {
                bgm.Beat = GetTotalBeatUntil(bgm.Bar) + bgm.Beat * GetBeat(bgm.Bar);
                bgm.Timing = CalculateTiming(bgm);
            }
            BGMs.Sort();

            yield return null;
        }

        private float GetTotalBeatUntil(int bar)
        {
            float sum = 0;

            for (var i = 0; i < bar; i++)
            {
                sum += GetBeat(i);
            }

            return sum * 4f;
        }

        private float CalculateTiming(LObject obj)
        {
            float timing = 0;

            var i = 0;
            for (i = 1; i < BPMs.Length - 1 && BPMs[i - 1].Beat < obj.Beat; i++)
            {
                timing += (BPMs[i].Beat - BPMs[i - 1].Beat) / BPMs[i - 1].Bpm * 60;
            }

            timing += (obj.Beat - BPMs[i].Beat) / BPMs[i].Bpm * 60;

            return timing;
        }

        public float GetBeat(int bar) => Beats.ContainsKey(bar) ? Beats[bar] : 1;
        
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
    }
}
