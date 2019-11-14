using System;
using UnityEngine;

namespace UI.Animation
{
    public class ComboText : TextChangedEvent<ComboText>
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private string _animationName;
        private int _animationNameCache = int.MinValue;
        
        public void GainCombo(int Value)
        {
            Text = (int.Parse(Text) + Value).ToString();
        }

        public void TimeScale(float n)
        {
            Time.timeScale = n;
        }

        public void ResetCombo()
        {
            var tmp = _onTextChanged;
            _onTextChanged.RemoveAllListeners();
            Text = 0.ToString();
            _onTextChanged = tmp;
        }

        public void PlayAnimation()
        {
            if (_animationNameCache == int.MinValue)
            {
                _animationNameCache = Animator.StringToHash(_animationName);
            }
            _animator.Play(_animationNameCache);
        }

        private void Awake()
        {
            Text = 0.ToString();
        }
    }
}
