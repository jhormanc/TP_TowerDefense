using UnityEngine;
using System.Collections;

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
        Transform tourelle = transform.FindChild("Base").FindChild("Tourelle");
        Transform head = tourelle.FindChild("Head");
        Transform cannon = head.FindChild("Cannon");

        Transform target = GetTarget();
        Vector3 target_pos = Vector3.zero;

        if (target != null)
        {
            float fire_speed = transform.FindChild("Base").FindChild("Tourelle").FindChild("Head").FindChild("Cannon").FindChild("Particle").GetComponent<ParticleDamage>().GetSpeed();
            target_pos = target.position;

            target_pos += (target.position - head.position).magnitude
                             * target.forward
                             * target.GetComponent<Enemy>().Speed
                             / (fire_speed * 3f);

            Debug.DrawLine(target_pos, target_pos + target.forward);
        }

        Move(tourelle, head, cannon, target_pos);
    }

    protected override void Fire()
    {
        Transform shoot_point = transform.FindChild("Base").FindChild("Tourelle").FindChild("Head").FindChild("Cannon");

        StartCoroutine(Fire(shoot_point, null, false));
    }

    protected override void EmitParticle(bool emit)
    {
        Transform p = transform.FindChild("Base").FindChild("Tourelle").FindChild("Head").FindChild("Cannon").FindChild("Particle");

        p.GetComponent<ParticleSystem>().enableEmission = emit;
        p.FindChild("Smoke").GetComponent<ParticleSystem>().enableEmission = emit;
        p.FindChild("Sparkles").GetComponent<ParticleSystem>().enableEmission = emit;

        if (emit)
            p.GetComponent<ParticleSystem>().Play();
        else
            p.GetComponent<ParticleSystem>().Stop();

    }
}