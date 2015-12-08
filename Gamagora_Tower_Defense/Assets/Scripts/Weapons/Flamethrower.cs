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
}