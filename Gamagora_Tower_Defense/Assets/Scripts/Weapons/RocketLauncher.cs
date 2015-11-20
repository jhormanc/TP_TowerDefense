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
        GameObject bullet = _bullets[_bullet_nb];

        EmitParticle(true);

        StartCoroutine(Fire(shoot, bullet, false));

        _selected_cannon++;
        if (_selected_cannon > 5)
            _selected_cannon = 0;
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
        Transform cannon = null;

        switch(_selected_cannon)
        {
            case 0:
                cannon = head.FindChild("Cannon_1");
                break;
            case 1:
                cannon = head.FindChild("Cannon_2");
                break;
            case 2:
                cannon = head.FindChild("Cannon_3");
                break;
            case 3:
                cannon = head.FindChild("Cannon_4");
                break;
            case 4:
                cannon = head.FindChild("Cannon_5");
                break;
            case 5:
                cannon = head.FindChild("Cannon_6");
                break;
        }

        return cannon;
    }
}
