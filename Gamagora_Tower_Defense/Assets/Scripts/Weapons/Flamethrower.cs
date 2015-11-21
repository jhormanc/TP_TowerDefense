using UnityEngine;
using System.Collections;

public class Flamethrower : Weapon {

    public Flamethrower() : base()
    {

    }

    protected override void Move()
    {
        Transform tourelle = transform.FindChild("Base").FindChild("Tourelle");
        Transform head = tourelle.FindChild("Head");
        Transform cannon = head.FindChild("Cannon");

        Move(tourelle, head, cannon);
    }

    protected override void EmitParticle(bool emit)
    {
        Transform p = transform.FindChild("Base").FindChild("Tourelle").FindChild("Head").FindChild("Cannon").FindChild("Particle");

        p.GetComponent<ParticleSystem>().enableEmission = emit;
        p.FindChild("Smoke").GetComponent<ParticleSystem>().enableEmission = emit;
        p.FindChild("Sparkles").GetComponent<ParticleSystem>().enableEmission = emit;
    }
}