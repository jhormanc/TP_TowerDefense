using UnityEngine;
using System.Collections;

public class RocketLauncher : Weapon
{
    private int _selected_cannon;

	public RocketLauncher() : base()
    {
   
    }

    protected override void Move()
    {
        Transform tourelle = transform.FindChild("Base").FindChild("Tourelle");

        Move(tourelle, tourelle.FindChild("Head"));
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
