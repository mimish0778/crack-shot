using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CrackShot
{
    public class SelectSceneUI : MonoBehaviour
    {
        [SerializeField] private Button[] stageButtons;
        [SerializeField] private TextMeshProUGUI[] stageNameTexts;
        [SerializeField] private StageData[] stageDatas;
        [SerializeField] private Button backButton;

        private void Start()
        {
            backButton?.onClick.AddListener(() => { AudioManager.Instance?.PlaySelectBack(); GameManager.Instance?.LoadTitleScene(); });
            for (int i = 0; i < stageButtons.Length; i++)
            {
                int index = i;
                stageButtons[i]?.onClick.AddListener(() => { AudioManager.Instance?.PlaySelectStage(); OnStageSelected(index); });
                if (i < stageNameTexts.Length && i < stageDatas.Length)
                {
                    stageNameTexts[i].text = stageDatas[i].StageName;
                }
            }
        }

        private void OnStageSelected(int index)
        {
            if (GameManager.Instance == null)
            {
                return;
            }
            GameManager.Instance.SelectedStageIndex = index;
            GameManager.Instance.LoadPlayScene();
        }
    }
}
