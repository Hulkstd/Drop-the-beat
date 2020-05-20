using GameManager;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.U2D;
using UnityEngine.UI;

namespace UI
{
    public class MusicInfoUI : Singleton<MusicInfoUI>
    {
        public Image _banner;
        public Image _stageFile;
        public TextMeshProUGUI _title;
        public GameObject _titleParent;
        public TextMeshProUGUI _artist;
        public GameObject _artistParent;

        public void Initialize(Sprite banner, Sprite stageFile, string title, string artist)
        {
            _banner.gameObject.SetActive(true);
            _stageFile.gameObject.SetActive(true);
            _titleParent.SetActive(true);
            _artistParent.SetActive(true);
            
            if(banner)
                _banner.sprite = banner;
            else
                _banner.gameObject.SetActive(false);
            if (stageFile)
                _stageFile.sprite = stageFile;
            else
                _stageFile.gameObject.SetActive(false);
            if(title != "")
                _title.text = title;
            else
                _titleParent.SetActive(false);
            if(artist != "")
                _artist.text = artist;
            else
                _artistParent.SetActive(false);
        }
    }
}
