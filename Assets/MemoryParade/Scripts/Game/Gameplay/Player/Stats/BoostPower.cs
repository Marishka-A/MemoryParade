using UnityEngine;

namespace Assets.MemoryParade.Scripts.Game.Gameplay.Player.Stats
{
    public class BoostPower : MonoBehaviour
    {
        public void OnClick()
        {
            if (PlayerСharacteristics.Instance == null) return;

            if (PlayerСharacteristics.Instance.numberOfWins > 0)
            {
                PlayerСharacteristics.Instance.AddStrength(5); // +5 силы

                PlayerСharacteristics.Instance.numberOfWins--;
            }
        }
    }
}