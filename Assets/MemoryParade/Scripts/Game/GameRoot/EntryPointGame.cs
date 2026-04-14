using System.Collections;
using Assets.MemoryParade.Scripts.Game.Gameplay.Root;
using Assets.MemoryParade.Scripts.Game.GameRoot;
using Assets.MemoryParade.Scripts.Game.MainMenu.Root;
using Assets.MemoryParade.Scripts.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using R3;

namespace Assets.MemoryParade.Scripts.Game.GameRoot
{
    /// <summary>
    /// точка входа в игру
    /// </summary>
    public class EntryPointGame
    {
        private static EntryPointGame _instance;
        private Coroutines _coroutines;
        private UIRootView _uiRoot;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void StartGame()
        {
            _instance = new EntryPointGame();
            _instance.RunGame();
        }

        private EntryPointGame()
        {
            _coroutines = new GameObject("[COROUTINES]").AddComponent<Coroutines>();
            Object.DontDestroyOnLoad(_coroutines.gameObject);

            var prefabUIRoot = Resources.Load<UIRootView>("UIRoot");
            _uiRoot = Object.Instantiate(prefabUIRoot);
            Object.DontDestroyOnLoad(_uiRoot.gameObject);
        }

        private void RunGame()
        {
            EntryParamsGameplay entryGameplayParams = new EntryParamsGameplay(0, null, false);
            EntryParamsMainMenu entryMainMenuParams = new EntryParamsMainMenu(0, null);

#if UNITY_EDITOR
            var sceneName = SceneManager.GetActiveScene().name;

            if (sceneName == Scenes.GAMEPLAY || sceneName == Scenes.LOBBY)
            {
                _coroutines.StartCoroutine(LoadAndStartGameplay(entryGameplayParams));
                return;
            }

            if (sceneName == Scenes.MAIN_MENU)
            {
                _coroutines.StartCoroutine(LoadAndStartMainMenu(entryMainMenuParams));
                return;
            }

            if (sceneName == Scenes.DIALOGUE_INTRO)
            {
                return;
            }
#endif

            // В билде игра стартует с главного меню
            _coroutines.StartCoroutine(LoadAndStartMainMenu(entryMainMenuParams));
        }

        /// <summary>
        /// Корутина для старта геймплея
        /// </summary>
        private IEnumerator LoadAndStartGameplay(EntryParamsGameplay entryParams)
        {
            _uiRoot.ShowLoadingScreen();

            yield return LoadScene(Scenes.LOBBY);
            yield return new WaitForSeconds(0.5f);

            _uiRoot.HideLoadingScreen();
        }

        /// <summary>
        /// Корутина для старта главного меню
        /// </summary>
        private IEnumerator LoadAndStartMainMenu(EntryParamsMainMenu entryParams)
        {
            _uiRoot.ShowLoadingScreen();

            yield return LoadScene(Scenes.MAIN_MENU);
            yield return new WaitForSeconds(0.5f);

            _uiRoot.HideLoadingScreen();
        }

        /// <summary>
        /// Корутина для загрузки сцены
        /// </summary>
        private IEnumerator LoadScene(string sceneName)
        {
            yield return SceneManager.LoadSceneAsync(sceneName);
        }
    }
}