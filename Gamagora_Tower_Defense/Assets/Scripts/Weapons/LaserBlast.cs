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
        GameObject bullet = _bullets.GetNextObj();
        bullet.transform.FindChild("Particle").GetComponent<ParticleSystem>().enableEmission = false;
        bullet.transform.FindChild("Particle").GetComponent<ParticleSystem>().Stop(true);
        bullet.GetComponent<Rigidbody>().isKinematic = false;
        bullet.GetComponent<BoxCollider>().enabled = true;
        bullet.transform.FindChild("Base").gameObject.SetActive(true);
        Transform target = GetTarget();
        if (target != null)
            bullet.GetComponent<Bomb>().Target = target.gameObject;
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
            float h = (dist * dist / 15f);

            // Angle de vision max vers le haut
            if (h > 15f)
                h = 15f;

            // Change bullet speed
            BulletSpeed = 300f + dist * 17f;

            target_pos +=  dist // (target.position - head.position).magnitude
                             * target.forward
                             * target.GetComponent<Enemy>().Speed
                             / (BulletSpeed * 0.02f);

            // Change target y position
            target_pos = target_pos + new Vector3(0f, h, 0f);
            Debug.DrawLine(target_pos, target_pos + target.forward);
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
