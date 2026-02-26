using UnityEngine;

public class SpriteDepthSorter : MonoBehaviour
{
    private SpriteRenderer TargetRenderer;
    public int SortingOrderOffset = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TargetRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (TargetRenderer != null)
        {
            TargetRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100f) + SortingOrderOffset;
        }
    }
}
