using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIBackHandler : MonoBehaviour
{
    public static UIBackHandler Instance;

    private readonly Stack<Action> BackStack = new Stack<Action>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(transform.root.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            HandleBackPressed();
        }
    }

    public void Push(Action onBackPressed)
    {
        BackStack.Push(onBackPressed);
    }

    public void Pop()
    {
        if (BackStack.Count > 0)
            BackStack.Pop();
    }

    private void HandleBackPressed()
    {
        if (BackStack.Count > 0)
            BackStack.Peek()?.Invoke();
        else
            HandleDefaultBack();
    }

    private void HandleDefaultBack()
    {
        if (GameManager.Instance == null) return;

        switch (GameManager.Instance.CurrentState)
        {
            case GameState.Lobby:
                if (GameManager.Instance.ExitConfirmPanel != null)
                    GameManager.Instance.ExitConfirmPanel.Show();
                break;

            case GameState.PlayerTurn:
            case GameState.EnemyTurn:
                if (GameManager.Instance.PauseMenu != null)
                    GameManager.Instance.PauseMenu.Show();
                break;
        }
    }
}
