using UnityEngine;

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
    public ScoreManager ScoreManager;

    public GameObject PlayerPrefab;

    [Header("UI")]
    public GameObject LobbyUIPanel;
    public GameObject GameUIPanel;
    public GameObject GameOverUIPanel;

    private bool IsInitialized = false;
    private PlayerController CurrentPlayer;
    void Start()
    {
        ChangeState(GameState.Lobby);

        if (LobbyUIPanel != null) LobbyUIPanel.SetActive(true);
        if (GameUIPanel != null) GameUIPanel.SetActive(false);
        if (GameOverUIPanel != null) GameOverUIPanel.SetActive(false);
    }

    public void ChangeState(GameState state)
    {
        CurrentState = state;
        switch (CurrentState)
        {
            case GameState.Initialize:
                break;
            case GameState.PlayerTurn:
                break;
            case GameState.EnemyTurn:
                break;
            case GameState.GameOver:
                break;
            default:
                break;
        }
    }

    private void InitiallizeGameStage()
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
        }

        HealthComponent PlayerHealth = PlayerInstance.GetComponent<HealthComponent>();
        if (PlayerHealth != null && HealthUIManager != null)
        {
            HealthUIManager.InitializeHealthUI(PlayerHealth);
            PlayerHealth.OnDied += GameOver;
        }
    }

    public void StartGame()
    {
        ChangeState(GameState.Initialize);

        if (LobbyUIPanel != null) LobbyUIPanel.SetActive(false);
        if (GameUIPanel != null) GameUIPanel.SetActive(true);
        if (GameOverUIPanel != null) GameOverUIPanel.SetActive(false);

        if (ScoreManager != null) ScoreManager.ResetScore();

        ClearEnemies();

        if (!IsInitialized)
        {
            InitiallizeGameStage();
            IsInitialized = true;
        }
        else
        {
            if (BoardRenderer != null) BoardRenderer.gameObject.SetActive(true);

            if (CurrentPlayer != null)
            {
                CurrentPlayer.gameObject.SetActive(true);
                CurrentPlayer.ResetPlayer();
            }
        }

        TurnManager.StartGameLoop();
    }

    public void GameOver()
    {
        ChangeState(GameState.GameOver);
        if (GameOverUIPanel != null) GameOverUIPanel.SetActive(true);
    }

    public void RetryGame()
    {
        StartGame();
    }

    public void ReturnToLobby()
    {
        ChangeState(GameState.Lobby);

        ClearEnemies();

        if (BoardRenderer != null) BoardRenderer.gameObject.SetActive(false);
        if (CurrentPlayer != null) CurrentPlayer.gameObject.SetActive(false);

        if (LobbyUIPanel != null) LobbyUIPanel.SetActive(true);
        if (GameUIPanel != null) GameUIPanel.SetActive(false);
        if (GameOverUIPanel != null) GameOverUIPanel.SetActive(false);
    }

    private void ClearEnemies()
    {
        if (TurnManager != null) TurnManager.StopAllCoroutines();
        if (EnemyManager != null) EnemyManager.StopAllCoroutines();

        if (EnemyManager != null)
        {
            foreach (EnemyBase Enemy in EnemyManager.ActiveEnemies)
            {
                if (Enemy != null) Destroy(Enemy.gameObject);
            }
            EnemyManager.ActiveEnemies.Clear();
        }
    }
}
