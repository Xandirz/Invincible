using UnityEngine;

public enum GameSound
{
    CardAddedToHand,
    CardAddedToGenerator,
    ProjectileShot,
    EnemyDied,
    LightningCast
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource cardAddedToHandSource;
    [SerializeField] private AudioSource cardAddedToGeneratorSource;
    [SerializeField] private AudioSource projectileShotSource;
    [SerializeField] private AudioSource enemyDiedSource;
    [SerializeField] private AudioSource lightningCastSource;

    [Header("Clips")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioClip cardAddedToHandClip;
    [SerializeField] private AudioClip cardAddedToGeneratorClip;
    [SerializeField] private AudioClip projectileShotClip;
    [SerializeField] private AudioClip enemyDiedClip;
    [SerializeField] private AudioClip lightningCastClip;

    [Header("Volume")]
    [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.6f;
    [SerializeField] [Range(0f, 1f)] private float cardAddedToHandVolume = 1f;
    [SerializeField] [Range(0f, 1f)] private float cardAddedToGeneratorVolume = 1f;
    [SerializeField] [Range(0f, 1f)] private float projectileShotVolume = 1f;
    [SerializeField] [Range(0f, 1f)] private float enemyDiedVolume = 1f;
    [SerializeField] [Range(0f, 1f)] private float lightningCastVolume = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetupSources();
        ApplyVolumes();
    }

    private void Start()
    {
        PlayBackgroundMusic();
    }

    private void OnValidate()
    {
        ApplyVolumes();
    }

    private void SetupSources()
    {
        musicSource = EnsureSource(musicSource, "MusicSource", true);
        cardAddedToHandSource = EnsureSource(cardAddedToHandSource, "CardAddedToHandSource", false);
        cardAddedToGeneratorSource = EnsureSource(cardAddedToGeneratorSource, "CardAddedToGeneratorSource", false);
        projectileShotSource = EnsureSource(projectileShotSource, "ProjectileShotSource", false);
        enemyDiedSource = EnsureSource(enemyDiedSource, "EnemyDiedSource", false);
        lightningCastSource = EnsureSource(lightningCastSource, "LightningCastSource", false);
    }

    private AudioSource EnsureSource(AudioSource source, string childName, bool loop)
    {
        if (source == null)
        {
            Transform child = transform.Find(childName);

            if (child == null)
            {
                GameObject obj = new GameObject(childName);
                obj.transform.SetParent(transform);
                source = obj.AddComponent<AudioSource>();
            }
            else
            {
                source = child.GetComponent<AudioSource>();

                if (source == null)
                    source = child.gameObject.AddComponent<AudioSource>();
            }
        }

        source.playOnAwake = false;
        source.loop = loop;
        source.spatialBlend = 0f;

        return source;
    }

    private void ApplyVolumes()
    {
        if (musicSource != null)
            musicSource.volume = musicVolume;

        if (cardAddedToHandSource != null)
            cardAddedToHandSource.volume = cardAddedToHandVolume;

        if (cardAddedToGeneratorSource != null)
            cardAddedToGeneratorSource.volume = cardAddedToGeneratorVolume;

        if (projectileShotSource != null)
            projectileShotSource.volume = projectileShotVolume;

        if (enemyDiedSource != null)
            enemyDiedSource.volume = enemyDiedVolume;

        if (lightningCastSource != null)
            lightningCastSource.volume = lightningCastVolume;
    }

    public void PlayBackgroundMusic()
    {
        if (musicSource == null || backgroundMusic == null)
            return;

        if (musicSource.clip == backgroundMusic && musicSource.isPlaying)
            return;

        musicSource.clip = backgroundMusic;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    public void StopBackgroundMusic()
    {
        if (musicSource == null)
            return;

        musicSource.Stop();
    }

    public void PlaySfx(GameSound sound)
    {
        AudioSource source = GetSource(sound);
        AudioClip clip = GetClip(sound);

        if (source == null || clip == null)
            return;

        source.clip = clip;
        source.Play();
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        if (musicSource != null)
            musicSource.volume = musicVolume;
    }

    public void SetCardAddedToHandVolume(float value)
    {
        cardAddedToHandVolume = Mathf.Clamp01(value);
        if (cardAddedToHandSource != null)
            cardAddedToHandSource.volume = cardAddedToHandVolume;
    }

    public void SetCardAddedToGeneratorVolume(float value)
    {
        cardAddedToGeneratorVolume = Mathf.Clamp01(value);
        if (cardAddedToGeneratorSource != null)
            cardAddedToGeneratorSource.volume = cardAddedToGeneratorVolume;
    }

    public void SetProjectileShotVolume(float value)
    {
        projectileShotVolume = Mathf.Clamp01(value);
        if (projectileShotSource != null)
            projectileShotSource.volume = projectileShotVolume;
    }

    public void SetEnemyDiedVolume(float value)
    {
        enemyDiedVolume = Mathf.Clamp01(value);
        if (enemyDiedSource != null)
            enemyDiedSource.volume = enemyDiedVolume;
    }

    public void SetLightningCastVolume(float value)
    {
        lightningCastVolume = Mathf.Clamp01(value);
        if (lightningCastSource != null)
            lightningCastSource.volume = lightningCastVolume;
    }

    private AudioSource GetSource(GameSound sound)
    {
        switch (sound)
        {
            case GameSound.CardAddedToHand:
                return cardAddedToHandSource;

            case GameSound.CardAddedToGenerator:
                return cardAddedToGeneratorSource;

            case GameSound.ProjectileShot:
                return projectileShotSource;

            case GameSound.EnemyDied:
                return enemyDiedSource;

            case GameSound.LightningCast:
                return lightningCastSource;

            default:
                return null;
        }
    }

    private AudioClip GetClip(GameSound sound)
    {
        switch (sound)
        {
            case GameSound.CardAddedToHand:
                return cardAddedToHandClip;

            case GameSound.CardAddedToGenerator:
                return cardAddedToGeneratorClip;

            case GameSound.ProjectileShot:
                return projectileShotClip;

            case GameSound.EnemyDied:
                return enemyDiedClip;

            case GameSound.LightningCast:
                return lightningCastClip;

            default:
                return null;
        }
    }
}