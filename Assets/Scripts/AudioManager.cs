// Authored by: Finn Davis
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    #region Singleton
    public static AudioManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    [System.Serializable]
    public class Sounds
    {
        public string name;
        [HideInInspector] public AudioSource audioSource;
        [Range(0f, 1f)] public float volume;
        public float minPitch = 1;
        public float maxPitch = 1;
        public List<AudioClip> audioClips;
    }

    [HideInInspector] public float evffectsVolume;

    public AudioSource menuSource;
    public AudioSource gameSource;    
    [SerializeField] private List<Sounds> soundEffects = new List<Sounds>();

    private void OnEnable()
    {
        foreach (Sounds sound in soundEffects)
        {
            GameObject gObj = new GameObject("AS: " + sound.name);
            gObj.transform.parent = transform;
            sound.audioSource = gObj.AddComponent<AudioSource>();
            sound.audioSource.playOnAwake = false;
        }
    }
    public void PlaySound(string name)
    {
        Sounds sound;
        if (TryGetSound(name, out sound))
        {
            if (sound.audioClips.Count == 0) return;
            sound.audioSource.clip = sound.audioClips[Random.Range(0, sound.audioClips.Count - 1)];
            sound.audioSource.volume = sound.volume;
            sound.audioSource.pitch = Random.Range(sound.minPitch, sound.maxPitch);
            sound.audioSource.Play();
        }
        else
        {
            Debug.Log("Sound Name {" + name + "} is either wrong or not in the SoundEffects List");
        }
        
    }
    public void PlayMenuMusic(bool play) => menuSource.enabled = play;
    public void PlayGameMusic(bool play) => gameSource.enabled = play;    
    public bool TryGetSound(string name, out Sounds sound)
    {
        foreach (Sounds soundEffect in soundEffects)
        {
            if (soundEffect.name == name) { sound = soundEffect; return true; }
        }
        sound = null;
        return false;
    }
}
