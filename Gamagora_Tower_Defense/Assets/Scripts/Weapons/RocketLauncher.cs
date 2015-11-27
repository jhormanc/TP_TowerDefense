using UnityEngine;
using System.Collections;

public class RocketLauncher : Weapon
{
    private int _selected_cannon = 1;

	public RocketLauncher() : base()
    {

    }

    protected void OnEnable()
    {
        _selected_cannon = 1;
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
            float h = (dist * dist / 10f);

            // Angle de vision max vers le haut
            if (h > 20f)
                h = 20f;

            target_pos += (target.position - head.position).magnitude
                             * target.forward
                             * target.GetComponent<Enemy>().Speed
                             / (BulletSpeed);

            // Change target y position
            target_pos = target_pos + new Vector3(0f, h, 0f);
            Debug.DrawLine(target_pos, target_pos + target.forward);
        }

        Move(tourelle, head, null, target_pos);
    }



    protected override void Fire()
    {
        Transform shoot = GetCannon();
        GameObject bullet = _bullets.GetNextObj();

        EmitParticle(true);
        StartCoroutine(Fire(shoot, bullet, false));

        _selected_cannon++;
        if (_selected_cannon > 6)
            _selected_cannon = 1;
    }

    protected override void EmitParticle(bool emit)
    {
        Transform p = GetCannon().FindChild("Particle");

        p.GetComponent<ParticleSystem>().enableEmission = emit;

        if(emit)
            p.GetComponent<ParticleSystem>().Emit(100);
    }

    private Transform GetCannon()
    {
        Transform head = transform.FindChild("Base").FindChild("Tourelle").FindChild("Head");
        Transform cannon = head.FindChild(string.Format("Cannon_{0}", _selected_cannon));

        return cannon;
    }
}
