using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utilities;

namespace UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField]
        private Button playButton;
        [SerializeField]
        private Button settingsButton;
        [SerializeField]
        private Button quitButton;

        //============================================================================================================//
        
        // Start is called before the first frame update
        private void Start()
        {
            ScreenFader.ForceSetColorBlack();
            playButton.onClick.AddListener(OnPlayButtonPressed);

#if UNITY_WEBGL
            quitButton.gameObject.SetActive(false);
#else
            quitButton.onClick.AddListener(OnQuitButtonPressed);
#endif

            ScreenFader.FadeIn(1f, null);
        }

        //============================================================================================================//
        
        private void OnPlayButtonPressed()
        {
            ScreenFader.FadeOut(1f, () =>
            {
                SceneManager.LoadScene(1);
            });
        }

        private void OnQuitButtonPressed()
        {
            Application.Quit();
        }
        
        //============================================================================================================//
    }
}
