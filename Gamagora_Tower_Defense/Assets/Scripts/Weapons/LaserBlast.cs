using UnityEngine;
using System.Collections;
using System;

public class LaserBlast : Weapon
{
    public LaserBlast() : base()
    {

    }

    protected override void Fire()
    {
        GameObject bullet = _bullets.GetNextObj();
        _particle.Play(true);
        StartCoroutine(WaitEffectFire(_cannon, bullet, false));
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
        Transform target = GetTarget();
        Vector3 target_pos = Vector3.zero;

        if (target != null)
        {
            target_pos = target.position;
            float dist = Vector3.Distance(_head.position, target_pos);
            float h = (dist * dist / 20f);

            // Angle de vision max vers le haut
            if (h > 15f)
                h = 15f;

            target_pos +=  (target.position - _head.position).magnitude
                             * target.forward
                             * target.GetComponent<Enemy>().Speed
                             / (BulletSpeed * 0.1f);

            // Change target y position
            target_pos = target_pos + new Vector3(0f, h, 0f);
            Debug.DrawLine(target_pos, target_pos + target.forward);
        }

        Move(_tourelle, _head, null, target_pos);
    }

    protected override void EmitParticle(bool emit)
    {
        ParticleSystem.EmissionModule em = _particle.emission;
        em.enabled = emit;

        if (!emit)
            _particle.Stop(true);
    }

    public override Audio_Type GetNewWeaponAudioType()
    {
        return Audio_Type.NewLaserBlast;
    }

    protected override void PlayFireSound(bool stop = false)
    {
        // TODO
    }
}
