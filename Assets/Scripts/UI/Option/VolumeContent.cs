using System;
using GameManager.SaveData;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;

namespace UI.Option
{
    public class VolumeContent : Content<VolumeContent>, IInitialize, IFinalization, IPointerDownHandler
    {
        [SerializeField] private AudioMixer _masterVolume; 
        protected override void PreDraw()
        {
            base.PreDraw();

            var pos = _origin;
            
            for (var i = 0; i < _contentNames.Count; i++)
            {
                switch (_contentTypes[i])
                {
                    case ContentType.TextBox:

                        var text = Instantiate(_textBoxPrefab, transform);
                        text.GetComponent<RectTransform>().anchoredPosition = pos;
                        pos += _offset;

                        text.transform.GetComponent<TextMeshProUGUI>().text = _contentNames[i];
                        text.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text =
                            KeyboardManager.GetKeyCode(i + 1).ToString();
                        
                        _spawnObject.Add(text);
                        
                        break;
                    case ContentType.Slider:
                        
                        var slider = Instantiate(_sliderPrefab, transform);
                        slider.GetComponent<RectTransform>().anchoredPosition = pos;
                        pos += _offset;

                        _masterVolume.GetFloat("Master", out var vol);
                        vol = Utility.Utility.Map(vol, -80, 20, 0, 1);

                        slider.transform.GetComponent<TextMeshProUGUI>().text = _contentNames[i];
                        slider.GetComponentsInChildren<TextMeshProUGUI>()[1].text = vol.ToString("0%");
                        
                        slider.GetComponentInChildren<UnityEngine.UI.Slider>().value = vol;
                        slider.GetComponentInChildren<UnityEngine.UI.Slider>().onValueChanged.RemoveAllListeners();
                        slider.GetComponentInChildren<UnityEngine.UI.Slider>().onValueChanged.AddListener(
                            (x) => slider.transform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>().text = x.ToString("0%"));
                        slider.GetComponentInChildren<UnityEngine.UI.Slider>().onValueChanged.AddListener(
                            (x) =>
                            {
                                var volume = Utility.Utility.Map(x, 0, 1, -80, 20);
                                _masterVolume.SetFloat("Master", volume);
                                VolumeManager.SetVolume(volume);
                            });
                        
                        _spawnObject.Add(slider);
                        
                        break;      
                }
            }
        }

        private void Start()
        {
            _masterVolume.SetFloat("Master", VolumeManager.GetVolume());
        }

        protected override void Update()
        {
            base.Update();
            
            if (_lastInput == "") return; 

            KeyboardManager.ReplaceKey((KeyCode) Enum.Parse(typeof(KeyCode), ((TextMeshProUGUI) _selectedText).text),
                (KeyCode) Enum.Parse(typeof(KeyCode), _lastInput.ToUpper()));
            ((TextMeshProUGUI) _selectedText).text = _lastInput;
            _isClickInputWindow = false;
            _selectedText = null;
            _lastInput = "";
        }
    }
}
