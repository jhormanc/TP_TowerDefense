using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;

public class SoundManager : Singleton<SoundManager>
{
    public AudioMixerGroup MasterMixerGroup;
    public GameObject AudioSource;

    private Dictionary<Audio_Type, AudioData> _audioDatas;
    private Dictionary<int, AudioSrc> _audioPlaying; 
    private PullManager _audioPool;

    void Awake()
    {
        _audioPool = ScriptableObject.CreateInstance<PullManager>();
        _audioPool.Init(AudioSource, 32, 256);
        _audioDatas = new Dictionary<Audio_Type, AudioData>();
        _audioPlaying = new Dictionary<int, AudioSrc>();

        if (MasterMixerGroup == null)
        {
            AudioMixer masterMixer = (Resources.Load("Audio/MasterMixer") as AudioMixer);

            if (masterMixer != null)
                MasterMixerGroup = masterMixer.FindMatchingGroups("Master")[0];
        }
    }


    public void RegisterAudioData(Audio_Type type, AudioData data)
    {
        _audioDatas.Add(type, data);
    }


    public AudioMixerGroup GetMixerType(Audio_Type type)
    {
        AudioData datas;
        if (_audioDatas.TryGetValue(type, out datas))
        {
            return datas.DefaultMixerGroup;
        }

        return null;
    }


    public void SetGlobalVolume(float volume)
    {
        if (MasterMixerGroup != null)
        {
            MasterMixerGroup.audioMixer.SetFloat("master_volume", volume);
        }
    }


    public void SetGlobalPitch(float pitch)
    {
        if (MasterMixerGroup != null)
        {
            MasterMixerGroup.audioMixer.SetFloat("master_pitch", pitch);
        }
    }


    public void Move(int key, float time, Vector3 position)
    {
        AudioSrc source;
        if (_audioPlaying.TryGetValue(key, out source))
        {
            source.Move(time, position);
        }
    }


    public void MoveAll(int key, float time, Vector3 position)
    {
        foreach (KeyValuePair<int, AudioSrc> entry in _audioPlaying)
        {
            entry.Value.Move(time, position);
        }
    }


    public void Fade(int key, float time, float volume)
    {
        volume = Mathf.Clamp(volume, .0f, 1.0f);

        AudioSrc source;
        if (_audioPlaying.TryGetValue(key, out source))
        {
            source.Fade(time, volume);
        }
    }


    public void FadeAll(int key, float time, float volume)
    {
        volume = Mathf.Clamp(volume, .0f, 1.0f);

        foreach (KeyValuePair<int, AudioSrc> entry in _audioPlaying)
        {
            entry.Value.Fade(time, volume);
        }
    }


    public void Pitch(int key, float time, float pitch)
    {
        pitch = Mathf.Clamp(pitch, -3.0f, 3.0f);

        AudioSrc source;
        if (_audioPlaying.TryGetValue(key, out source))
        {
            source.Pitch(time, pitch);
        }
    }


    public void PitchAll(int key, float time, float pitch)
    {
        pitch = Mathf.Clamp(pitch, -3.0f, 3.0f);

        foreach (KeyValuePair<int, AudioSrc> entry in _audioPlaying)
        {
            entry.Value.Pitch(time, pitch);
        }
    }


    /*
     * Hashtable parameters :
     *  loop : boolean
     *  delayed : boolean
     *  delayedtime : float
     *  priority : int
     *  volume : float
     *  pitch : float
     *  panStereo : float
     *  spatialBlend : float
     *  reverbZoneMix : float
     *  dopplerLevel : float
     *  spread : float
     *  rolloffMode : AudioRolloffMode
     *  minDistance : float
     *  maxDistance : float
     *  position : Vector3
     */
    public int PlayAudio(Audio_Type type, Hashtable param = null)
    {
        AudioSource source;
        int key;

        if (GetSource(type, out source, out key))
        {
            if (param == null)
                param = new Hashtable();

            bool delayed = param.ContainsKey("delayed") ? (bool)param["delayed"] : false;

            source.loop = param.ContainsKey("loop") ? (bool)param["loop"] : false;
            source.priority = param.ContainsKey("priority") ? (int)param["priority"] : 128;
            source.volume = param.ContainsKey("volume") ? (float)param["volume"] : 1.0f;
            source.pitch = param.ContainsKey("pitch") ? (float)param["pitch"] : 1.0f;
            source.panStereo = param.ContainsKey("panStereo") ? (float)param["panStereo"] : .0f;
            source.spatialBlend = param.ContainsKey("spatialBlend") ? (float)param["spatialBlend"] : .0f;
            source.reverbZoneMix = param.ContainsKey("reverbZoneMix") ? (float)param["reverbZoneMix"] : .45f;
            source.dopplerLevel = param.ContainsKey("dopplerLevel") ? (float)param["dopplerLevel"] : 1.0f;
            source.spread = param.ContainsKey("spread") ? (float)param["spread"] : .0f;
            source.rolloffMode = param.ContainsKey("rolloffMode") ? (AudioRolloffMode)param["rolloffMode"] : AudioRolloffMode.Logarithmic;
            source.minDistance = param.ContainsKey("minDistance") ? (float)param["minDistance"] : 1.0f;
            source.maxDistance = param.ContainsKey("maxDistance") ? (float)param["maxDistance"] : 500.0f;

            source.transform.position = param.ContainsKey("position") ? (Vector3)param["position"] : Vector3.zero;

            if (delayed)
            {
                float time = param.ContainsKey("delayedtime") ? (float)param["delayedtime"] : .0f;

                source.PlayDelayed(time);
            }
            else
            {
                source.Play();
            }
        }
        else
        {
            Destroy(source);
        }

        return key;
    }


    public void Pause(int key, bool mute = true)
    {
        AudioSrc source;
        if (_audioPlaying.TryGetValue(key, out source))
        {
            source.Source.Pause();
        }
    }


    public void PauseAll(bool mute = true)
    {
        foreach (KeyValuePair<int, AudioSrc> entry in _audioPlaying)
        {
            entry.Value.Source.Pause();
        }
    }


    public void stop(int key, bool mute = true)
    {
        AudioSrc source;
        if (_audioPlaying.TryGetValue(key, out source))
        {
            source.Source.Stop();
            _audioPool.RemoveObj(source.gameObject);
        }
    }


    public void stopAll(bool mute = true)
    {
        foreach (KeyValuePair<int, AudioSrc> entry in _audioPlaying)
        {
            entry.Value.Source.Stop();
            _audioPool.RemoveObj(entry.Value.gameObject);
        }
    }


    public void mute(int key, bool mute = true)
    {
        AudioSrc source;
        if (_audioPlaying.TryGetValue(key, out source))
        {
            source.Source.mute = mute;
        }
    }


    public void muteAll(bool mute = true)
    {
        foreach (KeyValuePair<int, AudioSrc> entry in _audioPlaying)
        {
            entry.Value.Source.mute = mute;
        }
    }

    private bool GetSource(Audio_Type type, out AudioSource source, out int key)
    {
        AudioData data;

        if (_audioDatas.TryGetValue(type, out data))
        {
            GameObject obj = _audioPool.GetNextObj(true);
            obj.SetActive(true);
            AudioSrc psource = obj.GetComponent<AudioSrc>();

            key = _audioPlaying.Count - 1;

            _audioPlaying.Add(key, psource);

            source = psource.Source;

            source.clip = data.GetClip();
            source.outputAudioMixerGroup = data.DefaultMixerGroup;

            return true;
        }
        else
        {
            Destroy(data);
        }

        source = new AudioSource();
        key = -1;

        return false;
    }
}
