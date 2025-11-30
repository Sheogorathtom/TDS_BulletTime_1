using System.Collections;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health enemyHealth;
    [SerializeField] private AudioSource audioSource;

    [Header("Music Clips")]
    [SerializeField] private AudioClip phase1Clip;      // стартовая музыка (один раз)
    [SerializeField] private AudioClip phase2LoopClip;  // луп до 30 HP
    [SerializeField] private AudioClip phase3LoopClip;  // луп при <= 30 HP
    [SerializeField] private AudioClip victoryClip;     // музыка после смерти врага

    [Header("Settings")]
    [SerializeField] private float phase3HealthThreshold = 30f;
    [SerializeField] private float fadeDuration = 1.0f; // время плавного перехода

    private bool _phase2Started = false;
    private bool _phase3Started = false;
    private bool _victoryPlayed = false;

    private Coroutine _fadeCoroutine;
    private float _defaultVolume = 1f;

    void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource != null)
            _defaultVolume = audioSource.volume;
    }

    void Start()
    {
        // Сразу играем первую музыку один раз, без лупа
        if (phase1Clip != null && audioSource != null)
        {
            audioSource.loop = false;
            audioSource.clip = phase1Clip;
            audioSource.volume = _defaultVolume;
            audioSource.Play();
        }
    }

    void Update()
    {
        if (enemyHealth == null || audioSource == null)
            return;

        // Переход от первой к второй
        if (!_phase2Started)
        {
            if (!audioSource.isPlaying)
            {
                StartPhase2Loop();
            }
            return;
        }

        // Переход ко второй фазе (третья музыка) по HP
        if (!_phase3Started)
        {
            if (enemyHealth.GetCurrentHealth() <= phase3HealthThreshold && !enemyHealth.IsDead())
            {
                StartPhase3Loop();
            }
        }

        // Музыка победы
        if (!_victoryPlayed)
        {
            if (enemyHealth.IsDead())
            {
                PlayVictory();
            }
        }
    }

    private void StartPhase2Loop()
    {
        if (phase2LoopClip == null) return;

        _phase2Started = true;
        CrossfadeToClip(phase2LoopClip, true);
    }

    private void StartPhase3Loop()
    {
        if (phase3LoopClip == null) return;

        _phase3Started = true;
        CrossfadeToClip(phase3LoopClip, true);
    }

    private void PlayVictory()
    {
        if (victoryClip == null) return;

        _victoryPlayed = true;
        CrossfadeToClip(victoryClip, false);
    }

    private void CrossfadeToClip(AudioClip newClip, bool loop)
    {
        if (audioSource == null || newClip == null) return;

        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);

        _fadeCoroutine = StartCoroutine(FadeRoutine(newClip, loop));
    }

    private IEnumerator FadeRoutine(AudioClip newClip, bool loop)
    {
        float startVolume = audioSource.volume;
        float t = 0f;

        // Фейд-аут старого трека
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float k = t / fadeDuration;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, k);
            yield return null;
        }

        audioSource.Stop();
        audioSource.clip = newClip;
        audioSource.loop = loop;
        audioSource.Play();

        // Фейд-ин нового трека
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float k = t / fadeDuration;
            audioSource.volume = Mathf.Lerp(0f, _defaultVolume, k);
            yield return null;
        }

        audioSource.volume = _defaultVolume;
    }
}
