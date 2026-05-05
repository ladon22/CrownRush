using DG.Tweening;
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
    public GameState CurrentState;

    public GridManager GridManager;
    public BoardRenderer BoardRenderer;
    public InputController InputController;
    public TurnManager TurnManager;
    public EnemyManager EnemyManager;
    public HealthUIManager HealthUIManager;
    public ItemManager ItemManager;
    public ScoreManager ScoreManager;
    //public FireBaseManager FireBaseManager;

    public GameObject PlayerPrefab;

    [Header("UI")]
    public UITransitionController UITransition;
    public GameObject LobbyUIPanel;
    public GameObject GameUIPanel;
    public GameOverUIController GameOverUI;
    public SettingsUIController SettingsUI;

    private bool IsInitialized = false;
    private PlayerController CurrentPlayer;

    public CollectionUIController CollectionUI;
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
        //if (GameUIPanel != null) GameUIPanel.SetActive(false);
    }

    void Update()
    {
        // 유니티 에디터에서 플레이 중일 때만 작동하는 안전한 치트키입니다.
#if UNITY_EDITOR
        if (Keyboard.current != null && Keyboard.current.f12Key.wasPressedThisFrame)
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("[Cheat] 모든 데이터 초기화 완료! 씬을 재시작합니다.");

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
        }

        HealthComponent PlayerHealth = PlayerInstance.GetComponent<HealthComponent>();
        if (PlayerHealth != null && HealthUIManager != null)
        {
            HealthUIManager.InitializeHealthUI(PlayerHealth);
            PlayerHealth.OnDied += GameOver;
        }

        // 플레이어가 생성된 직후, 현재 장착 중인 스킨을 입혀줍니다.
        if (CurrentPlayer != null)
        {
            CurrentPlayer.ApplyEquippedSkin();
        }
    }

    public void StartGame(bool IsRetry = false)
    {
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

        //if (LobbyUIPanel != null) LobbyUIPanel.SetActive(false);
        //if (GameUIPanel != null) GameUIPanel.SetActive(true);

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

        DOVirtual.DelayedCall(StartDelay, () =>
        {
            TurnManager.StartGameLoop();
        });
    }

    public void GameOver()
    {
        ChangeState(GameState.GameOver);

        // 게임 오버가 될 때마다 누적 플레이 횟수를 증가시키고 보고합니다.
        int PlayCount = PlayerPrefs.GetInt("TotalPlayCount", 0) + 1;
        PlayerPrefs.SetInt("TotalPlayCount", PlayCount);
        PlayerPrefs.Save();

        if (AchievementManager.Instance != null)
        {
            AchievementManager.Instance.ReportProgress(AchievementType.PlayCount, PlayCount);
        }


        int CurrentScore = ScoreManager != null ? ScoreManager.CurrentScore : 0;
        bool bIsNewRecord = false;
        if (GameOverUI != null) 
            bIsNewRecord = GameOverUI.Show(CurrentScore);

        if (bIsNewRecord && ScoreManager != null && FireBaseManager.Instance != null)
        {
            FireBaseManager.Instance.SaveScoreToDatabase(CurrentScore);
        }
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

    public void CloseSettings()
    {
        if (SettingsUI != null) SettingsUI.Hide();
        Time.timeScale = 1f;
    }

    public void ReturnToLobby()
    {
        Time.timeScale = 1f;

        ChangeState(GameState.Lobby);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayBGM(BgmType.Lobby);

        ClearEnemies();

        //if (BoardRenderer != null) BoardRenderer.gameObject.SetActive(false);
        //if (CurrentPlayer != null) CurrentPlayer.gameObject.SetActive(false);

        if (CurrentPlayer != null) CurrentPlayer.ResetPlayer();

        //if (LobbyUIPanel != null) LobbyUIPanel.SetActive(true);
        //if (GameUIPanel != null) GameUIPanel.SetActive(false);

        if (UITransition != null)
            UITransition.SlideToLobby();

        if (GameOverUI != null) GameOverUI.Hide();

        if (ItemManager != null) ItemManager.ClearItem();
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
                    PoolManager.Instance.ReturnToPool(Enemy.gameObject);
                }
            }
            EnemyManager.ActiveEnemies.Clear();
        }
    }

    public void OpenCollectionUI()
    {
        // 로비 상태일 때만 열리도록 방어 코드 (게임 중엔 안 열리게)
        if (CurrentState == GameState.Lobby)
        {
            if (CollectionUI != null && !CollectionUI.isActiveAndEnabled)
            {
                CollectionUI.Show();
            }
        }
    }
}
