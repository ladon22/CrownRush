using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    [Header("UI")]
    public Canvas TransitionCanvas;
    public Image FadeImage; // 화면을 덮을 검은색 이미지
    public float FadeDuration = 0.5f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            // 1. 이 캔버스가 무조건 화면의 맨 앞(유리창)에 오도록 코드로 강제 설정합니다!
            Canvas myCanvas = GetComponent<Canvas>();
            if (myCanvas != null)
            {
                myCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                myCanvas.sortingOrder = 32767; // 유니티에서 설정 가능한 가장 높은 순서
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void TransitionToScene(int sceneIndex)
    {
        FadeImage.DOKill(); // 2. 실행 중인 다른 애니메이션이 있다면 강제 종료
        FadeImage.gameObject.SetActive(true);
        FadeImage.raycastTarget = true;

        FadeImage.DOFade(1f, FadeDuration).SetUpdate(true).OnComplete(() =>
        {
            // 3. DOTween 충돌을 막기 위해 코루틴을 통해 딱 1프레임 쉰 뒤에 씬을 넘깁니다!
            StartCoroutine(LoadSceneRoutine(sceneIndex));
        });
    }

    private IEnumerator LoadSceneRoutine(int sceneIndex)
    {
        yield return null; // 1프레임 대기 (마법의 타이밍 해결사)
        SceneManager.LoadScene(sceneIndex);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(DelayedFadeInRoutine());
    }

    // 무거운 초기화 작업(Instantiate 등)이 지나가기를 기다려주는 코루틴
    private IEnumerator DelayedFadeInRoutine()
    {
        // GameManager, BoardRenderer 등의 Awake()와 Start()가 
        // 렉(Spike)을 유발하며 실행될 시간을 벌어줍니다.
        yield return null;
        yield return null;
        yield return null; // 3프레임 정도 쉬어주면 초기화 렉이 완벽히 끝납니다.
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        yield return null; // 3프레임 정도 쉬어주면 초기화 렉이 완벽히 끝납니다.
        yield return null;
        yield return null;

        // 렉이 완전히 풀린 부드러운 상태에서 스르륵 밝아집니다!
        FadeIn();
    }

    private void FadeIn()
    {
        FadeImage.DOKill();

        // 4. 페이드인 시작 전에 이미지의 알파값을 무조건 1(완전 검은색)로 고정시킵니다.
        Color c = FadeImage.color;
        c.a = 1f;
        FadeImage.color = c;
        FadeImage.gameObject.SetActive(true);

        // 스르륵 다시 밝아지게 만듭니다.
        FadeImage.DOFade(0f, FadeDuration).SetUpdate(true).OnComplete(() =>
        {
            FadeImage.gameObject.SetActive(false);
            FadeImage.raycastTarget = false;
        });
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}