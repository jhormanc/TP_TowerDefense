using UnityEngine;
using System.Collections;

public class Shotgun : Weapon
{
    private bool _selected_cannon;

    public Shotgun() : base()
    {
        _selected_cannon = true;
    }

    protected override void Fire()
    {
        Transform canon = GetCannon();
        Transform shoot = canon.FindChild("Shoot");
        GameObject bullet = _bullets[_bullet_nb];

        StartCoroutine(Fire(shoot, bullet, false));

        _selected_cannon = !_selected_cannon;
    }

    protected override void Move()
    {
        Transform tourelle = transform.FindChild("Base").FindChild("Tourelle");

        Move(tourelle, tourelle.FindChild("Head"));
    }

    protected override void EmitParticle(bool emit)
    {
        Transform p = GetCannon().FindChild("Particle");

        p.GetComponent<ParticleSystem>().enableEmission = emit;

        if (emit)
            p.GetComponent<ParticleSystem>().Emit(100);
    }

    private Transform GetCannon()
    {
        Transform head = transform.FindChild("Base").FindChild("Tourelle").FindChild("Head");
        Transform canon = null;

        if(head != null)
            canon = _selected_cannon ? head.FindChild("Cannon_1") : head.FindChild("Cannon_2");

        return canon;
    }

}
