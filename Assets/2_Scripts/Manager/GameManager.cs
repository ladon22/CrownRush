using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public enum GameState
{
    Lobby,
    Initialize,
    PlayerTurn,
    EnemyTurn,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState CurrentState;

    public GridManager GridManager;
    public BoardRenderer BoardRenderer;
    public InputController InputController;
    public TurnManager TurnManager;
    public EnemyManager EnemyManager;
    public HealthUIManager HealthUIManager;
    public ItemManager ItemManager;
    public ScoreManager ScoreManager;

    public GameObject PlayerPrefab;

    [Header("UI")]
    public UITransitionController UITransition;
    public GameObject LobbyUIPanel;
    public GameObject GameUIPanel;
    public GameOverUIController GameOverUI;
    public SettingsUIController SettingsUI;
    public UpdateBestScoreUI LobbyUI;
    public PauseMenuController PauseMenu;
    public ExitConfirmPanel ExitConfirmPanel;

    private bool IsInitialized = false;
    private PlayerController CurrentPlayer;

    public CollectionUIController CollectionUI;
    private bool HasRevivedThisGame = false;
    private const string BestScoreKey = "BestScore";
    private bool IsStartingGame = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        ChangeState(GameState.Lobby);

        if (!IsInitialized)
        {
            InitializeGameStage();
            IsInitialized = true;
        }

        if (AudioManager.Instance != null) AudioManager.Instance.PlayBGM(BgmType.Lobby);
        if (LobbyUIPanel != null) LobbyUIPanel.SetActive(true);
    }

    void Update()
    {
#if UNITY_EDITOR
        if (Keyboard.current != null && Keyboard.current.f12Key.wasPressedThisFrame)
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();

            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
            );
        }
#endif
    }
    public void ChangeState(GameState state)
    {
        CurrentState = state;
    }

    private void InitializeGameStage()
    {
        Vector2 PrefabSize = BoardRenderer.GetTileSize();
        GridManager.InitializeGrid(PrefabSize);
        BoardRenderer.GenerateBoardVisuals(GridManager);
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        if (PlayerPrefab == null)
        {
            Debug.LogError("PlayerPrefab is null");
            return;
        }

        int StartX = 0;
        int StartY = 0;
        GridManager.GetPlayerStartIndex(out StartX, out StartY);
        Vector2 SpawnPosition = GridManager.GetWorldPosition(StartX, StartY);
        GameObject PlayerInstance = Instantiate(PlayerPrefab, SpawnPosition, Quaternion.identity);
        PlayerInstance.name = "Player";

        CurrentPlayer = PlayerInstance.GetComponent<PlayerController>();

        if (CurrentPlayer != null)
        {
            CurrentPlayer.GridManager = GridManager;
            CurrentPlayer.InputController = InputController;
            CurrentPlayer.CurrentX = StartX;
            CurrentPlayer.CurrentY = StartY;
            CurrentPlayer.TurnManager = TurnManager;
            CurrentPlayer.EnemyManager = EnemyManager;
            CurrentPlayer.ItemManager = ItemManager;
            GridManager.PlaceOccupant(CurrentPlayer, StartX, StartY);
        }

        HealthComponent PlayerHealth = PlayerInstance.GetComponent<HealthComponent>();
        if (PlayerHealth != null && HealthUIManager != null)
        {
            HealthUIManager.InitializeHealthUI(PlayerHealth);
            PlayerHealth.OnDied += GameOver;
        }

        if (CurrentPlayer != null)
        {
            CurrentPlayer.ApplyEquippedSkin();
        }
    }

    public void StartGame(bool IsRetry = false)
    {

        if (IsStartingGame) return;
        IsStartingGame = true;

        ChangeState(GameState.Initialize);

        float StartDelay = 0f;

        if (!IsRetry)
        {
            if (UITransition != null)
            {
                UITransition.SlideToGame();
                StartDelay = UITransition.SlideDuration;
            }
        }

        if (GameOverUI != null) GameOverUI.Hide();

        if (ScoreManager != null) ScoreManager.ResetScore();
        if (ItemManager != null) ItemManager.ClearItem();

        ClearEnemies();

        if (CurrentPlayer != null)
        {
            CurrentPlayer.gameObject.SetActive(true);
            CurrentPlayer.ResetPlayer();
        }

        if (AudioManager.Instance != null) AudioManager.Instance.PlayBGM(BgmType.InGame);

        if (InputController != null)
            InputController.SetTurnInputBlocked(true);

        DOVirtual.DelayedCall(StartDelay, () =>
        {
            TurnManager.StartGameLoop();
            IsStartingGame = false;
        });

        HasRevivedThisGame = false;
    }

    public void GameOver()
    {
        ChangeState(GameState.GameOver);

        if (InputController != null)
            InputController.SetTurnInputBlocked(true);

        int playCount = PlayerPrefs.GetInt("TotalPlayCount", 0) + 1;
        PlayerPrefs.SetInt("TotalPlayCount", playCount);
        PlayerPrefs.Save();

        if (AchievementManager.Instance != null)
            AchievementManager.Instance.ReportProgress(AchievementType.PlayCount, playCount);

        int currentScore = ScoreManager != null ? ScoreManager.CurrentScore : 0;
        int bestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
        bool isNewRecord = currentScore > bestScore;

        if (isNewRecord)
        {
            bestScore = currentScore;
            PlayerPrefs.SetInt(BestScoreKey, bestScore);
            PlayerPrefs.Save();

            if (FireBaseManager.Instance != null)
                FireBaseManager.Instance.SaveScoreToDatabase(currentScore);

            if (LobbyUI != null)
                LobbyUI.LoadBestScore();
        }

        if (FireBaseManager.Instance != null)
            FireBaseManager.Instance.SavePlayCount(playCount);

        if (ScoreManager != null)
        {
            int bestComboThisGame = ScoreManager.BestComboThisGame;
            int savedBestCombo = PlayerPrefs.GetInt("BestCombo", 0);

            if (bestComboThisGame > savedBestCombo)
            {
                PlayerPrefs.SetInt("BestCombo", bestComboThisGame);
                PlayerPrefs.Save();

                if (FireBaseManager.Instance != null)
                    FireBaseManager.Instance.SaveBestCombo(bestComboThisGame);
            }
        }

        if (GameOverUI != null)
            GameOverUI.Show(currentScore, bestScore, !HasRevivedThisGame);
    }

    public void OnClickAdReviveButton()
    {
        if (AdManager.Instance != null)
        {
            AdManager.Instance.ShowRewardedAd(OnReviveSuccess);
        }
        else
        {
            OnReviveSuccess();
        }
    }

    private void OnReviveSuccess()
    {
        HasRevivedThisGame = true;

        if (GameOverUI != null) GameOverUI.Hide();

        if (CurrentPlayer != null && CurrentPlayer.HealthComponent != null)
        {
            CurrentPlayer.HealthComponent.Revive(1);
        }

        if (ScoreManager != null)
        {
            ScoreManager.ResetCombo();
        }

        if (TurnManager != null) TurnManager.StartPlayerTurn();

        if (UITransition != null) UITransition.SlideToGame();
    }

    public void RetryGame()
    {
        StartGame(true);
    }

    public void OpenSettings()
    {
        if (SettingsUI != null) SettingsUI.Show();

        if (CurrentState != GameState.Lobby)
        {
            Time.timeScale = 0f;
        }
    }

    public void ReturnToLobby()
    {
        Time.timeScale = 1f;

        ChangeState(GameState.Lobby);

        if (InputController != null)
            InputController.SetTurnInputBlocked(true);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayBGM(BgmType.Lobby);

        ClearEnemies();

        if (CurrentPlayer != null) CurrentPlayer.ResetPlayer();

        if (UITransition != null)
            UITransition.SlideToLobby();

        if (GameOverUI != null) GameOverUI.Hide();

        if (ItemManager != null) ItemManager.ClearItem();

        if (LobbyUI != null) LobbyUI.LoadBestScore();
    }

    private void ClearEnemies()
    {
        if (TurnManager != null) TurnManager.StopAllCoroutines();
        if (EnemyManager != null) EnemyManager.StopAllCoroutines();

        if (EnemyManager != null)
        {
            foreach (EnemyBase Enemy in EnemyManager.ActiveEnemies)
            {
                if (Enemy != null)
                {
                    GridManager.RemoveOccupant(Enemy, Enemy.CurrentX, Enemy.CurrentY);
                    PoolManager.Instance.ReturnToPool(Enemy.gameObject);
                }
            }
            EnemyManager.ActiveEnemies.Clear();
        }
    }

    public void OpenCollectionUI()
    {
        if (CurrentState == GameState.Lobby)
        {
            if (CollectionUI != null && !CollectionUI.isActiveAndEnabled)
            {
                CollectionUI.Show();
            }
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus) return;

        if (AdManager.IsAdTransitioning) return;

        bool isPlaying = CurrentState == GameState.PlayerTurn
                        || CurrentState == GameState.EnemyTurn;

        if (!isPlaying) return;

        if (PauseMenu != null && !PauseMenu.gameObject.activeInHierarchy)
        {
            PauseMenu.Show();
        }
    }
}
