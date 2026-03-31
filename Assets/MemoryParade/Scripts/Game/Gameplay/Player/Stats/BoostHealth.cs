using UnityEngine;

namespace Assets.MemoryParade.Scripts.Game.Gameplay.Player.Stats
{
    public class BoostHealth : MonoBehaviour
    {
        public void OnClick()
        {
            if (PlayerСharacteristics.Instance == null) return;

            if (PlayerСharacteristics.Instance.numberOfWins > 0)
            {
                PlayerСharacteristics.Instance.healthPoints += 20;

                // ограничение, чтобы не уходило выше 100
                if (PlayerСharacteristics.Instance.healthPoints > 100)
                    PlayerСharacteristics.Instance.healthPoints = 100;

                PlayerСharacteristics.Instance.numberOfWins--;
            }
        }
    }
}