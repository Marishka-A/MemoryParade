using TMPro;
using UnityEngine;

namespace Assets.MemoryParade.Scripts.Game.Gameplay.Player
{
    public class UpdateNumbersOfWins : MonoBehaviour
    {
        private TextMeshProUGUI winsText;
        private TextMeshProUGUI health;
        private TextMeshProUGUI attackPower;

        private void Awake()
        {
            winsText = GetComponent<TextMeshProUGUI>();

            GameObject healthObj = GameObject.Find("HealthCount");
            if (healthObj != null)
                health = healthObj.GetComponent<TextMeshProUGUI>();

            GameObject attackObj = GameObject.Find("AttackPower");
            if (attackObj != null)
                attackPower = attackObj.GetComponent<TextMeshProUGUI>();
        }

        private void Update()
        {
            if (PlayerСharacteristics.Instance == null)
                return;

            if (attackPower != null)
                attackPower.text = PlayerСharacteristics.Instance.strength.ToString();

            if (health != null)
                health.text = PlayerСharacteristics.Instance.healthPoints.ToString();

            if (winsText != null)
                winsText.text = PlayerСharacteristics.Instance.numberOfWins.ToString();
        }
    }
}