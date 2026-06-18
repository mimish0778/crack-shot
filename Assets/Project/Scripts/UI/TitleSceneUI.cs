using UnityEngine;
using UnityEngine.UI;

namespace CrackShot
{
    public class TitleSceneUI : MonoBehaviour
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button quitButton;

        private void Start()
        {
            playButton?.onClick.AddListener(() => { AudioManager.Instance?.PlayTitlePlay(); GameManager.Instance?.LoadSelectScene(); });
            quitButton?.onClick.AddListener(() =>
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });
        }
    }
}
