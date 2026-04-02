using UnityEngine;
using Assets.MemoryParade.Scripts.Game.GameRoot;

public class MainMenuController : MonoBehaviour
{
    public void PlayGame()
    {
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.GoToScene(Scenes.DIALOGUE_INTRO);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(Scenes.DIALOGUE_INTRO);
        }
    }

    public void ExitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}