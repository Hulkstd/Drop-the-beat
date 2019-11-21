using System;
using UnityEngine;

namespace UI.Animation
{
    public class JudgementText : TextChangedEvent<JudgementText>
    {
        [Serializable]
        public enum Judgement
        {
            Excelent = 0,
            Great = 1,
            Good = 2,
            Bad = 3,
            Poor = 4,
            Ignore = 5,
        }
        
        [SerializeField] private Animator _animator;
        [SerializeField] private string _animationName;
        [SerializeField] private Color[] _colorPerJudgement;
        private int _animationNameCache = int.MinValue;

        public void Judge(Judgement judge)
        {
            Text = judge.ToString();
            SetColor(_colorPerJudgement[(int)judge]);
            _onTextChanged.Invoke();
        }
        public void Judge(int judge)
        {
            Text = ((Judgement)judge).ToString();
            SetColor(_colorPerJudgement[judge]);
            _onTextChanged.Invoke();
        }

        private void SetColor(Color color)
        {
            if (TextMesh != null)
            {
                TextMesh.color = color;
            }
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
