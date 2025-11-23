using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [System.Serializable]
    public class AudioClipData
    {
        public string name;
        public AudioClip clip;
    }

    [Header("Audio Clips")]
    public List<AudioClipData> audioClips; 

    [Header("Settings")]
    public int sfxSourceCount = 3;
    public float musicFadeTime = 1f;

    private Dictionary<string, AudioClip> clipDict;
    private AudioSource[] sfxSources;
    private AudioSource musicSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        clipDict = new Dictionary<string, AudioClip>();
        foreach (var data in audioClips)
        {
            clipDict[data.name] = data.clip;
        }

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;

        sfxSources = new AudioSource[sfxSourceCount];
        for (int i = 0; i < sfxSourceCount; i++)
        {
            GameObject sfxObj = new GameObject("SFX_Source_" + i);
            sfxObj.transform.SetParent(transform);
            sfxSources[i] = sfxObj.AddComponent<AudioSource>();
            sfxSources[i].playOnAwake = false;
        }

        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    private void OnSceneChanged(Scene oldScene, Scene newScene)
    {
        StopMusic(musicFadeTime);
    }

    public void PlayMusic(string clipName)
    {
        if (!clipDict.TryGetValue(clipName, out AudioClip clip))
        {
            return;
        }

        StartCoroutine(FadeToNewMusic(clip, musicFadeTime));
    }

    private IEnumerator FadeToNewMusic(AudioClip newClip, float fadeTime)
    {
        if (musicSource.isPlaying)
        {
            float startVol = musicSource.volume;
            for (float t = 0; t < fadeTime; t += Time.deltaTime)
            {
                musicSource.volume = Mathf.Lerp(startVol, 0f, t / fadeTime);
                yield return null;
            }
            musicSource.Stop();
        }


        musicSource.clip = newClip;
        musicSource.Play();
        musicSource.loop = true;

        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(0f, 1f, t / fadeTime);
            yield return null;
        }
        musicSource.volume = 1f;
    }

    public void StopMusic(float fadeDuration = 0.75f)
    {
        if (musicSource.isPlaying)
            StartCoroutine(FadeOutAndStop(fadeDuration));
    }

    private IEnumerator FadeOutAndStop(float duration)
    {
        float startVol = musicSource.volume;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVol, 0f, t / duration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = 1f; // reset for next playback
    }

    public void PlaySFX(string clipName, float volume = 1f)
    {
        if (!clipDict.TryGetValue(clipName, out AudioClip clip))
        {
            return;
        }

        foreach (var src in sfxSources)
        {
            if (!src.isPlaying)
            {
                src.clip = clip;
                src.volume = volume;
                src.loop = false;
                src.Play();
                return;
            }
        }

        sfxSources[0].Stop();
        sfxSources[0].clip = clip;
        sfxSources[0].volume = volume;
        sfxSources[0].Play();
    }
}
