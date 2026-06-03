using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Применяет и сохраняет громкость каналов «Музыка» и «Эффекты» через AudioMixer.
/// Громкость хранится в PlayerPrefs как линейное значение 0..1 и переводится
/// в децибелы для миксера (логарифмическая шкала громкости).
///
/// Должен жить и в сцене меню (где слайдеры его дёргают), и в игровой сцене
/// (где он на старте просто применяет сохранённые значения к миксеру).
/// </summary>
public class AudioVolumeController : MonoBehaviour
{
    public const string MusicKey = "MusicVolume";
    public const string SFXKey = "SFXVolume";

    [Header("Mixer")]
    [SerializeField] private AudioMixer mixer;

    [Tooltip("Имя exposed-параметра группы музыки в AudioMixer.")]
    [SerializeField] private string musicExposedParam = "MusicVolume";

    [Tooltip("Имя exposed-параметра группы эффектов в AudioMixer.")]
    [SerializeField] private string sfxExposedParam = "SFXVolume";

    [Header("Defaults")]
    [Range(0f, 1f)]
    [SerializeField] private float defaultMusic = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float defaultSfx = 1f;

    public float MusicVolume { get; private set; }
    public float SFXVolume { get; private set; }

    private void Awake()
    {
        MusicVolume = PlayerPrefs.GetFloat(MusicKey, defaultMusic);
        SFXVolume = PlayerPrefs.GetFloat(SFXKey, defaultSfx);

        ApplyMusic(MusicVolume);
        ApplySFX(SFXVolume);
    }

    public void SetMusicVolume(float value)
    {
        MusicVolume = Mathf.Clamp01(value);
        ApplyMusic(MusicVolume);
        PlayerPrefs.SetFloat(MusicKey, MusicVolume);
    }

    public void SetSFXVolume(float value)
    {
        SFXVolume = Mathf.Clamp01(value);
        ApplySFX(SFXVolume);
        PlayerPrefs.SetFloat(SFXKey, SFXVolume);
    }

    private void ApplyMusic(float value)
    {
        if (mixer != null)
            mixer.SetFloat(musicExposedParam, LinearToDecibel(value));
    }

    private void ApplySFX(float value)
    {
        if (mixer != null)
            mixer.SetFloat(sfxExposedParam, LinearToDecibel(value));
    }

    /// <summary>
    /// 0..1 → дБ. При 0 отдаём -80 дБ (минимум микшера, фактически тишина),
    /// иначе 20·log10(value). При 1 это 0 дБ (без изменения громкости).
    /// </summary>
    private static float LinearToDecibel(float value)
    {
        return Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f;
    }
}
