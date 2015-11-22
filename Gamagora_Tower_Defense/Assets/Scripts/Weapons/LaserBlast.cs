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
        bullet.transform.FindChild("Particle").GetComponent<ParticleSystem>().enableEmission = false;
        bullet.transform.FindChild("Particle").GetComponent<ParticleSystem>().Stop(true);
        bullet.GetComponent<Rigidbody>().isKinematic = false;
        bullet.GetComponent<BoxCollider>().enabled = true;
        bullet.GetComponent<Bomb>().Target = GetTarget().gameObject;
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
            float h = dist * dist / 10f - 1f;
            Vector3 l = target.forward * target.GetComponent<Enemy>().Speed * dist / 10f;
            // TODO
            //BulletSpeed = 
            target_pos = target_pos + new Vector3(l.x, h, l.y);
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
