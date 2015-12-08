using UnityEngine;
using UnityEngine.Audio;

public class AudioData : MonoBehaviour
{
    [SerializeField]
    AudioClip[] AudioClips;

    public AudioMixerGroup DefaultMixerGroup;
    public Audio_Type Type;
    public bool PlayRandom;

    private int _lastPlayed;


    void Start()
    {
        _lastPlayed = 0;

        SoundManager.Instance.RegisterAudioData(Type, this);
    }


    public AudioClip GetClip()
    {
        AudioClip clip;

        if (PlayRandom)
        {
            clip = AudioClips[(int)Mathf.Floor(Random.value * AudioClips.Length)];
        }
        else
        {
            int idx = _lastPlayed = _lastPlayed == AudioClips.Length - 1 ? 0 : _lastPlayed + 1;
            clip = AudioClips[idx];
        }

        return clip;
    }
}
