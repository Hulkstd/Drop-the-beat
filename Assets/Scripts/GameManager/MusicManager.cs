using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UI;
using UI.ListBox;
using UnityEngine;
using UnityEngine.UI;

namespace GameManager
{
    public class MusicManager : Singleton<MusicManager>
    {
        private static string _basePath = "";
        public static List<GameObject> Items = new List<GameObject>();

        private static List<string> _files;
        private static List<string> _directories;
        private static int _showIndex;
        private static double _angle;

        private static int _rotateSpeed = 10;
        private static Transform _rectTransform;
        private static bool _updateCooldown;
        private static System.Action<int> _directoryUpdate; 
        private static int _directoryIndex;
        
        public static int DirectoryIndex
        {
            get => _directoryIndex;

            set
            {
                if (value < 0 || value >= _directories.Count)
                    return;
                
                var direction = value - DirectoryIndex;
                
                _directoryIndex = value;
                Instance.StartCoroutine(RotateItems(direction, _directoryUpdate));
            }
        }

        private GraphicRaycaster _canvas;
        
        public bool _enterDirectory;
        public Transform _circle;
        public Toggle _autoPlay;
        public TextMeshProUGUI _profile;
            
        public static void LoadDirectory()
        {
            if (!Directory.Exists(_basePath))
                Directory.CreateDirectory(_basePath);
            
            _directories = Directory.GetDirectories(_basePath).ToList();
        }

        public static void LoadFiles(string directoryName)
        {
            _files = Directory.GetFiles($"{_basePath}/{directoryName}", "*.*", SearchOption.TopDirectoryOnly)
                .Where((x) => x.EndsWith(".bme") || x.EndsWith(".bms")).ToList();
        }

        public static string GetCurrentDirectory() => _directories[DirectoryIndex];

        private void Start()
        {
            if (_basePath == "")
                _basePath = Application.persistentDataPath + "/Music";
            if (_circle != null)
                _rectTransform = _circle;
            if (!Directory.Exists(_basePath))
                Directory.CreateDirectory(_basePath);

            _profile.text =
                $"{SceneChanger.AgentState.Name} #{SceneChanger.AgentState.address.ToHex().Substring(0, 4).ToUpper()}";
            
            ItemCreator.Instance.CreateObject(0);
            ItemCreator.Instance.CreateObject(12);
            _angle = 360.0 / ItemCreator.Instance._itemCount;

            Items.Clear();
            ItemCreator.CopyList(ref Items);
            
            LoadDirectory(); 
            
            _showIndex = (int)Mathf.Ceil((float) (90 / _angle)) + 1;
            _directoryIndex = 0;

            _directoryUpdate = null;
            _directoryUpdate += UpdateDirectoryName;
            
            _directoryUpdate.Invoke(DirectoryIndex);
            _canvas = GetComponent<GraphicRaycaster>();
        }

        private void Update()
        {
            if (!_canvas.enabled)
                return;
            
            if (Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.DownArrow))
                _rotateSpeed = 10;
            if (_updateCooldown)
                return;
            if (_enterDirectory)
            {
                if (Input.GetKeyUp(KeyCode.Escape))
                {
                    MusicList.Instance.RemoveAll();
                    RankingManager.Instance.RemoveAll();
                    _enterDirectory = false;
                }
                if (Input.GetKeyUp(KeyCode.UpArrow))
                    MusicList.Instance.FileIndex--;
                if (Input.GetKeyUp(KeyCode.DownArrow))
                    MusicList.Instance.FileIndex++;
                if (Input.GetKeyUp(KeyCode.Return))
                {
                    var currentItem = MusicList.Instance.CurrentItem;
                    var path = GetCurrentDirectory() + "/" + currentItem.MusicName;
                    var sceneName = currentItem.MusicKey == MusicList.MusicInfo.KeyStatus.Key5 ? "5kScene" : "7kScene";

                    SceneChanger.Instance.SceneChange(sceneName, true, path, _autoPlay.isOn, currentItem.MusicKey);
                }

                return;
            }
            
            if (Input.GetKey(KeyCode.UpArrow) && !_updateCooldown)
                DirectoryIndex++;
            if (Input.GetKey(KeyCode.DownArrow) && !_updateCooldown)
                DirectoryIndex--;
            if (Input.GetKeyUp(KeyCode.Return))
            {
                _enterDirectory = true;
                
                var directoryName = Items[0].GetComponentInChildren<TextMeshProUGUI>().text;
                LoadFiles(directoryName);

                _files.ForEach((x) =>
                {
                    var name = (string)GetValue(-1, x.Split('\\').ToList());
                    MusicList.Instance.AddOption(new MusicList.MusicInfo(name, Parser.DiscriminationKey(x) ? MusicList.MusicInfo.KeyStatus.Key7 : MusicList.MusicInfo.KeyStatus.Key5));
                });
                RankingManager.Instance.Initialize(MusicList.Instance.CurrentItem.MusicName);
            }
        }

        private static void UpdateDirectoryName(int index)
        {
            for (var i = -_showIndex; i <= _showIndex; i++)
            {
                var obj = (GameObject)GetValue(i, Items);

                if (obj == null)
                    continue;    
                
                if (index + i < 0 || index + i >= _directories.Count)
                {
                    obj.SetActive(false);
                }
                else
                {
                    var text = obj.GetComponentInChildren<TextMeshProUGUI>();

                    var name = (string) GetValue(index + i, _directories);

                    obj.SetActive(true);
                    if(name == null)
                        obj.SetActive(false);
                    else 
                        text.text = name.Split('\\')[1];
                }
            }
        }

        private static IEnumerator RotateItems(int direction, System.Action<int> funcChain)
        {
            _updateCooldown = true;
            var rotation = _rectTransform.rotation;
            var from = rotation;
            var to = Quaternion.Euler(0, 0, (float) (rotation.eulerAngles.z + _angle * direction));
            for (var i = 1; i <= _rotateSpeed; i++)
            {
                _rectTransform.rotation = Quaternion.Lerp(from, to, (float)i / _rotateSpeed);
                yield return null;
            }

            _rectTransform.rotation = to;
            _rotateSpeed = Mathf.Clamp(_rotateSpeed - 1, 3, 10);
            
            if (direction > 0)
            {
                var obj = Items[0];
                Items.RemoveAt(0);
                Items.Add(obj);
            }
            else
            {
                var obj = Items[Items.Count - 1];
                Items.RemoveAt(Items.Count - 1);
                Items.Insert(0, obj);
            }
            
            funcChain?.Invoke(DirectoryIndex);
            _updateCooldown = false;
            yield return null;
        }

        private static object GetValue<T>(int index, List<T> list) => Utility.Utility.GetValue(index, list);
    }
}
