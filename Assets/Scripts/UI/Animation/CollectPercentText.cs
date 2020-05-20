using System.Collections;
using GameManager;
using UnityEngine;

namespace UI.Animation
{
    public class CollectPercentText : TextChangedEvent<CollectPercentText>
    {
        [SerializeField] private double _collect;
        [SerializeField] private double _totalC;
        
        public double Percent => _collect / _totalC;

        public void UpdateData(JudgementText.Judgement judge)
        {
            _collect += judge == JudgementText.Judgement.Excellent ? 1 :
                        judge == JudgementText.Judgement.Great ? 0.7 :
                        judge == JudgementText.Judgement.Good ? 0.4 :
                        0;
            _totalC += 1;

            Text = Percent.ToString("000.0%");
        }
    }
}
