using System;
using System.Collections;
using GameManager;
using Node;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace UI.Animation
{
    public class ScoreText : TextChangedEvent<ScoreText>
    {
        [SerializeField] private long _score;
        public long Score
        {
            get => _score;
            set
            {
                _score = value;
                Text = _score.ToString("D9");
            }
        }

        private long _accumulatorGainScore;

        private void Start()
        {
            StartCoroutine(GainScore());
        }

        public void GainScore(int val)
        {
            _accumulatorGainScore += val;
        }

        private IEnumerator GainScore()
        {
            while (!BMSCapacity.Instance.IsGameDone || _accumulatorGainScore != 0)
            {
                if (_accumulatorGainScore == 0)
                {
                    yield return null;
                    continue;
                }

                var val = (long)Random.Range(1, _accumulatorGainScore);

                _accumulatorGainScore -= val;
                Score += val;
                yield return null;
            }
            yield return null;
        }
    }
}
