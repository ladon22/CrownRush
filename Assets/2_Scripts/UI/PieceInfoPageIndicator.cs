using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PieceInfoPageIndicator : MonoBehaviour
{
    [SerializeField] private Transform Container;
    [SerializeField] private Image DotPrefab;
    [SerializeField] private Color ActiveColor = Color.white;
    [SerializeField] private Color InactiveColor = new Color(1f, 1f, 1f, 0.35f);

    private readonly List<Image> Dots = new List<Image>();

    public void Setup(int count)
    {
        foreach (Image dot in Dots)
        {
            if (dot != null) Destroy(dot.gameObject);
        }
        Dots.Clear();

        if (DotPrefab == null || Container == null) return;

        for (int i = 0; i < count; i++)
            Dots.Add(Instantiate(DotPrefab, Container));
    }

    public void SetIndex(int index)
    {
        for (int i = 0; i < Dots.Count; i++)
            Dots[i].color = (i == index) ? ActiveColor : InactiveColor;
    }
}
