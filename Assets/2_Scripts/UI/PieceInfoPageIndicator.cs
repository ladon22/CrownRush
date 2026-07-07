using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 카드 캐러셀의 현재 위치를 점(dot)으로 표시합니다.
/// </summary>
public class PieceInfoPageIndicator : MonoBehaviour
{
    [SerializeField] private Transform Container;
    [SerializeField] private Image DotPrefab;
    [SerializeField] private Color ActiveColor = Color.white;
    [SerializeField] private Color InactiveColor = new Color(1f, 1f, 1f, 0.35f);

    private readonly List<Image> _dots = new List<Image>();

    public void Setup(int count)
    {
        foreach (Image dot in _dots)
        {
            if (dot != null) Destroy(dot.gameObject);
        }
        _dots.Clear();

        if (DotPrefab == null || Container == null) return;

        for (int i = 0; i < count; i++)
            _dots.Add(Instantiate(DotPrefab, Container));
    }

    public void SetIndex(int index)
    {
        for (int i = 0; i < _dots.Count; i++)
            _dots[i].color = (i == index) ? ActiveColor : InactiveColor;
    }
}
