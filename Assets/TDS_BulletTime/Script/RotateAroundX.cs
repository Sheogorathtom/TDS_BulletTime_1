using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    [Header("Default Settings")]
    public float defaultDuration = 0.1f;
    public float defaultMagnitude = 0.2f;

    private Vector3 _originalPos;
    private Coroutine _shakeRoutine;

    void Awake()
    {
        Instance = this;
        _originalPos = transform.localPosition;
    }

    public void Shake(float duration, float magnitude)
    {
        if (_shakeRoutine != null)
            StopCoroutine(_shakeRoutine);

        _shakeRoutine = StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    public void Shake()  // перегрузка с настройками по умолчанию
    {
        Shake(defaultDuration, defaultMagnitude);
    }

    private System.Collections.IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            Vector3 offset = Random.insideUnitSphere * magnitude;
            offset.z = 0f; // чтобы не двигать камеру вперёд/назад

            transform.localPosition = _originalPos + offset;

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = _originalPos;
        _shakeRoutine = null;
    }
}
