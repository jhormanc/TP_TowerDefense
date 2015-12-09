using UnityEngine;
using System.Collections;

public class AudioSrc : MonoBehaviour
{
    public AudioSource Source;

    // Fading
    protected bool _fading;
    protected float _fade;
    protected float _fadeTime;
    protected float _currentFadeTime;

    // Pitching
    protected bool _pitching;
    protected float _pitch;
    protected float _pitchTime;
    protected float _currentPitchTime;

    // Moving
    protected bool _moving;
    protected Vector3 _move;
    protected float _moveTime;
    protected float _currentMoveTime;

    protected void Awake()
    {
        Source = GetComponent<AudioSource>();
    }

    public void Fade(float time, float volume)
    {
        _fade = volume;
        _fadeTime = time == .0f ? .0f : 1.0f / (time * time * 10.0f);
        _currentFadeTime = .0f;
        _fading = true;
    }

    public void Pitch(float time, float volume)
    {
        _pitch = volume;
        _pitchTime = time == .0f ? .0f : 1.0f / (time * time * 5.5f);
        _currentPitchTime = .0f;
        _pitching = true;
    }

    public void Move(float time, Vector3 position)
    {
        _move = position;
        _moveTime = time;
        _moving = true;
        _currentMoveTime = .0f;
    }

    void Update()
    {
        if (_fading)
        {
            _currentFadeTime += Time.deltaTime;

            if (Mathf.Abs(_fade - Source.volume) < 0.05f)
            {
                _fading = false;
                _currentFadeTime = _fadeTime;
                Source.volume = _fade;
            }

            Source.volume = Mathf.Lerp(Source.volume, _fade, _currentFadeTime * _fadeTime);
        }

        if (_pitching)
        {
            _currentPitchTime += Time.deltaTime;

            if (Mathf.Abs(_pitch - Source.pitch) < 0.01f)
            {
                _pitching = false;
                _currentPitchTime = _fadeTime;
                Source.pitch = _pitch;
            }

            Source.pitch = Mathf.Lerp(Source.pitch, _pitch, _currentPitchTime * _pitchTime);
        }

        if (_moving)
        {
            if (_currentMoveTime <= _moveTime)
            {
                _currentMoveTime += Time.deltaTime;
                transform.position = Vector3.Lerp(transform.position, _move, _currentMoveTime / _moveTime);
            }
            else
            {
                transform.position = _move;
                _moving = false;
            }
        }
    }
}
