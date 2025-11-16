using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;

[System.Serializable]
public class BgmTrack
{
    public EBgmName eBgmName;
    public AudioClip clip;
}
[System.Serializable]
public class SoundEffect
{
    public ESfxName eSfxName;
    public AudioClip clip;
}
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("AudioMixer")]
    [SerializeField] private AudioMixer masterMixer;

    [Header("이벤트 발행")]
    [SerializeField] private VoidEventChannelSO onBgmEnd;
    private Coroutine bgmEndCheckCo;

    [Header("BGM")]
    [SerializeField] private AudioSource bgmSpeaker;
    [SerializeField] private List<BgmTrack> bgmList;
    private Dictionary<EBgmName, AudioClip> bgmDic;

    [Range(0.0f, 1.0f)][SerializeField] private float masterBgmVolume = 0.5f;
    [SerializeField] private float fadeDuration = 0.8f;
    private Coroutine bgmFadeCo;

    [Header("SFX")]
    [SerializeField] private AudioSource sfxSpeaker;
    [SerializeField] private List<SoundEffect> sfxList;
    private Dictionary<ESfxName, AudioClip> sfxDic;

    void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        bgmDic = new Dictionary<EBgmName, AudioClip>();
        foreach (BgmTrack bgm in bgmList)
        {
            if (!bgmDic.ContainsKey(bgm.eBgmName))
            {
                bgmDic.Add(bgm.eBgmName, bgm.clip);
            }
        }

        sfxDic = new Dictionary<ESfxName, AudioClip>();
        foreach (SoundEffect sfx in sfxList)
        {
            if (!sfxDic.ContainsKey(sfx.eSfxName))
            {
                sfxDic.Add(sfx.eSfxName, sfx.clip);
            }
        }
    }
    private void Start()
    {
        float savedMasterVolume = PlayerPrefs.GetFloat("MasterVolume", 0.7f);
        SetMasterVolume(savedMasterVolume);
        float savedBgmVolume = PlayerPrefs.GetFloat("BgmVolume", 0.3f);
        SetBGMVolume(savedBgmVolume);
        float savedSfxVolume = PlayerPrefs.GetFloat("SfxVolume", 0.8f);
        SetSFXVolume(savedSfxVolume);
    }
    public void SetMasterVolume(float sliderValue)
    {
        float volume = Mathf.Log10(Mathf.Max(sliderValue, 0.0001f)) * 20;
        masterMixer.SetFloat("MasterVolume", volume);
        PlayerPrefs.SetFloat("MasterVolume", sliderValue);
    }
    public void SetBGMVolume(float sliderValue)
    {
        float volume = Mathf.Log10(Mathf.Max(sliderValue, 0.0001f)) * 20;
        masterMixer.SetFloat("BgmVolume", volume);
        PlayerPrefs.SetFloat("BgmVolume", sliderValue);
    }

    public void SetSFXVolume(float sliderValue)
    {
        float volume = Mathf.Log10(Mathf.Max(sliderValue, 0.0001f)) * 20;
        masterMixer.SetFloat("SfxVolume", volume);
        PlayerPrefs.SetFloat("SfxVolume", sliderValue);
    }
    public void PlayBGM(EBgmName eBgmName, bool loop)
    {
        PlayBGM(eBgmName, loop, fadeDuration);
    }
    public void PlayBGM(EBgmName eBgmName, bool loop, float fadeDuration)
    {
        if (bgmSpeaker == null || !bgmDic.ContainsKey(eBgmName)) return;
        AudioClip clipToPlay = bgmDic[eBgmName];

        if (bgmSpeaker.clip == clipToPlay && bgmSpeaker.isPlaying) return;
        if (bgmFadeCo != null) StopCoroutine(bgmFadeCo);

        //코루틴으로 실행
        bgmFadeCo = StartCoroutine(BgmFadeCo(clipToPlay, loop, fadeDuration));
    }
    IEnumerator BgmFadeCo(AudioClip newClip, bool loop, float fadeDuration)
    {
        if (bgmSpeaker.isPlaying)
        {
            float startVolume = bgmSpeaker.volume;
            float fadeOutTime = fadeDuration * 0.5f;
            float timerOut = 0f;

            while (timerOut < fadeOutTime)
            {
                timerOut += Time.deltaTime;
                bgmSpeaker.volume = Mathf.Lerp(startVolume, 0f, timerOut / fadeOutTime);
                yield return null;
            }
            bgmSpeaker.volume = 0f;
            bgmSpeaker.Stop();
        }

        bgmSpeaker.clip = newClip;
        bgmSpeaker.loop = loop;
        bgmSpeaker.Play();

        //End check 코루틴 재시작
        if (bgmEndCheckCo != null) StopCoroutine(bgmEndCheckCo);
        if (!loop)
        {
            bgmEndCheckCo = StartCoroutine(BgmEndCheckCo());
        }

        float fadeInTime = fadeDuration * 0.5f;
        float timerIn = 0f;
        while (timerIn < fadeInTime)
        {
            timerIn += Time.deltaTime;
            bgmSpeaker.volume = Mathf.Lerp(0f, masterBgmVolume, timerIn / fadeInTime);
            yield return null;
        }
        bgmSpeaker.volume = masterBgmVolume;

        bgmFadeCo = null;
    }
    IEnumerator BgmEndCheckCo()
    {
        yield return null;
        yield return new WaitUntil(() => bgmSpeaker.isPlaying);
        yield return new WaitUntil(() => !bgmSpeaker.isPlaying);
        onBgmEnd.Raise();
    }
    public void StopBGM()
    {
        if (bgmSpeaker == null) return;
        bgmSpeaker.Stop();
    }
    public void PlaySFX(ESfxName eSfxName)
    {
        if (sfxSpeaker == null || !sfxDic.ContainsKey(eSfxName)) return;
        sfxSpeaker.PlayOneShot(sfxDic[eSfxName]);
    }

    public void SetBgmVolume(float volume)
    {
        bgmSpeaker.volume = volume;
    }

    public void SetSfxVolume(float volume)
    {
        sfxSpeaker.volume = volume;
    }
}