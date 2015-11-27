using UnityEngine;
using System.Collections;

public class LaserBlast : Weapon
{
    public LaserBlast() : base()
    {

    }

    protected override void Fire()
    {
        Transform shoot = transform.FindChild("Base").FindChild("Tourelle").FindChild("Head").FindChild("Cannon");
        GameObject bullet = _bullets.GetNextObj();
        
        StartCoroutine(WaitEffectFire(shoot, bullet, false));
    }

    private IEnumerator WaitEffectFire(Transform shoot, GameObject bullet, bool ray_shoot)
    {
        _allowFire = false;
        yield return new WaitForSeconds(0.4f);
        bullet.SetActive(true);
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
            float h = (dist * dist / 20f);

            // Angle de vision max vers le haut
            if (h > 15f)
                h = 15f;

            target_pos +=  (target.position - head.position).magnitude
                             * target.forward
                             * target.GetComponent<Enemy>().Speed
                             / (BulletSpeed * 0.1f);

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
            p.GetComponent<ParticleSystem>().Play();
        else
            p.GetComponent<ParticleSystem>().Stop();
    }
}
