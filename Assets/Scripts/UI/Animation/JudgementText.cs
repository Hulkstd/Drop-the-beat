using System;
using UnityEngine;

namespace UI.Animation
{
    public class JudgementText : TextChangedEvent<JudgementText>
    {
        [Serializable]
        public enum Judgement
        {
            Excelent = 1,
            Great = 2,
            Good = 3,
            Bad = 4,
            Miss = 5,
        }
        
        [SerializeField] private Animator _animator;
        [SerializeField] private string _animationName;
        private int _animationNameCache = int.MinValue;

        public void Judge(Judgement judge)
        {
            Text = judge.ToString();
            _onTextChanged.Invoke();
        }
        public void Judge(int judge)
        {
            Text = ((Judgement)judge).ToString();
            _onTextChanged.Invoke();
        }
        
        public void PlayAnimation()
        {
            if (_animationNameCache == int.MinValue)
            {
                _animationNameCache = Animator.StringToHash(_animationName);
            }
            _animator.Play(_animationNameCache);
        }
    }
}
