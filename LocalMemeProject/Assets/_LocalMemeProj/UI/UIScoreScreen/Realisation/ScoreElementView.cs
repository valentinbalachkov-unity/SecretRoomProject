using TMPro;
using UnityEngine;

namespace _LocalMemeProj.UI.UIScoreScreen.Realisation
{
    public class ScoreElementView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _playerText;
        
        public void UpdateElement(string playerName, int score)
        {
            if (score == 0)
            {
                _playerText.text = $"{playerName}: {score} мемкоинов (лузер)";
            }
            else
            {
                _playerText.text = $"{playerName}: {score} мемкоинов";
            }
        }
    }
}