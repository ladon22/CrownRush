using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// �ȵ���̵� �ڷΰ��� ��ư(�ý��� ��) ó���� �߾ӿ��� �����մϴ�.
///
/// ���� ���� ? ����(Stack) ���
///   �� ��� �г��� ���� ��(Show) �ڽ��� �ݴ� ������ ���ÿ� Push�ϰ�,
///   ���� ��(Hide �Ϸ�) ���ÿ��� Pop�մϴ�.
///
///   �ڷΰ��⸦ ������ ������ ���� ��(���� �ֱٿ� �� �г�)�� �����ϴ�.
///   ��: �Ͻ����� �� ���� ������ �����ٸ�
///       �ڷΰ��� 1�� �� ������ ����
///       �ڷΰ��� 1�� �� �� �Ͻ����� ���� (�̾��ϱ�� ���� ����)
///
/// ������ ������� ��(���� �г��� ���� ��)��
/// HandleDefaultBack()���� ��Ȳ�� �⺻ ������ ó���մϴ�.
/// </summary>
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
        // �� Input System���� �ȵ���̵� �ڷΰ��� ��ư�� Escape Ű�� ���ε˴ϴ�.
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            HandleBackPressed();
        }
    }

    // ���� �гο��� ȣ���ϴ� API ����������������������������������������������������������������

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

    /// <summary>
    /// �����ִ� �г��� �ϳ��� ���� ���� �⺻ ����.
    /// ������Ʈ ��Ȳ�� �°� �����Ӱ� �����ϼ���.
    /// </summary>
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
