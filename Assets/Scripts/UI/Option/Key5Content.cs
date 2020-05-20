using System;
using GameManager.SaveData;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.Option
{
    public class Key5Content : Content<Key5Content>, IInitialize, IFinalization, IPointerDownHandler
    {
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
                            KeyboardManager.GetKeyCode(i + 2).ToString();
                        
                        _spawnObject.Add(text);
                        
                        break;
                    case ContentType.Slider:
                        
                        var slider = Instantiate(_sliderPrefab, transform);
                        slider.GetComponent<RectTransform>().anchoredPosition = pos;
                        pos += _offset;

                        slider.transform.GetComponent<TextMeshProUGUI>().text = _contentNames[i];
                        slider.GetComponentInChildren<UnityEngine.UI.Slider>().onValueChanged.RemoveAllListeners();
                        slider.GetComponentInChildren<UnityEngine.UI.Slider>().onValueChanged.AddListener(
                            (x) => slider.GetComponentInChildren<TextMeshProUGUI>().text = (1 / x).ToString("0%"));
                        
                        _spawnObject.Add(slider);
                        
                        break;      
                }
            }
        }

        protected override void Update()
        {
            base.Update();
            
            if (_lastInput == "") return;     
            if (KeyboardManager.ContainKeyCode((KeyCode) Enum.Parse(typeof(KeyCode), _lastInput)))
            {
                _lastInput = "";
                return;
            }

            KeyboardManager.ReplaceKey((KeyCode) Enum.Parse(typeof(KeyCode), ((TextMeshProUGUI) _selectedText).text),
                (KeyCode) Enum.Parse(typeof(KeyCode), _lastInput));
            ((TextMeshProUGUI) _selectedText).text = _lastInput;
            _isClickInputWindow = false;
            _selectedText = null;
            _lastInput = "";
        }
    }
}
