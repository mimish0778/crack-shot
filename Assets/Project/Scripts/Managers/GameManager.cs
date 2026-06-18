using UnityEngine;

namespace CrackShot
{
    public enum FailReason { BallCollision, OutOfBounds, DidNotPassThrough, NotThroughGate }

    public class GameManager : PersistentSingleton<GameManager>
    {
        public enum GameState { Idle, Aiming, Shooting, Judging, Success, Failed, StageClear }

        public GameState CurrentState { get; private set; } = GameState.Idle;
        public int SelectedStageIndex { get; set; }
        public int TotalStageCount { get; set; }

        public int NextStageIndex => SelectedStageIndex + 1;
        public bool HasNextStage => TotalStageCount > 0 && NextStageIndex < TotalStageCount;

        protected override void OnAwake()
        {
#if UNITY_EDITOR
            PlayerPrefs.DeleteAll();
#endif
        }

        public void ChangeState(GameState newState) => CurrentState = newState;

        public void AnnounceFailedShot(FailReason reason)
        {
            ChangeState(GameState.Failed);
            ScoreManager.Instance?.AddPenalty();
            AudioManager.Instance?.PlayFailed();
            PlaySceneUI.Instance?.ShowFailedMessage(reason);
        }

        public void LoadTitleScene() => SceneLoader.Instance?.LoadTitleScene();
        public void LoadSelectScene() => SceneLoader.Instance?.LoadSelectScene();
        public void LoadPlayScene() => SceneLoader.Instance?.LoadPlayScene();
        public void LoadResultScene() => SceneLoader.Instance?.LoadResultScene();
        public void ReloadCurrentScene() => SceneLoader.Instance?.ReloadCurrentScene();

        public void ResetAndReload() => ResetThen(ReloadCurrentScene);
        public void ResetAndGoToPlay() => ResetThen(LoadPlayScene);
        public void ResetAndGoToSelect() => ResetThen(LoadSelectScene);

        private void ResetThen(System.Action load)
        {
            ScoreManager.Instance?.ResetScore();
            ChangeState(GameState.Idle);
            load();
        }

        public bool IsControllable() => CurrentState == GameState.Idle || CurrentState == GameState.Aiming;
    }
}
