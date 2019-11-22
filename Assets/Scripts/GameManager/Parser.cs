using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

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
            var randomNum = -1;
            var startIf = false;
            var inIf = false;
        
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
                    else if (SubString(line, "#BPM "))
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

                        var length = line.Length - 7 - 4; // 확장자 제거를 위하여 -4
                        var fileName = line.Substring(7, length);
                        bms.Head.WavFileCount++;

                        StaticClassCoroutineManager.Instance.Coroutine(
                            bms.AddWavFIle($@"{fileParent}\",$"{fileName}", strNum));
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

                        var length = line.Length - 8;
                        var strData = line.Substring(8, length);
                        bms.AddStopCommand(strNum, strData);
                    }
                    else if (SubString(line,"#BPM"))
                    {
                        var strNum = line.Substring(4, 2);

                        var length = line.Length - 7;
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
                    else if (SubString(line, "#random"))
                    {
                        var num = int.Parse(line.Substring(8, 1));

                        randomNum = Random.Range(1, num);
                    }
                    else if (SubString(line, "#if"))
                    {
                        var num = int.Parse(line.Substring(4, line.Length - 4));
                        startIf = true;

                        inIf = num == randomNum;
                    }
                    else if (startIf && !inIf && SubString(line, "#else"))
                    {
                        inIf = true;
                    }
                    else if (SubString(line, "#endif"))
                    {
                        inIf = startIf = false;
                    }
                    else if (inIf)
                    {
                        if (!SubString(line, "#")) continue;
                        //#00101 aabbccdd length 15 9
                        var measure = int.Parse(line.Substring(1, 3));
                        var channel = int.Parse(line.Substring(4, 2));
                        bms.AddNoteOnMeasure(measure, channel, line.Substring(7, line.Length - 7));
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
                private string _type = "1";
                private readonly List<string> _longNotes = new List<string>();

                public void SetType(string input) => _type = input;
                public void AddLongNoteNum(string noteNum) => _longNotes.Add(noteNum);

                public string GetNoteType() => _type;
                public IEnumerable<string> GetLongNoteList() => _longNotes;
            }
        
            public int Player;
            public string Title;
            public string Artist;
            public string Genre;
            public double Bpm;
            public int PlayLevel;
            public int Rank;
            public int Total;
            public int VolumeWav;
            public int WavFileCount;
            public Sprite StageFile;
            public readonly LongNoteInfo LongNote;

            public Dictionary<string, AudioClip> WavFiles { get; }
            public Dictionary<string, string> BmpFiles { get; }
            public Dictionary<string, double> StopCommand { get; }
            public Dictionary<string, double> BpmCommand { get; }

            public Header()
            {
                VolumeWav = 100;
                Bpm = 130;
                Rank = 2;
                LongNote = new LongNoteInfo();
                WavFiles = new Dictionary<string, AudioClip>();
                BmpFiles = new Dictionary<string, string>();
                StopCommand = new Dictionary<string, double>();
                BpmCommand = new Dictionary<string, double>();
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
                P2SideKey6 = 28,
                P2SideKey7 = 29,
                P1InvisibleObject = 31, // until 36
                P2InvisibleObject = 41, // until 46
                P1SideLongNote1 = 51,
                P1SideLongNote2 = 52,
                P1SideLongNote3 = 53,
                P1SideLongNote4 = 54,
                P1SideLongNote5 = 55,
                P1SideLongNoteSc = 56,
                P1SideLongNoteFree = 57,
                P1SideLongNote6 = 58,
                P1SideLongNote7 = 59,
                P2SideLongNote1 = 61,
                P2SideLongNote2 = 62,
                P2SideLongNote3 = 63,
                P2SideLongNote4 = 64,
                P2SideLongNote5 = 65,
                P2SideLongNoteSc = 66,
                P2SideLongNoteFree = 67,
                P2SideLongNote6 = 68,
                P2SideLongNote7 = 69,
            }
            private readonly SortedDictionary<int, Dictionary<EventChannel, List<string>>> _commandSection;
            private readonly SortedDictionary<int, Dictionary<EventChannel, List<string>>> _noteSection;
            private readonly Dictionary<int, int> _noteSectionMaxLength;
            public int TotalBar;

            public DataSection()
            {
                _commandSection = new SortedDictionary<int, Dictionary<EventChannel, List<string>>>();
                _noteSection = new SortedDictionary<int, Dictionary<EventChannel, List<string>>>();
                _noteSectionMaxLength = new Dictionary<int, int>();
            }

            public void PushData(int measure, EventChannel channel, string seq)
            {
                if (measure > TotalBar) TotalBar = measure;
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
                    case EventChannel.P1SideScratch:
                    case EventChannel.P1SideLongNoteSc:
                        {
                            if (!_commandSection.ContainsKey(measure))
                            {
                                _commandSection.Add(measure, new Dictionary<EventChannel, List<string>>());
                            }

                            if (channel == EventChannel.P1SideScratch || channel == EventChannel.P1SideLongNoteSc)
                                channel = EventChannel.BackgroundMusic;

                            if (!_commandSection[measure].ContainsKey(channel))
                            {
                                _commandSection[measure].Add(channel, new List<string>());
                            }
                            if(seq != "00")
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
                    case EventChannel.P1SideLongNote1:
                    case EventChannel.P1SideLongNote2:
                    case EventChannel.P1SideLongNote3:
                    case EventChannel.P1SideLongNote4:
                    case EventChannel.P1SideLongNote5:
                    case EventChannel.P1SideLongNote6:
                    case EventChannel.P1SideLongNote7:
                        {
                            if (!_noteSection.ContainsKey(measure))
                            {
                                _noteSection.Add(measure, new Dictionary<EventChannel, List<string>>());
                            }

                            if (!_noteSection[measure].ContainsKey(channel))
                            {
                                _noteSection[measure].Add(channel, new List<string>());
                            }

                            if(!_noteSectionMaxLength.ContainsKey(measure))
                            {
                                    _noteSectionMaxLength.Add(measure, seq.Length);
                            }

                            _noteSectionMaxLength[measure] = _noteSectionMaxLength[measure] < seq.Length ? seq.Length : _noteSectionMaxLength[measure];
                            _noteSection[measure][channel].Add(seq);
                        }
                        break;
                }
            }

            public SortedDictionary<int, Dictionary<EventChannel, List<string>>> GetNoteSection() => _noteSection;
            public SortedDictionary<int, Dictionary<EventChannel, List<string>>> GetCommandSection() => _commandSection;
            public Dictionary<int, int> GetNoteSectionMaxLength() => _noteSectionMaxLength;
        }

        public readonly Header Head;
        public readonly DataSection Data;

        private static AudioType[] _supportTypes =
        {
            AudioType.OGGVORBIS,
            AudioType.WAV,
            AudioType.MPEG,
        };
        
        public Bms()
        {
            Head = new Header();
            Data = new DataSection();
        }

        public void SetTitle(string input) => Head.Title = input;
        
        public void SetArtist(string input) => Head.Artist = input;
    
        public void SetGenre(string input) => Head.Genre = input;
    
        public void SetPlayer(int input) => Head.Player = input;

        public void SetBpm(double input) => Head.Bpm = input;

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

        public IEnumerator AddWavFIle(string path, string fileName,string strNum)
        {
            var type = AudioType.OGGVORBIS;

            if (File.Exists(path + fileName + ".wav"))
            {
                fileName += ".wav";
                type = AudioType.WAV;
            }
            else if(File.Exists(path + fileName + ".ogg"))
            {
                fileName += ".ogg";
                type = AudioType.OGGVORBIS;
            }
            else if(File.Exists(path + fileName + ".mp3"))
            {
                fileName += ".mp3";
                type = AudioType.OGGVORBIS;
            }

            using (var www = UnityWebRequestMultimedia.GetAudioClip("file://"+path + UnityWebRequest.EscapeURL(fileName), type))
            {
                yield return www.SendWebRequest();
                
                if (www.downloadHandler.data.Length == 0)
                {
//                    Debug.Log(www.downloadHandler.data);
                    Debug.Log($"{path}{UnityWebRequest.EscapeURL(fileName)}, {strNum} {type.ToString()} doesnt find in files");
                    Debug.Log($"{www.error}");
                    Head.WavFiles.Add(strNum, null);
                }
                else
                {
                    var audio = DownloadHandlerAudioClip.GetContent(www);
                    audio.LoadAudioData();
                    Head.WavFiles.Add(strNum, audio);
                }
            }
        }

        public void AddBmpFile(string input, string strNum) => Head.BmpFiles.Add(strNum, input);

        public void AddStopCommand(string strNum, string strData) => Head.StopCommand.Add(strNum, int.Parse(strData));

        public void AddBpmCommand(string strNum, string strData) => Head.BpmCommand.Add(strNum, double.Parse(strData));

        public AudioClip GetAudioClip(string hex) => Head.WavFiles.ContainsKey(hex) ? Head.WavFiles[hex] : null;
        
        private static string GetHex(string str, int i) => Utility.Utility.GetHex(str, i);
    }
}