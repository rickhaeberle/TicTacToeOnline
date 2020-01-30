using UnityEngine;

public class SFXPlayer : MonoBehaviour
{

    public static SFXPlayer Instance;

    private AudioSource _source;

    private void Awake()
    {
        if (Instance != null)
            Destroy(Instance.gameObject);

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _source = GetComponent<AudioSource>();
    }

    public void Play(AudioClip clip, float volume = 1f)
    {
        _source.PlayOneShot(clip, volume);

    }

}
