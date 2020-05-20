using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Action;
using LibplanetUnity;
using LibplanetUnity.Action;
using Node;
using UI.Animation;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
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
        [NonSerialized] public SortQueue<Bmp> Bmps;
        [FormerlySerializedAs("currentBar")] public int _currentBar = 0;
        [FormerlySerializedAs("currentBeat")] public float _currentBeatWeight = 1;
        [FormerlySerializedAs("path")] public string _path;
        [FormerlySerializedAs("BarLine")] public GameObject _barLine;

        protected override bool Awake()
        {
            if (!base.Awake())
                return false;

            MeasureOffset = new Dictionary<int, float>();
            Beats = new Dictionary<int, float>();
            BPMs = new SortQueue<BPM>();
            Stops = new SortQueue<Stop>();
            Notes = new SortQueue<Note>();
            BGMs = new SortQueue<Note>();
            Bmps = new SortQueue<Bmp>();

            StartCoroutine(StartParsing());
            StartCoroutine(GameDoneCheck());

            return true;
        }

        private IEnumerator GameDoneCheck()
        {
            yield return new WaitUntil(() => IsDone);
            
            yield return new WaitUntil(() => 
                (BPMs.Length == 0)  && 
                (Stops.Length == 0) && 
                (Notes.Length == 0) && 
                (BGMs.Length == 0)  && 
                (Bmps.Length == 0));
            
            yield return new WaitForSeconds(5.0f);
            
            //yield return new WaitUntil(() => Input.GetKeyUp(KeyCode.Return));
            GameDone();
        }

        private void GameDone()
        {
//            if (SceneChanger.AutoPlay)
//                return;
            Debug.Log("GameDone");
            var path = (SceneChanger.SongPath == "" ? _path : SceneChanger.SongPath);
            var title = path.Substring(path.LastIndexOf('/') + 1);

            var list = new List<ActionBase>();
            var action = new ModifyRank()
            {
                Score = ScoreText.Instance.Score,
                Title = title,
            };
            list.Add(action);

            Agent.instance.MakeTransaction(list);
            
            SceneChanger.Instance.SceneChange("MainScene", true);
        }

        private IEnumerator StartParsing()
        {
            Bms = Parser.Parse(SceneChanger.SongPath == "" ? _path : SceneChanger.SongPath);
            Bms.AddNoteOnMeasure(Bms.Data.TotalBar+1, 2, "1");
            Bms.AddNoteOnMeasure(Bms.Data.TotalBar+1, 3, "64");

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
            yield return StartCoroutine(ProcessBmp());
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
                                    lastNote[index].ToBeat = ((double)i / count) * 4.0;
                                    continue;
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
                                    Notes.Push(tmp);
                                    lastNote[index] = tmp;
                                }
                                else
                                {
                                    lastNote[index].ToBar = bar.Key;
                                    lastNote[index].ToBeat = ((double)(i + 1) / count) * 4.0;
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
                                        lastNote[index].ToBeat = ((double)(i+1) / count) * 4.0;
                                    }

                                    continue;
                                }

                                if (lastNote[index] != null)
                                {
                                    if (lastNote[index].Sound != hex)
                                    {
                                        lastNote[index].ToBar = bar.Key;
                                        lastNote[index].ToBeat = ((double)(i+1) / count) * 4.0;

                                        continue;
                                    }
                                }

                                var tmp = new Note(hex, true, index, bar.Key, i, count);
                                Notes.Push(tmp);
                                lastNote[index] = tmp;
                            }
                        }
                    }
                }
            }
            Notes.Sort();
            
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

        private IEnumerator ProcessBmp()
        {
            var command = Bms.Data.GetCommandSection();
            var bmpPath = Bms.Head.BmpFiles;

            foreach (var bar in command)
            {
                foreach (var bgm in bar.Value.Where(y => y.Key == Bms.DataSection.EventChannel.BgaBase).SelectMany(pair => pair.Value))
                {
                    var count = bgm.Length / 2;

                    for (var i = 0; i < count; i++)
                    {
                        var hex = GetHex(bgm, i * 2);
                        
                        if(hex == "00")
                            continue;

                        Bmps.Push(new Bmp(bmpPath[hex], bar.Key, i, count));
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
            
            yield return null;
            if (BPMs.Length == 0 || BPMs.Top.Beat != 0)
                BPMs.Push(new BPM(Bms.Head.Bpm, 0, 0, 1));

            BPMs.Top.Timing = 0;

            for (var i = 1; i < BPMs.Length; i++)
            {
                BPMs[i].Timing = BPMs[i - 1].Timing + (BPMs[i].Beat - BPMs[i - 1].Beat) / (BPMs[i - 1].Bpm / 60);
            }
            yield return null;
            
            foreach (var stop in Stops)
            {
                stop.Beat = GetTotalBeatUntil(stop.Bar) + stop.Beat * GetBeat(stop.Bar);
                stop.Timing = CalculateTiming(stop);
            }
            Stops.Sort();

            yield return null;
            foreach (var note in Notes)
            {
                note.Beat = GetTotalBeatUntil(note.Bar) + note.Beat * GetBeat(note.Bar);
                note.Timing = CalculateTiming(note);

                if (!note.IsLongNote) continue;
                
                note.ToBeat = GetTotalBeatUntil(note.ToBar) + note.ToBeat * GetBeat(note.ToBar);
                note.ToTiming = CalculateTiming(note.ToBeat);
            }
            Notes.Sort();

            yield return null;
            foreach (var bgm in BGMs)
            {
                bgm.Beat = GetTotalBeatUntil(bgm.Bar) + bgm.Beat * GetBeat(bgm.Bar);
                bgm.Timing = CalculateTiming(bgm);
            }
            BGMs.Sort();
            
            yield return null;
            foreach (var bmp in Bmps)
            {
                bmp.Beat = GetTotalBeatUntil(bmp.Bar) + bmp.Beat * GetBeat(bmp.Bar);
                bmp.Timing = CalculateTiming(bmp);
            }
            Bmps.Sort();

            yield return null;
            for (var i = 0; i < Bms.Data.TotalBar; i++)
            {
                var obj = new LObject() {Bar = i, Beat = 0, Timing = 0};

                obj.Beat = GetTotalBeatUntil(obj.Bar);
                Instantiate(_barLine, new Vector3(-3.5f, (float)obj.Beat * NodeCreator.Instance._noteSpeed, 0), Quaternion.Euler(0, 0, 0));
            }

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

        private double CalculateTiming(LObject obj)
        {
            double timing = 0;

            var i = 0;
            for (i = 0; i < BPMs.Length - 1 && obj.Beat > BPMs[i + 1].Beat; ++i)
            {
                timing += (BPMs[i + 1].Beat - BPMs[i].Beat) / BPMs[i].Bpm * 60;
            }

            timing += (obj.Beat - BPMs[i].Beat) / BPMs[i].Bpm * 60;

            return timing;
        }
        
        private double CalculateTiming(double beat)
        {
            double timing = 0;

            var i = 0;
            for (i = 0; i < BPMs.Length - 1 && beat > BPMs[i + 1].Beat; ++i)
            {
                timing += (BPMs[i + 1].Beat - BPMs[i].Beat) / BPMs[i].Bpm * 60;
            }

            timing += (beat - BPMs[i].Beat) / BPMs[i].Bpm * 60;

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
