using System;
using System.Collections;
using GameManager;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using Utility;

namespace UI.ListBox
{
    public class MusicList : ListBox<MusicList, MusicList.MusicInfo, MusicList.MusicListItem>
    {
        [Serializable]
        public class MusicListItem : ListBoxItem
        {
            [SerializeField]
            private TextMeshProUGUI _title;
            [SerializeField]
            private TextMeshProUGUI _key;

            public TextMeshProUGUI Title
            {
                get => _title;
                set => _title = value;
            }

            public TextMeshProUGUI Key
            {
                get => _key;
                set => _key = value;
            }


            public void Set(string title, string key)
            {
                _title.text = title;
                _key.text = key;
            }
        }

        [Serializable]
        public class MusicInfo
        {
            public enum KeyStatus
            {
                Key5,
                Key7,
            }

            public string MusicName;
            public KeyStatus MusicKey;

            public MusicInfo(string musicName, KeyStatus musicKey)
            {
                MusicName = musicName;
                MusicKey = musicKey;
            }
            
            public MusicInfo(string musicName, string musicKey)
            {
                MusicName = musicName;
                MusicKey = (KeyStatus)Enum.Parse(typeof(KeyStatus), musicKey);
            }
        }

        public class SelectUI : MonoBehaviour{}
        
        [Space]
        [SerializeField] private RectTransform _selectUI;
        private SelectUI _selectImage;
        private SelectUI _currentSelectUI;
        [SerializeField] private bool _cooldown;
        [SerializeField] private int _fileIndex;
        public int FileIndex
        {
            get => _fileIndex;
            set
            {
                if (_cooldown)
                    return;
                
                var weight = Mathf.Clamp(value, 0, _list.Count - 1);

                if (weight - _fileIndex == 0)
                    return;
                
                var direction = weight - _fileIndex;
                _fileIndex = value;

                _cooldown = true;
                
                RankingManager.Instance.Initialize(MusicList.Instance.CurrentItem.MusicName);
                StartCoroutine(SyncSelectUI(direction));
            }
        }
        public MusicInfo CurrentItem => _list[FileIndex];

        protected override void Start()
        {
            base.Start();

            RefreshDropdown();
        }

        protected override void RefreshDropdown()
        {
            if (_itemTransform.GetComponent<MusicListItem>() == null)
            {
                Item = _itemTransform.AddComponent<MusicListItem>();
                Item.Title = _titleText as TextMeshProUGUI;
                Item.Key = _keyText as TextMeshProUGUI;
            }

            if (_selectUI.GetComponent<SelectUI>() == null)
            {
                _selectImage = _selectUI.gameObject.AddComponent<SelectUI>();
            }
            _fileIndex = 0;

            base.RefreshDropdown();

            var itemTemplate = ListBoxTransform.GetComponentInChildren<MusicListItem>().transform;
            _currentSelectUI = ListBoxTransform.GetComponentInChildren<SelectUI>();

            itemTemplate.gameObject.SetActive(false);

            var offset = ((RectTransform) itemTemplate.parent).sizeDelta;

            ((RectTransform) itemTemplate.parent).sizeDelta -= offset;

            var i = 0;
            _list.ForEach((x) =>
            {
                Item.Set(x.MusicName, x.MusicKey.ToString());

                var obj = Instantiate(Item, itemTemplate.parent, false);
                obj.gameObject.SetActive(true);
                obj.name = "Item" + i;

                ((RectTransform) obj.transform.parent).sizeDelta += offset;
                ((RectTransform) obj.transform).anchoredPosition -= offset * i++;

                Items.Add(obj);
            });
            if (_list.Count == 0)
            {
                _currentSelectUI.gameObject.SetActive(false);
                MusicInfoUI.Instance.Initialize(null, null, "", "");
            }
            else
            {
//                Debug.Log(MusicManager.GetCurrentDirectory() + "\\" + _list[FileIndex].MusicName);
                var bms = Parser.ParseHead(MusicManager.GetCurrentDirectory() + "\\" + _list[FileIndex].MusicName);
  
//                Debug.Log(bms.Head.Banner);
//                Debug.Log(bms.Head.StageFile);

                StartCoroutine(SyncUI(bms));
            }
        }

        private IEnumerator SyncSelectUI(int direction)
        {
            var contentTransform = Items[0].transform.parent as RectTransform;
            var moveTarget = _currentSelectUI.transform as RectTransform;
            var moveOffset = new Vector2(0, 0);
            var sizeOffset = contentTransform.sizeDelta.y / _list.Count;
            
            if (direction == -1)
            {
                if (Mathf.Abs(((RectTransform)_currentSelectUI.transform).anchoredPosition.y) < 0.5f) // 1번쨰.
                {
                    moveTarget = contentTransform;
                    moveOffset.y -= sizeOffset;
                }
                else // 2번째, 3번쨰, 4번쨰
                {
                    moveOffset.y += sizeOffset;
                }
            }
            else
            {
                if (Mathf.Abs(((RectTransform)_currentSelectUI.transform).anchoredPosition.y + 300f) < 0.5f) // 4번쨰.
                {
                    moveTarget = contentTransform;
                    moveOffset.y += sizeOffset;
                }
                else // 1번쨰, 2번째, 3번쨰
                {
                    moveOffset.y -= sizeOffset;
                }
            }

            var fromPos = moveTarget.anchoredPosition;
            var toPos = moveTarget.anchoredPosition + moveOffset;
            for (var i = 1; i <= 8; i++)
            {
                moveTarget.anchoredPosition = Vector2.Lerp(fromPos, toPos, i / 8f);
                yield return null;
            }
            moveTarget.anchoredPosition = toPos;

//            Debug.Log(MusicManager.GetCurrentDirectory() + "\\" + _list[FileIndex].MusicName);
            var bms = Parser.ParseHead(MusicManager.GetCurrentDirectory() + "\\" + _list[FileIndex].MusicName);

            StartCoroutine(SyncUI(bms));
            
            _cooldown = false;
            
            yield return null;
        }

        private IEnumerator SyncUI(Bms bms)
        {
            Sprite banner = null;
            var bannerDone = false;
            if (bms.Head.Banner != null)
            {
                var bannerUrl = bms.Head.Banner.Split('/')[0] + "/" +
                                UnityWebRequest.EscapeURL(bms.Head.Banner.Split('/')[1]);
//                Debug.Log(bannerUrl);
                StartCoroutine(CreateSprite(bannerUrl, new Vector2Int(300, 80),
                    (x) =>
                    {
                        banner = Sprite.Create(x, new Rect(0, 0, x.width, x.height), Vector2.one / 2);
                        bannerDone = true;
                    }));
            }
            else
                bannerDone = true;

            Sprite stageFile = null;
            var stageFileDone = false;
            if (bms.Head.StageFile != null)
            {
                var stageFileUrl = bms.Head.StageFile.Split('/')[0] + "/" +
                                   UnityWebRequest.EscapeURL(bms.Head.StageFile.Split('/')[1]);
                StartCoroutine(CreateSprite(stageFileUrl, new Vector2Int(640, 480),
                    (x) =>
                    {
                        stageFile = Sprite.Create(x, new Rect(0, 0, x.width, x.height), Vector2.one / 2);
                        stageFileDone = true;
                    }));
            }
            else
                stageFileDone = true;

            while (!stageFileDone || !bannerDone)
                yield return null;
            
            MusicInfoUI.Instance.Initialize(banner, stageFile, bms.Head.Title, bms.Head.Artist);
        }

        private static IEnumerator CreateSprite(string path, Vector2Int sizeDelta, Action<Texture2D> setSprite)
        {
            if (path.EndsWith(".bmp"))
            {
                var loader = new BMPManager.BMPLoader();
                var image = loader.LoadBMP(path);
                setSprite(image.ToTexture2D());
            }
            else
            {
                using (var www = UnityWebRequestTexture.GetTexture("file:///" + path))
                {
                    yield return www.SendWebRequest();

                    if (www.downloadHandler.data.Length == 0)
                    {

                    }
                    else
                    {
                        var texture = DownloadHandlerTexture.GetContent(www);
                        setSprite(texture);
                    }
                }
            }
        }
    }
}
