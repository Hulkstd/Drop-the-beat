using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Option
{
    public class OptionSubject : Subject<OptionSubject>, IInitialize, IPointerDownHandler
    {
        [SerializeField] private Image _selectCover;
        private Coroutine _coverMove;

        public override int Index
        {
            get => base.Index;

            set
            {
                base.Index = value;
                
                if(_coverMove != null)
                    StopCoroutine(_coverMove);
                
                _coverMove = StartCoroutine(CoverMove(CalculateOffset(_index)));
            }
        }

        private IEnumerator CoverMove(Vector3 to)
        {
            Debug.Log(to);
            to += Vector3.down * 5 + Vector3.left * 10;
            for (var i = 1; i <= 30; i++)
            {
                _selectCover.rectTransform.anchoredPosition = Vector3.Lerp(_selectCover.rectTransform.anchoredPosition, to, i / 30f);

                yield return null;
            }

            _coverMove = null;
            yield return null;
        }
    }
}
