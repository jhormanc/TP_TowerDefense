using UnityEngine;
using System;

public class Generator : Weapon
{
    public GameObject LightingParticle;

    private static readonly int ParticlesSize = 100;
    private GameObject[] _particles;

    public Generator() : base()
    {
        if (LightingParticle != null)
        {
            _particles = new GameObject[ParticlesSize];
            for (int i = 0; i < ParticlesSize; i++)
            {
                _particles[i] = Instantiate<GameObject>(LightingParticle);
                _particles[i].SetActive(false);
            }
        }
    }

    protected override void Awake()
    {
        base.Awake();

        LightingParticle.SetActive(false);
    }

    protected override void Fire()
    {
        
    }

    protected override void Move()
    {
        Transform tourelle = transform.FindChild("Base").FindChild("Tourelle");
        Transform head = tourelle.FindChild("Cannon");

        Move(tourelle, head);
    }

    protected override void EmitParticle(bool emit)
    {
        Transform p = transform.FindChild("Base").FindChild("Tourelle").FindChild("Cannon").FindChild("Particle");

        p.GetComponent<ParticleSystem>().enableEmission = emit;
        p.FindChild("Lightning").GetComponent<ParticleSystem>().enableEmission = emit;
        p.FindChild("Spakles").GetComponent<ParticleSystem>().enableEmission = emit;
        p.FindChild("Ring").GetComponent<ParticleSystem>().enableEmission = emit;
        p.FindChild("Ray").GetComponent<ParticleSystem>().enableEmission = emit;

        if (emit)
            p.GetComponent<ParticleSystem>().Emit(1);
    }

    public override Audio_Type GetNewWeaponAudioType()
    {
        return Audio_Type.NULL;
    }

    protected override void PlayFireSound(bool stop = false)
    {
        // TODO
    }
}
