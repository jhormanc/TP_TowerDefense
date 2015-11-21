using UnityEngine;
using System.Collections;

public class LaserBlast : Weapon
{

    public LaserBlast() : base()
    {

    }

    protected override void Awake()
    {
        Transform head = transform.FindChild("Base").FindChild("Tourelle").FindChild("Head");
        base.Awake();
    }

    protected override void Fire()
    {
        Transform shoot = transform.FindChild("Base").FindChild("Tourelle").FindChild("Head").FindChild("Cannon");
        GameObject bullet = _bullets[_bullet_nb];

        bullet.GetComponent<Rigidbody>().isKinematic = false;
        StartCoroutine(Fire(shoot, bullet, false));
    }

    protected override void Move()
    {
        Transform tourelle = transform.FindChild("Base").FindChild("Tourelle");
        Transform head = tourelle.FindChild("Head");
        Transform target = GetTarget();
        Vector3 target_pos = Vector3.zero;

        if (target != null)
        {
            target_pos = target.position;
            float dist = Vector3.Distance(head.position, target_pos);
            float h = Mathf.Tan(35f) * dist;
            target_pos = target_pos + new Vector3(0f, h, 0f);
        }

        Move(tourelle, head, null, target_pos);
        
    }

    protected override void EmitParticle(bool emit)
    {
        Transform p = transform.FindChild("Base").FindChild("Tourelle").FindChild("Head").FindChild("Cannon").FindChild("Particle");

        p.GetComponent<ParticleSystem>().enableEmission = emit;

        if (emit)
            p.GetComponent<ParticleSystem>().Emit(1);
    }
}
