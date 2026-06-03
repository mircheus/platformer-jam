using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Центральная точка всего звука в игре. Синглтон, переживает загрузку сцен
/// (DontDestroyOnLoad), поэтому музыка и ambient не прерываются на границе
/// Меню ↔ Игра. Внутри локаций сцена не перезагружается (LocationTransitionController
/// просто переключает объекты), так что там звук непрерывен автоматически.
///
/// Все источники создаются в рантайме (см. <see cref="BuildSources"/>) — в инспекторе
/// нужно лишь назначить группы AudioMixer и, по желанию, стартовые клипы. Громкость
/// каналов рулит <see cref="AudioVolumeController"/> через dB-параметры микшера;
/// здесь же фейды идут через AudioSource.volume (0..1), чтобы не конфликтовать с
/// пользовательской настройкой — это два независимых множителя.
///
/// Категории:
///  • Ambient   — один зацикленный источник, играет всегда;
///  • Music     — два источника для кроссфейда (плавная смена трека) и fade in/out;
///  • SFX       — разовые звуки (получение предмета и пр.) через PlayOneShot;
///  • Voice     — фразы персонажей в диалоге (новая обрывает предыдущую);
///  • MoveLoop  — луп шагов, пока персонаж движется (вкл/выкл с микрофейдом от щелчков).
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Mixer routing")]
    [SerializeField] private AudioMixerGroup musicGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;
    [Tooltip("Группа для ambient. Если пусто — уйдёт в группу музыки.")]
    [SerializeField] private AudioMixerGroup ambientGroup;

    [Header("Стартовые клипы (опционально)")]
    [Tooltip("Зацикленный фоновый ambient. Запускается на старте, если задан.")]
    [SerializeField] private AudioClip ambientClip;
    [Tooltip("Музыка, играющая по умолчанию при старте. Пусто — тишина до первого PlayMusic.")]
    [SerializeField] private AudioClip startMusic;

    [Header("Параметры")]
    [Range(0f, 1f)]
    [Tooltip("Базовая громкость музыки внутри менеджера (поверх неё работает громкость микшера).")]
    [SerializeField] private float musicVolume = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float ambientVolume = 1f;
    [Tooltip("Длительность кроссфейда/fade музыки по умолчанию, сек.")]
    [SerializeField] private float defaultMusicFade = 1.5f;
    [Tooltip("Микрофейд старта/останова лупа шагов — убирает щелчок, сек.")]
    [SerializeField] private float moveLoopFade = 0.08f;

    // Источники, созданные в рантайме.
    private AudioSource ambientSource;
    private AudioSource[] musicSources;   // 0 и 1 — для кроссфейда
    private AudioSource sfxSource;        // PlayOneShot, миксует несколько звуков
    private AudioSource voiceSource;      // фразы персонажей
    private AudioSource moveLoopSource;   // луп шагов

    private int activeMusic;              // индекс текущего музыкального источника
    private AudioClip currentMusicClip;   // что играет/играло последним (для SetMusicEnabled)
    private bool musicEnabled = true;

    private Coroutine musicRoutine;
    private Coroutine moveLoopRoutine;

    public bool MusicEnabled => musicEnabled;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildSources();
    }

    private void Start()
    {
        if (ambientClip != null)
            PlayAmbient(ambientClip);

        if (startMusic != null)
            PlayMusic(startMusic);
    }

    /// <summary>Создаёт все AudioSource в коде и направляет их в нужные группы микшера.</summary>
    private void BuildSources()
    {
        AudioMixerGroup ambientOut = ambientGroup != null ? ambientGroup : musicGroup;

        ambientSource = CreateSource("Ambient", musicGroup: ambientOut, loop: true);

        musicSources = new[]
        {
            CreateSource("Music A", musicGroup: musicGroup, loop: true),
            CreateSource("Music B", musicGroup: musicGroup, loop: true),
        };

        sfxSource = CreateSource("SFX", musicGroup: sfxGroup, loop: false);
        voiceSource = CreateSource("Voice", musicGroup: sfxGroup, loop: false);
        moveLoopSource = CreateSource("MoveLoop", musicGroup: sfxGroup, loop: true);
    }

    private AudioSource CreateSource(string label, AudioMixerGroup musicGroup, bool loop)
    {
        var go = new GameObject(label);
        go.transform.SetParent(transform, false);

        AudioSource source = go.AddComponent<AudioSource>();
        source.outputAudioMixerGroup = musicGroup;
        source.playOnAwake = false;
        source.loop = loop;
        source.spatialBlend = 0f; // 2D: звук не зависит от позиции в платформере
        source.volume = 0f;

        return source;
    }

    // ───────────────────────────── Ambient ─────────────────────────────

    /// <summary>Запускает (или меняет) зацикленный ambient. Играет до явной остановки.</summary>
    public void PlayAmbient(AudioClip clip)
    {
        if (clip == null)
            return;

        ambientSource.clip = clip;
        ambientSource.volume = ambientVolume;
        ambientSource.Play();
    }

    public void StopAmbient() => ambientSource.Stop();

    // ────────────────────────────── Music ──────────────────────────────

    /// <summary>
    /// Плавно переходит на новый трек (кроссфейд). Если этот трек уже играет — ничего
    /// не делает. Передай fadeDuration &lt; 0, чтобы взять значение по умолчанию.
    /// </summary>
    public void PlayMusic(AudioClip clip, float fadeDuration = -1f)
    {
        if (clip == null)
        {
            StopMusic(fadeDuration);
            return;
        }

        musicEnabled = true;
        currentMusicClip = clip;

        AudioSource active = musicSources[activeMusic];
        if (active.isPlaying && active.clip == clip)
            return; // уже играет нужное

        float fade = fadeDuration < 0f ? defaultMusicFade : fadeDuration;
        int next = 1 - activeMusic;

        AudioSource incoming = musicSources[next];
        incoming.clip = clip;
        incoming.volume = 0f;
        incoming.Play();

        activeMusic = next;
        RestartMusicRoutine(CrossfadeRoutine(incoming, active, fade));
    }

    /// <summary>Плавно глушит музыку и останавливает источник.</summary>
    public void StopMusic(float fadeDuration = -1f)
    {
        musicEnabled = false;

        float fade = fadeDuration < 0f ? defaultMusicFade : fadeDuration;
        AudioSource active = musicSources[activeMusic];
        RestartMusicRoutine(FadeOutAndStop(active, fade));
    }

    /// <summary>
    /// Вкл/выкл музыки через fade — для кнопки в меню. При включении возобновляет
    /// последний (или стартовый) трек.
    /// </summary>
    public void SetMusicEnabled(bool enabled, float fadeDuration = -1f)
    {
        if (enabled)
        {
            AudioClip clip = currentMusicClip != null ? currentMusicClip : startMusic;
            PlayMusic(clip, fadeDuration);
        }
        else
        {
            StopMusic(fadeDuration);
        }
    }

    public void ToggleMusic(float fadeDuration = -1f) => SetMusicEnabled(!musicEnabled, fadeDuration);

    private void RestartMusicRoutine(IEnumerator routine)
    {
        if (musicRoutine != null)
            StopCoroutine(musicRoutine);

        musicRoutine = StartCoroutine(routine);
    }

    private IEnumerator CrossfadeRoutine(AudioSource incoming, AudioSource outgoing, float duration)
    {
        yield return FadePair(incoming, musicVolume, outgoing, 0f, duration);

        if (outgoing != incoming)
            outgoing.Stop();

        musicRoutine = null;
    }

    private IEnumerator FadeOutAndStop(AudioSource source, float duration)
    {
        float from = source.volume;
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(from, 0f, t / duration);
            yield return null;
        }

        source.volume = 0f;
        source.Stop();
        musicRoutine = null;
    }

    /// <summary>Параллельно ведёт два источника к их целевым громкостям на unscaled-времени.</summary>
    private IEnumerator FadePair(AudioSource a, float aTarget, AudioSource b, float bTarget, float duration)
    {
        float aFrom = a.volume;
        float bFrom = b.volume;
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = duration > 0f ? t / duration : 1f;
            a.volume = Mathf.Lerp(aFrom, aTarget, k);
            b.volume = Mathf.Lerp(bFrom, bTarget, k);
            yield return null;
        }

        a.volume = aTarget;
        b.volume = bTarget;
    }

    // ─────────────────────────────── SFX ───────────────────────────────

    /// <summary>Разовый звук (получение предмета и т.п.). Несколько вызовов микшируются.</summary>
    public void PlaySfx(AudioClip clip, float volume = 1f)
    {
        if (clip != null)
            sfxSource.PlayOneShot(clip, Mathf.Clamp01(volume));
    }

    // ────────────────────────────── Voice ──────────────────────────────

    /// <summary>
    /// Фраза персонажа в диалоге. Новая фраза обрывает предыдущую, чтобы при быстром
    /// проматывании реплик голоса не накладывались.
    /// </summary>
    public void PlayVoice(AudioClip clip)
    {
        if (clip == null)
            return;

        voiceSource.Stop();
        voiceSource.volume = 1f;
        voiceSource.clip = clip;
        voiceSource.Play();
    }

    public void StopVoice() => voiceSource.Stop();

    // ───────────────────────────── MoveLoop ────────────────────────────

    /// <summary>
    /// Включает луп шагов под заданный клип (если ещё не играет). Безопасно вызывать
    /// каждый кадр — повторные вызовы с тем же клипом игнорируются.
    /// </summary>
    public void StartMoveLoop(AudioClip clip)
    {
        if (clip == null)
            return;

        if (moveLoopSource.isPlaying && moveLoopSource.clip == clip)
            return;

        moveLoopSource.clip = clip;
        moveLoopSource.volume = 0f;
        moveLoopSource.Play();
        RestartMoveLoopRoutine(FadeMoveLoop(1f, stopAtEnd: false));
    }

    /// <summary>Глушит луп шагов микрофейдом и останавливает источник.</summary>
    public void StopMoveLoop()
    {
        if (!moveLoopSource.isPlaying)
            return;

        RestartMoveLoopRoutine(FadeMoveLoop(0f, stopAtEnd: true));
    }

    private void RestartMoveLoopRoutine(IEnumerator routine)
    {
        if (moveLoopRoutine != null)
            StopCoroutine(moveLoopRoutine);

        moveLoopRoutine = StartCoroutine(routine);
    }

    private IEnumerator FadeMoveLoop(float target, bool stopAtEnd)
    {
        float from = moveLoopSource.volume;
        float t = 0f;

        while (t < moveLoopFade)
        {
            t += Time.unscaledDeltaTime;
            moveLoopSource.volume = Mathf.Lerp(from, target, t / moveLoopFade);
            yield return null;
        }

        moveLoopSource.volume = target;

        if (stopAtEnd)
            moveLoopSource.Stop();

        moveLoopRoutine = null;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
