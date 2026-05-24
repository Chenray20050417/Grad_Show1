using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void BootstrapForDirectScenePlay()
    {
        if (FindObjectOfType<AudioManager>() != null)
            return;

        GameObject audioManagerObject = new GameObject("AudioManager");
        audioManagerObject.AddComponent<AudioManager>();
    }

    [Header("音量")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float bgmVolume = 0.7f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    public bool muted;

    [Header("BGM")]
    public AudioClip mainMenuBgm;
    public AudioClip level1Bgm;
    public AudioClip level2Bgm;
    public AudioClip level3Bgm;
    public AudioClip comicBgm;
    public AudioClip cinematicBgm;

    [Header("SFX")]
    public AudioClip buttonHoverSfx;
    public AudioClip buttonClickSfx;
    public AudioClip gestureClickSfx;
    public AudioClip pauseSfx;
    public AudioClip resumeSfx;
    public AudioClip stressBallBreakSfx;
    public AudioClip stressBallMissSfx;
    public AudioClip playerHurtSfx;
    public AudioClip supplementPickupSfx;
    public AudioClip supplementUseSfx;
    public AudioClip passSfx;
    public AudioClip failSfx;
    public AudioClip countdownWarningSfx;
    public AudioClip transitionSfx;

    [Header("Player Animation SFX")]
    public AudioClip level1PlayerAnimationSfx;
    public AudioClip level2PlayerAnimationSfx;
    public AudioClip level3PlayerAnimationSfx;

    [Header("Slow Motion SFX")]
    public AudioClip slowMotionStartSfx;
    public AudioClip slowMotionLoopSfx;
    public AudioClip slowMotionEndSfx;
    [Range(0f, 1f)] public float slowMotionLoopVolume = 0.25f;
    [Range(0f, 1f)] public float slowMotionBgmVolumeMultiplier = 0.45f;

    private AudioSource bgmSource;
    private AudioSource sfxSource;
    private AudioSource slowMotionLoopSource;
    private AudioClip currentBgm;
    private bool slowMotionAudioActive;

    private const string BgmVolumeKey = "SweatFactory_BgmVolume";
    private const string SfxVolumeKey = "SweatFactory_SfxVolume";
    private const string MutedKey = "SweatFactory_Muted";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Instance.CopyMissingClipReferencesFrom(this);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
#if UNITY_EDITOR
        AutoAssignEditorClips();
#endif
        LoadAudioSettings();
        EnsureSources();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        PlayBgmForScene(SceneManager.GetActiveScene().name);
        RegisterSceneButtons();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StopSlowMotionAudio(false);
        PlayBgmForScene(scene.name);
        RegisterSceneButtons();
    }

    public void PlayButtonClick()
    {
        PlaySfx(buttonClickSfx);
    }

    public void PlayButtonHover()
    {
        PlaySfx(buttonHoverSfx);
    }

    public void PlayGestureClick()
    {
        PlaySfx(gestureClickSfx);
    }

    public void PlayPause()
    {
        PlaySfx(pauseSfx);
    }

    public void PlayResume()
    {
        PlaySfx(resumeSfx);
    }

    public void PlayStressBallBreak()
    {
        PlaySfx(stressBallBreakSfx);
    }

    public void PlayStressBallMiss()
    {
        PlaySfx(stressBallMissSfx);
    }

    public void PlayPlayerHurt()
    {
        PlaySfx(playerHurtSfx);
    }

    public void PlaySupplementPickup()
    {
        PlaySfx(supplementPickupSfx);
    }

    public void PlaySupplementUse()
    {
        PlaySfx(supplementUseSfx);
    }

    public void PlayPass()
    {
        PlaySfx(passSfx);
    }

    public void PlayFail()
    {
        PlaySfx(failSfx);
    }

    public void PlayCountdownWarning()
    {
        PlaySfx(countdownWarningSfx);
    }

    public void PlayTransition()
    {
        PlaySfx(transitionSfx);
    }

    public void PlayLevel1PlayerAnimation()
    {
        PlaySfx(level1PlayerAnimationSfx);
    }

    public void PlayLevel2PlayerAnimation()
    {
        PlaySfx(level2PlayerAnimationSfx);
    }

    public void PlayLevel3PlayerAnimation()
    {
        PlaySfx(level3PlayerAnimationSfx);
    }

    public void StartSlowMotionAudio()
    {
        EnsureSources();

        if (slowMotionAudioActive)
            return;

        slowMotionAudioActive = true;
        PlaySfx(slowMotionStartSfx);

        if (slowMotionLoopSfx != null)
        {
            slowMotionLoopSource.clip = slowMotionLoopSfx;
            slowMotionLoopSource.loop = true;
            slowMotionLoopSource.volume = masterVolume * sfxVolume * slowMotionLoopVolume;
            slowMotionLoopSource.Play();
        }

        UpdateVolumes();
    }

    public void StopSlowMotionAudio(bool playEndSfx = true)
    {
        EnsureSources();

        if (!slowMotionAudioActive)
            return;

        slowMotionAudioActive = false;

        if (slowMotionLoopSource.isPlaying)
            slowMotionLoopSource.Stop();

        slowMotionLoopSource.clip = null;

        if (playEndSfx)
            PlaySfx(slowMotionEndSfx);

        UpdateVolumes();
    }

    public void PlaySfx(AudioClip clip)
    {
        if (clip == null || muted)
            return;

        EnsureSources();
        sfxSource.PlayOneShot(clip, GetEffectiveSfxVolume());
    }

    public void PlayBgm(AudioClip clip)
    {
        EnsureSources();

        if (clip == currentBgm)
        {
            if (clip != null && !bgmSource.isPlaying)
            {
                if (bgmSource.clip == clip)
                    bgmSource.UnPause();
                else
                {
                    bgmSource.clip = clip;
                    bgmSource.loop = true;
                    bgmSource.volume = GetEffectiveBgmVolume();
                    bgmSource.Play();
                }
            }

            return;
        }

        currentBgm = clip;

        if (clip == null)
        {
            bgmSource.Stop();
            bgmSource.clip = null;
            return;
        }

        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.volume = GetEffectiveBgmVolume();
        bgmSource.Play();
    }

    public void UpdateVolumes()
    {
        EnsureSources();
        bgmSource.volume = GetEffectiveBgmVolume();
        slowMotionLoopSource.volume = GetEffectiveSfxVolume() * slowMotionLoopVolume;
    }

    public void SetBgmVolume(float value)
    {
        bgmVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(BgmVolumeKey, bgmVolume);
        PlayerPrefs.Save();
        UpdateVolumes();
    }

    public void SetSfxVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(SfxVolumeKey, sfxVolume);
        PlayerPrefs.Save();
        UpdateVolumes();
    }

    public void SetMuted(bool value)
    {
        muted = value;
        PlayerPrefs.SetInt(MutedKey, muted ? 1 : 0);
        PlayerPrefs.Save();
        UpdateVolumes();
    }

    public void ToggleMuted()
    {
        SetMuted(!muted);
    }

    public void PauseBgm()
    {
        EnsureSources();

        if (bgmSource.isPlaying)
            bgmSource.Pause();
    }

    public void ResumeBgm()
    {
        EnsureSources();

        if (bgmSource.clip != null && !bgmSource.isPlaying)
            bgmSource.UnPause();
    }

    public AudioSource GetBgmSource()
    {
        EnsureSources();
        return bgmSource;
    }

    private float GetEffectiveBgmVolume()
    {
        if (muted)
            return 0f;

        float multiplier = slowMotionAudioActive ? slowMotionBgmVolumeMultiplier : 1f;
        return masterVolume * bgmVolume * multiplier;
    }

    private float GetEffectiveSfxVolume()
    {
        return muted ? 0f : masterVolume * sfxVolume;
    }

    private void LoadAudioSettings()
    {
        bgmVolume = PlayerPrefs.GetFloat(BgmVolumeKey, bgmVolume);
        sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, sfxVolume);
        muted = PlayerPrefs.GetInt(MutedKey, muted ? 1 : 0) == 1;
    }

    private void CopyMissingClipReferencesFrom(AudioManager source)
    {
        if (source == null)
            return;

        CopyIfMissing(ref mainMenuBgm, source.mainMenuBgm);
        CopyIfMissing(ref level1Bgm, source.level1Bgm);
        CopyIfMissing(ref level2Bgm, source.level2Bgm);
        CopyIfMissing(ref level3Bgm, source.level3Bgm);
        CopyIfMissing(ref comicBgm, source.comicBgm);
        CopyIfMissing(ref cinematicBgm, source.cinematicBgm);

        CopyIfMissing(ref buttonHoverSfx, source.buttonHoverSfx);
        CopyIfMissing(ref buttonClickSfx, source.buttonClickSfx);
        CopyIfMissing(ref gestureClickSfx, source.gestureClickSfx);
        CopyIfMissing(ref pauseSfx, source.pauseSfx);
        CopyIfMissing(ref resumeSfx, source.resumeSfx);
        CopyIfMissing(ref stressBallBreakSfx, source.stressBallBreakSfx);
        CopyIfMissing(ref stressBallMissSfx, source.stressBallMissSfx);
        CopyIfMissing(ref playerHurtSfx, source.playerHurtSfx);
        CopyIfMissing(ref supplementPickupSfx, source.supplementPickupSfx);
        CopyIfMissing(ref supplementUseSfx, source.supplementUseSfx);
        CopyIfMissing(ref passSfx, source.passSfx);
        CopyIfMissing(ref failSfx, source.failSfx);
        CopyIfMissing(ref countdownWarningSfx, source.countdownWarningSfx);
        CopyIfMissing(ref transitionSfx, source.transitionSfx);

        CopyIfMissing(ref level1PlayerAnimationSfx, source.level1PlayerAnimationSfx);
        CopyIfMissing(ref level2PlayerAnimationSfx, source.level2PlayerAnimationSfx);
        CopyIfMissing(ref level3PlayerAnimationSfx, source.level3PlayerAnimationSfx);

        CopyIfMissing(ref slowMotionStartSfx, source.slowMotionStartSfx);
        CopyIfMissing(ref slowMotionLoopSfx, source.slowMotionLoopSfx);
        CopyIfMissing(ref slowMotionEndSfx, source.slowMotionEndSfx);

        UpdateVolumes();
    }

    private void CopyIfMissing(ref AudioClip target, AudioClip source)
    {
        if (target == null && source != null)
            target = source;
    }

    private void PlayBgmForScene(string sceneName)
    {
        if (sceneName == "MainMenu")
        {
            PlayBgm(mainMenuBgm);
            return;
        }

        if (sceneName == "CinematicTrailer")
        {
            PlayBgm(cinematicBgm);
            return;
        }

        if (sceneName.StartsWith("ComicScene"))
        {
            PlayBgm(comicBgm);
            return;
        }

        if (sceneName == "level1")
        {
            PlayBgm(level1Bgm);
            return;
        }

        if (sceneName == "level2")
        {
            PlayBgm(level2Bgm);
            return;
        }

        if (sceneName == "Level3")
        {
            PlayBgm(level3Bgm);
            return;
        }
    }

    private void RegisterSceneButtons()
    {
        Button[] buttons = FindObjectsOfType<Button>(true);

        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].onClick.RemoveListener(PlayButtonClick);
            buttons[i].onClick.AddListener(PlayButtonClick);
        }
    }

    private void EnsureSources()
    {
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.playOnAwake = false;
            bgmSource.loop = true;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
        }

        if (slowMotionLoopSource == null)
        {
            slowMotionLoopSource = gameObject.AddComponent<AudioSource>();
            slowMotionLoopSource.playOnAwake = false;
            slowMotionLoopSource.loop = true;
        }
    }

#if UNITY_EDITOR
    private void AutoAssignEditorClips()
    {
        mainMenuBgm = LoadEditorClipIfMissing(mainMenuBgm, "Assets/Audio/MenuBGM..mp3");
        level1Bgm = LoadEditorClipIfMissing(level1Bgm, "Assets/Audio/Level1BGM.mp3");
        level2Bgm = LoadEditorClipIfMissing(level2Bgm, "Assets/Audio/Level2BGM.mp3");
        level3Bgm = LoadEditorClipIfMissing(level3Bgm, "Assets/Audio/Level3BGM.mp3");

        buttonHoverSfx = LoadEditorClipIfMissing(buttonHoverSfx, "Assets/Audio/hover.mp3");
        buttonClickSfx = LoadEditorClipIfMissing(buttonClickSfx, "Assets/Audio/click.mp3");
        gestureClickSfx = LoadEditorClipIfMissing(gestureClickSfx, "Assets/Audio/Gesture Click Sfx.mp3");
        stressBallBreakSfx = LoadEditorClipIfMissing(stressBallBreakSfx, "Assets/Audio/Stress Ball Break Sfx.mp3");
        stressBallMissSfx = LoadEditorClipIfMissing(stressBallMissSfx, "Assets/Audio/Stress Ball Miss Sfx.mp3");
        playerHurtSfx = LoadEditorClipIfMissing(playerHurtSfx, "Assets/Audio/playerhurt.mp3");
        supplementPickupSfx = LoadEditorClipIfMissing(supplementPickupSfx, "Assets/Audio/pickupsupply.mp3");
        supplementUseSfx = LoadEditorClipIfMissing(supplementUseSfx, "Assets/Audio/usesitem.mp3");
        passSfx = LoadEditorClipIfMissing(passSfx, "Assets/Audio/WIN.mp3");
        failSfx = LoadEditorClipIfMissing(failSfx, "Assets/Audio/lose.mp3");
        countdownWarningSfx = LoadEditorClipIfMissing(countdownWarningSfx, "Assets/Audio/countdownWarningSfx.mp3");
        transitionSfx = LoadEditorClipIfMissing(transitionSfx, "Assets/Audio/transitionSfx.mp3");

        level1PlayerAnimationSfx = LoadEditorClipIfMissing(level1PlayerAnimationSfx, "Assets/Audio/Level 1 Player Animation Sfx.mp3");
        level3PlayerAnimationSfx = LoadEditorClipIfMissing(level3PlayerAnimationSfx, "Assets/Audio/Level 3 Player Animation Sfx.mp3");

        slowMotionStartSfx = LoadEditorClipIfMissing(slowMotionStartSfx, "Assets/Audio/slow motion whoosh sfx.mp3");
        slowMotionLoopSfx = LoadEditorClipIfMissing(slowMotionLoopSfx, "Assets/Audio/Slow Motion Loop Sfx.mp3");
        slowMotionEndSfx = LoadEditorClipIfMissing(slowMotionEndSfx, "Assets/Audio/Slow Motion End Sfx.mp3");
    }

    private AudioClip LoadEditorClipIfMissing(AudioClip currentClip, string assetPath)
    {
        if (currentClip != null)
            return currentClip;

        return AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
    }
#endif
}
