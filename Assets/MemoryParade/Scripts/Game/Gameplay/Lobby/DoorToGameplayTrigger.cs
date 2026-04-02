using UnityEngine;
using Assets.MemoryParade.Scripts.Game.GameRoot;

public class DoorToGameplayTrigger : MonoBehaviour
{
    private bool _entered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_entered) return;
        if (!other.CompareTag("Player")) return;

        _entered = true;

        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.GoToScene(Scenes.GAMEPLAY);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(Scenes.GAMEPLAY);
    }
}