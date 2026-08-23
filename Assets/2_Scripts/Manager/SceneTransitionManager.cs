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
    public Image FadeImage;
    public float FadeDuration = 0.5f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            Canvas myCanvas = GetComponent<Canvas>();
            if (myCanvas != null)
            {
                myCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                myCanvas.sortingOrder = 32767;
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
        FadeImage.DOKill();
        FadeImage.gameObject.SetActive(true);
        FadeImage.raycastTarget = true;

        FadeImage.DOFade(1f, FadeDuration).SetUpdate(true).OnComplete(() =>
        {
            StartCoroutine(LoadSceneRoutine(sceneIndex));
        });
    }

    private IEnumerator LoadSceneRoutine(int sceneIndex)
    {
        yield return null;
        SceneManager.LoadScene(sceneIndex);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(DelayedFadeInRoutine());
    }

    private IEnumerator DelayedFadeInRoutine()
    {
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        yield return null;

        FadeIn();
    }

    private void FadeIn()
    {
        FadeImage.DOKill();

        Color c = FadeImage.color;
        c.a = 1f;
        FadeImage.color = c;
        FadeImage.gameObject.SetActive(true);

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