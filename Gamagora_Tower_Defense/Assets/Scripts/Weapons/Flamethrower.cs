﻿using UnityEngine;
using System.Collections;
using System;

public class Flamethrower : Weapon
{

    public Flamethrower() : base()
    {

    }

    protected override void Awake()
    {
        base.Awake();
        transform.FindChild("Base").FindChild("Tourelle").FindChild("Head").FindChild("Cannon").FindChild("Particle").GetComponent<ParticleDamage>().Source = gameObject;
    }

    protected override void Move()
    {
        Transform target = GetTarget();
        Vector3 target_pos = Vector3.zero;

        if (target != null)
        {
            float fire_speed = _cannon.FindChild("Particle").GetComponent<ParticleDamage>().GetSpeed();
            target_pos = target.position;

            target_pos += (target.position - _head.position).magnitude
                             * target.forward
                             * target.GetComponent<Enemy>().Speed
                             / (fire_speed * 3f);

            Debug.DrawLine(target_pos, target_pos + target.forward);
        }

        Move(_tourelle, _head, _cannon, target_pos);
    }

    protected override void Fire()
    {
        StartCoroutine(Fire(_cannon, null, false));
    }

    protected override void EmitParticle(bool emit)
    {
        Transform p = _cannon.FindChild("Particle");

        p.GetComponent<ParticleSystem>().enableEmission = emit;
        p.FindChild("Smoke").GetComponent<ParticleSystem>().enableEmission = emit;
        p.FindChild("Sparkles").GetComponent<ParticleSystem>().enableEmission = emit;

        if (emit)
            p.GetComponent<ParticleSystem>().Play();
        else
            p.GetComponent<ParticleSystem>().Stop();

    }

    public override Audio_Type GetNewWeaponAudioType()
    {
        return Audio_Type.NewFlamethrower;
    }

    protected override void PlayFireSound(bool stop = false)
    {
        if (_key_shoot_sound >= 0 && stop)
        {
            _soundManager.Fade(_key_shoot_sound, 0.5f, 0f);
            Invoke("StopFire", 0.5f);
        }
        else if (_key_shoot_sound == -1 && !stop)
        {
            Hashtable param = new Hashtable();
            param.Add("position", _tourelle.position);
            param.Add("loop", true);
            param.Add("volume", 0.5f);
            _key_shoot_sound = _soundManager.PlayAudio(Audio_Type.Flame, param);
            _soundManager.Fade(_key_shoot_sound, 0.5f, 1f);
        }
    }

    private void StopFire()
    {
        _soundManager.stop(_key_shoot_sound);
        _key_shoot_sound = -1;
    }
}