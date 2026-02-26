using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;
    private Vector3 OriginalPosition;

    [Header("Attack Shake Settings")]
    public float AttackShakeDuration = 0.1f;
    public float AttackShakeMagnitude = 0.15f;

    [Header("Hit Shake Settings")]
    public float HitShakeDuration = 0.3f;
    public float HitShakeMagnitude = 0.4f;

    private void Awake()
    {
        Instance = this;
        OriginalPosition = transform.position;
    }

    public void ShakeForAttack()
    {
        StopAllCoroutines();
        StartCoroutine(ShakeRoutine(AttackShakeDuration, AttackShakeMagnitude));
    }

    public void ShakeForHit()
    {
        StopAllCoroutines();
        StartCoroutine(ShakeRoutine(HitShakeDuration, HitShakeMagnitude));
    }

    private IEnumerator ShakeRoutine(float Duration, float Magnitude)
    {
        float Elapsed = 0.0f;

        while (Elapsed < Duration)
        {
            float X = Random.Range(-1f, 1f) * Magnitude;
            float Y = Random.Range(-1f, 1f) * Magnitude;

            transform.position = new Vector3(OriginalPosition.x + X, OriginalPosition.y + Y, OriginalPosition.z);

            Elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = OriginalPosition;
    }
}
