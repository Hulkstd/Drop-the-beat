using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace GameManager
{
    public static class Parser
    {
        public static Bms Parse(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError($"{filePath} cant Exists while Parsing");
                throw new FileLoadException();
            }
        
            var bms = new Bms();

            var qData = File.ReadAllLines(filePath);
            var headerField = true;
            var fileParent = Directory.GetParent(filePath).FullName;
        
            foreach (var line in qData)
            {
                if (line == "") continue;
                
                if (headerField)
                {
                    if (SubString(line, "#PLAYER"))
                    {
                        var length = line.Length - 8;
                        bms.SetPlayer(int.Parse(line.Substring(8, length)));
                    }
                    else if (SubString(line,"#GENRE"))
                    {    
                        var length = line.Length - 7;
                        bms.SetGenre(line.Substring(7, length));
                    }
                    else if (SubString(line, "#TITLE"))
                    {    
                        var length = line.Length - 7;
                        bms.SetTitle(line.Substring(7, length));
                    }
                    else if (SubString(line, "#ARTIST"))
                    {
                        var length = line.Length - 8;
                        bms.SetArtist(line.Substring(8, length));
                    }
                    else if (SubString(line, "#BPM"))
                    {    
                        var length = line.Length - 5;
                        bms.SetBpm(int.Parse(line.Substring(5, length)));
                    }
                    else if (SubString(line, "#PLAYLEVEL"))
                    {
                        var length = line.Length - 11;
                        bms.SetPlayLevel(int.Parse(line.Substring(11, length)));
                    }
                    else if (SubString(line,"#RANK"))
                    {
                        var length = line.Length - 6;
                        bms.SetRank(int.Parse(line.Substring(6, length)));
                    }
                    else if (SubString(line, "#TOTAL"))
                    {
                        var length = line.Length - 7;
                        bms.SetTotal(int.Parse(line.Substring(7,length)));
                    }
                    else if (SubString(line, "#VOLWAV"))
                    {
                        var length = line.Length - 8;
                        bms.SetVolumeWav(int.Parse(line.Substring(8, length)));
                    }
                    else if (SubString(line, "#STAGEFILE"))
                    {
                        var length = line.Length - 11;
                        bms.SetStageFile(fileParent + '/' + line.Substring(11, length));
                    }
                    else if (SubString(line, "#WAV"))
                    {
                        var strNum = line.Substring(4, 2);

                        var length = line.Length - 6;
                        var fileName = line.Substring(6, length);

                        StaticClassCoroutineManager.Instance.Coroutine(
                            bms.AddWavFIle($"{fileParent}/{fileName}", strNum));
                    }
                    else if (SubString(line,"#BMP"))
                    {
                        var strNum = line.Substring(4, 2);

                        var length = line.Length - 6;
                        var fileName = line.Substring(6, length);
                        bms.AddBmpFile($"{fileParent}/{fileName}", strNum);
                    }
                    else if (SubString(line, "#STOP"))
                    {
                        var strNum = line.Substring(5, 2);

                        var length = line.Length - 7;
                        var strData = line.Substring(8, length);
                        bms.AddStopCommand(strNum, strData);
                    }
                    else if (SubString(line,"#BPM"))
                    {
                        var strNum = line.Substring(4, 2);

                        var length = line.Length - 6;
                        var strData = line.Substring(7, length);
                        bms.AddBpmCommand(strNum, strData);
                    }
                    else if (SubString(line, "#LNTYPE"))
                    {
                        bms.SetLongNoteType(line.Substring(8,1));
                    }
                    else if (SubString(line, "#LNOBJ"))
                    {
                        bms.SetLongNoteType("LNOBJ");
                        bms.AddLongNote(line.Substring(7,2));
                    }

                    if (line == "*---------------------- MAIN DATA FIELD")
                    {
                        headerField = false;
                    }
                }
                else
                {
                    if (!SubString(line, "#")) continue;
                    //#00101 aabbccdd length 15 9
                    var measure = int.Parse(line.Substring(1, 3));
                    var channel = int.Parse(line.Substring(4, 2));
                    bms.AddNoteOnMeasure(measure, channel, line.Substring(7, line.Length - 7));
                }
            }

            return bms;
        }

        private static bool SubString(string a, string b)
        {
            if (a.Length < b.Length) return false;

            return a.Substring(0, b.Length) == b;
        }
    }
    
    public class Bms
    {
        public class Header
        {
            public class LongNoteInfo
            {
                private string _type;
                private readonly List<string> _longNotes = new List<string>();

                public void SetType(string input) => _type = input;
                public void AddLongNoteNum(string noteNum) => _longNotes.Add(noteNum);

                public string GetNoteType() => _type;
                public List<string> GetLongNoteList() => _longNotes;
            }
        
            public int Player;
            public string Title;
            public string Artist;
            public string Genre;
            public int Bpm;
            public int PlayLevel;
            public int Rank;
            public int Total;
            public int VolumeWav;
            public Sprite StageFile;
            public readonly LongNoteInfo LongNote;

            public Dictionary<string, AudioClip> WavFiles { get; }
            public Dictionary<string, string> BmpFiles { get; }
            public Dictionary<string, int> StopCommand { get; }
            public Dictionary<string, float> BpmCommand { get; }

            public Header()
            {
                VolumeWav = 100;
                Bpm = 130;
                Rank = 2;
                LongNote = new LongNoteInfo();
                WavFiles = new Dictionary<string, AudioClip>();
                BmpFiles = new Dictionary<string, string>();
                StopCommand = new Dictionary<string, int>();
                BpmCommand = new Dictionary<string, float>();
            }
        }
        
        public class DataSection
        {
            public enum EventChannel
            {
                BackgroundMusic = 1,
                Measure = 2,
                Bpm = 3,
                BgaBase = 4,
                ExtendedObject = 5,
                BgaPoor = 6,
                BgaLayer = 7,
                ExpendBpm = 8,
                SequenceStop = 9,
                P1SideKey1 = 11,
                P1SideKey2 = 12,
                P1SideKey3 = 13,
                P1SideKey4 = 14,
                P1SideKey5 = 15,
                P1SideScratch = 16,
                P1SideFreeZone = 17,
                P1SideKey6 = 18,
                P1SideKey7 = 19,
                P2SideKey1 = 21,
                P2SideKey2 = 22,
                P2SideKey3 = 23,
                P2SideKey4 = 24,
                P2SideKey5 = 25,
                P2SideScratch = 26,
                P2SideFreeZone = 27,
                P1InvisibleObject = 31, // until 36
                P2InvisibleObject = 41, // until 46
            }
            private readonly Dictionary<int, Dictionary<EventChannel, List<string>>> _commandSection;
            private readonly Dictionary<int, Dictionary<EventChannel, List<string>>> _noteSection;

            public DataSection()
            {
                _commandSection = new Dictionary<int, Dictionary<EventChannel, List<string>>>();
                _noteSection = new Dictionary<int, Dictionary<EventChannel, List<string>>>();
            }

            public void PushData(int measure, EventChannel channel, string seq)
            {
                switch (channel)
                {
                    case EventChannel.BackgroundMusic:
                    case EventChannel.Measure:
                    case EventChannel.Bpm:
                    case EventChannel.BgaBase:
                    case EventChannel.ExtendedObject:
                    case EventChannel.BgaPoor:
                    case EventChannel.BgaLayer:
                    case EventChannel.ExpendBpm:
                    case EventChannel.SequenceStop:
                    {
                        if (!_commandSection.ContainsKey(measure))
                        {
                            _commandSection.Add(measure, new Dictionary<EventChannel, List<string>>());
                        }

                        if (!_commandSection[measure].ContainsKey(channel))
                        {
                            _commandSection[measure].Add(channel, new List<string>());
                        }
                        _commandSection[measure][channel].Add(seq);
                    } 
                        break;
                    case EventChannel.P1SideKey1:
                    case EventChannel.P1SideKey2:
                    case EventChannel.P1SideKey3:
                    case EventChannel.P1SideKey4:
                    case EventChannel.P1SideKey5:
                    case EventChannel.P1SideKey6:
                    case EventChannel.P1SideKey7:
                    {
                        if (!_noteSection.ContainsKey(measure))
                        {
                            _noteSection.Add(measure, new Dictionary<EventChannel, List<string>>());
                        }

                        if (!_noteSection[measure].ContainsKey(channel))
                        {
                            _noteSection[measure].Add(channel, new List<string>());
                        }
                        _noteSection[measure][channel].Add(seq);
                    }
                        break;
                }
            }

            public Dictionary<int, Dictionary<EventChannel, List<string>>> GetNoteSection() => _noteSection;
            public Dictionary<int, Dictionary<EventChannel, List<string>>> GetCommandSection() => _commandSection;
        }

        public readonly Header Head;
        public readonly DataSection Data;

        public Bms()
        {
            Head = new Header();
            Data = new DataSection();
        }

        public void SetTitle(string input) => Head.Title = input;
        
        public void SetArtist(string input) => Head.Artist = input;
    
        public void SetGenre(string input) => Head.Genre = input;
    
        public void SetPlayer(int input) => Head.Player = input;

        public void SetBpm(int input) => Head.Bpm = input;

        public void SetRank(int input) => Head.Rank = input;

        public void SetTotal(int input) => Head.Total = input;
    
        public void SetPlayLevel(int input) => Head.PlayLevel = input;
    
        public void SetVolumeWav(int input) => Head.VolumeWav = input;

        public void SetStageFile(string input)
        {
            if (!File.Exists(input))
            {
                Debug.LogError($"{input} StageFile cant exists.");
                return;
            }

            var fileData = File.ReadAllBytes(input);
            if(Head.StageFile == null)
                Head.StageFile = Sprite.Create(new Texture2D(1,1), new Rect(), Vector2.one * 0.5f);
            Head.StageFile.texture.LoadImage(fileData);
        }

        public void SetLongNoteType(string input) => Head.LongNote.SetType(input);

        public void AddLongNote(string input) => Head.LongNote.AddLongNoteNum(input);

        public void AddNoteOnMeasure(int measure, int channel, string input) =>
            Data.PushData(measure, (DataSection.EventChannel) channel, input);

        public IEnumerator AddWavFIle(string path,string strNum)
        {
            var unityRequest = UnityWebRequest.Get(path);
//#pragma warning disable 618
//            var www = new WWW(path);
//#pragma warning restore 618
//
//            while (www.isDone)
//                yield return null;
//
//            var clip = www.GetAudioClip();
//           
//            Head.WavFiles.Add(strNum, clip);
        }

        public void AddBmpFile(string input, string strNum) => Head.BmpFiles.Add(strNum, input);

        public void AddStopCommand(string strNum, string strData) => Head.StopCommand.Add(strNum, int.Parse(strData));
        
        public void AddBpmCommand(string strNum, string strData) => Head.BpmCommand.Add(strNum, float.Parse(strData));
    }
}