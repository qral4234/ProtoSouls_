using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldSoundFXManager : MonoBehaviour
{
    public static WorldSoundFXManager instance;

    [Header("Music Settings")]
    public AudioSource musicAudioSource;
    public AudioClip fightMusic;
    public float fadeDuration = 2f;

    [Header("Debug")]
    public List<EnemyStats> activeCombatants = new List<EnemyStats>();

    private Coroutine currentFadeCoroutine;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Opsiyonel: Sahneler arası müzik devam etsin
        }
        else
        {
            Destroy(gameObject);
        }

        if (musicAudioSource == null)
        {
            musicAudioSource = GetComponent<AudioSource>();
            if (musicAudioSource == null)
            {
                musicAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        musicAudioSource.loop = true;
        musicAudioSource.volume = 0; // Başlangıçta sessiz
    }

    public void AddCombatant(EnemyStats enemy)
    {
        if (!activeCombatants.Contains(enemy))
        {
            activeCombatants.Add(enemy);
            CheckMusicState();
        }
    }

    public void RemoveCombatant(EnemyStats enemy)
    {
        if (activeCombatants.Contains(enemy))
        {
            activeCombatants.Remove(enemy);
            CheckMusicState();
        }
    }

    private void CheckMusicState()
    {
        if (activeCombatants.Count > 0)
        {
            // Eğer müzik çalmıyorsa başlat
            if (musicAudioSource.clip != fightMusic || !musicAudioSource.isPlaying)
            {
                PlayFightMusic();
            }
        }
        else
        {
            // Kimse kalmadıysa müziği durdur
            StopFightMusic();
        }
    }

    private void PlayFightMusic()
    {
        if (fightMusic == null) return;

        if (currentFadeCoroutine != null) StopCoroutine(currentFadeCoroutine);
        currentFadeCoroutine = StartCoroutine(FadeMusic(fightMusic, 0.5f)); // Hedef ses seviyesi 0.5f
    }

    private void StopFightMusic()
    {
        if (currentFadeCoroutine != null) StopCoroutine(currentFadeCoroutine);
        currentFadeCoroutine = StartCoroutine(FadeMusic(null, 0f));
    }

    private IEnumerator FadeMusic(AudioClip newClip, float targetVolume)
    {
        float startVolume = musicAudioSource.volume;
        float timer = 0;

        // Eğer yeni bir klip başlıyorsa, önce eskisini kıs
        if (newClip != null && musicAudioSource.clip != newClip)
        {
            musicAudioSource.clip = newClip;
            musicAudioSource.Play();
        }

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            musicAudioSource.volume = Mathf.Lerp(startVolume, targetVolume, timer / fadeDuration);
            yield return null;
        }

        musicAudioSource.volume = targetVolume;

        // Eğer hedef ses 0 ise durdur
        if (targetVolume <= 0.01f)
        {
            musicAudioSource.Stop();
            musicAudioSource.clip = null;
        }
    }
}
