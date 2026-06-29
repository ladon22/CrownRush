using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 안드로이드 뒤로가기 버튼(시스템 백) 처리를 중앙에서 관리합니다.
///
/// 동작 원리 ? 스택(Stack) 기반
///   각 모달 패널은 열릴 때(Show) 자신을 닫는 동작을 스택에 Push하고,
///   닫힐 때(Hide 완료) 스택에서 Pop합니다.
///
///   뒤로가기를 누르면 스택의 가장 위(가장 최근에 연 패널)만 닫힙니다.
///   예: 일시정지 → 설정 순으로 열었다면
///       뒤로가기 1번 → 설정만 닫힘
///       뒤로가기 1번 더 → 일시정지 닫힘 (이어하기와 동일 동작)
///
/// 스택이 비어있을 때(열린 패널이 없을 때)는
/// HandleDefaultBack()에서 상황별 기본 동작을 처리합니다.
/// </summary>
public class UIBackHandler : MonoBehaviour
{
    public static UIBackHandler Instance;

    private readonly Stack<Action> _backStack = new Stack<Action>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // 새 Input System에서 안드로이드 뒤로가기 버튼은 Escape 키로 매핑됩니다.
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            HandleBackPressed();
        }
    }

    // ── 패널에서 호출하는 API ────────────────────────────────

    /// <summary>패널이 열릴 때(Show) 호출합니다. 뒤로가기 시 실행할 동작을 등록합니다.</summary>
    public void Push(Action onBackPressed)
    {
        _backStack.Push(onBackPressed);
    }

    /// <summary>패널이 완전히 닫혔을 때(Hide 완료) 호출합니다.</summary>
    public void Pop()
    {
        if (_backStack.Count > 0)
            _backStack.Pop();
    }

    // ── 내부 처리 ─────────────────────────────────────────────

    private void HandleBackPressed()
    {
        if (_backStack.Count > 0)
        {
            // 가장 최근에 연 패널의 닫기 동작만 실행 (Peek ? 실제 Pop은 그 패널의 Hide 완료 시점에)
            _backStack.Peek()?.Invoke();
        }
        else
        {
            HandleDefaultBack();
        }
    }

    /// <summary>
    /// 열려있는 패널이 하나도 없을 때의 기본 동작.
    /// 프로젝트 상황에 맞게 자유롭게 수정하세요.
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
                // 인게임 중 열린 패널이 없으면 일시정지 메뉴를 띔
                // (톱니바퀴 버튼을 누른 것과 동일한 동작)
                if (GameManager.Instance.PauseMenu != null)
                    GameManager.Instance.PauseMenu.Show();
                break;
        }
    }
}
