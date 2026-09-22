using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

[RequireComponent(typeof(RectTransform))]
public class LocalizationLayoutRefresher : MonoBehaviour
{
    [SerializeField] private float RefreshWindowSeconds = 0.5f;

    private RectTransform RootRect;
    private readonly List<LayoutGroup> LayoutGroupsDeepestFirst = new List<LayoutGroup>();

    private void Awake()
    {
        RootRect = gameObject.GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private void OnLocaleChanged(Locale locale)
    {
        StopAllCoroutines();
        StartCoroutine(RebuildLayoutForAWhile());
    }

    private IEnumerator RebuildLayoutForAWhile()
    {
        float elapsed = 0f;
        while (elapsed < RefreshWindowSeconds)
        {
            RebuildNestedLayoutGroups();
            yield return null;
            elapsed += Time.unscaledDeltaTime;
        }
    }

    private void RebuildNestedLayoutGroups()
    {
        // Unity의 LayoutRebuilder는 중첩된 ContentSizeFitter+LayoutGroup 조합에서
        // 상위 하나만 강제 재계산해도 안쪽까지 제대로 전파되지 않는 알려진 한계가 있어,
        // 가장 안쪽(자식) 레이아웃 그룹부터 바깥쪽(부모) 순서로 개별 재계산한다.
        Canvas.ForceUpdateCanvases();

        LayoutGroupsDeepestFirst.Clear();
        RootRect.GetComponentsInChildren(true, LayoutGroupsDeepestFirst);
        LayoutGroupsDeepestFirst.Sort((a, b) => GetDepth(b.transform) - GetDepth(a.transform));

        foreach (var group in LayoutGroupsDeepestFirst)
            LayoutRebuilder.ForceRebuildLayoutImmediate(group.transform as RectTransform);

        LayoutRebuilder.ForceRebuildLayoutImmediate(RootRect);
    }

    private static int GetDepth(Transform t)
    {
        int depth = 0;
        while (t != null)
        {
            depth++;
            t = t.parent;
        }
        return depth;
    }
}
