using UnityEngine;

public enum GameState
{
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

    public GameObject PlayerPrefab;

    void Start()
    {
        InitiallizeGameStage();
        TurnManager.StartGameLoop();
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

        PlayerController Controller = PlayerInstance.GetComponent<PlayerController>();
        if (Controller != null)
        {
            Controller.GridManager = GridManager;
            Controller.InputController = InputController;
            Controller.CurrentX = StartX;
            Controller.CurrentY = StartY;
            Controller.TurnManager = TurnManager;
            Controller.EnemyManager = EnemyManager;
        }

    }
}
