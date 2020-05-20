using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class RankContent : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _rank;
        [SerializeField] private TextMeshProUGUI _playerName;
        [SerializeField] private TextMeshProUGUI _score;
        [SerializeField] private Image _bgImage;
        
        public long Score => long.Parse(_score.text);
        public int Rank => int.Parse(_rank.text);

        public void Initialize(int rank, string playerName, string playerHex, long score, bool bgImage)
        {
            _rank.text = rank.ToString();
            _playerName.text = $"{playerName} #{playerHex.Substring(0, 4).ToUpper()}";
            _score.text = score.ToString();
            _bgImage.enabled = bgImage;
        }
    }
}
