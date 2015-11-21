using UnityEngine;
using System.Collections;

public class Shotgun : Weapon
{
    // Gestion des deux canons
    private static readonly float LerpTime = 0.5f;
    private bool _selected_cannon;
    private Vector3 _z_cannon1, _z_cannon2;
    private float _lerp_pos1, _lerp_pos2;

    public Shotgun() : base()
    {
        _selected_cannon = true;
        _lerp_pos1 = 0f;
        _lerp_pos2 = 0f;
    }

    protected override void Awake()
    {
        base.Awake();
        Transform head = transform.FindChild("Base").FindChild("Tourelle").FindChild("Head");
        _z_cannon1 = head.FindChild("Cannon_1").localPosition;
        _z_cannon2 = head.FindChild("Cannon_2").localPosition;
    }

    protected override void Fire()
    {
        Transform canon = GetCannon();
        Transform shoot = canon.FindChild("Shoot");
        GameObject bullet = _bullets[_bullet_nb];

        canon.Translate(new Vector3(0f, 0f, -0.2f), Space.Self);
        StartCoroutine(Fire(shoot, bullet, false));      

        _selected_cannon = !_selected_cannon;
    }

    protected override void Move()
    {
        Transform tourelle = transform.FindChild("Base").FindChild("Tourelle");
        Transform head = tourelle.FindChild("Head");
        Transform cannon1 = head.FindChild("Cannon_1");
        Transform cannon2 = head.FindChild("Cannon_2");

        if (cannon1.localPosition != _z_cannon1)
        {
            _lerp_pos1 += Time.deltaTime / LerpTime;
            cannon1.localPosition = Vector3.Lerp(cannon1.localPosition, _z_cannon1, _lerp_pos1);
        }
        else
            _lerp_pos1 = 0f;

        if (cannon2.localPosition != _z_cannon2)
        {
            _lerp_pos2 += Time.deltaTime / LerpTime;
            cannon2.localPosition = Vector3.Lerp(cannon2.localPosition, _z_cannon2, _lerp_pos2);
        }
        else
            _lerp_pos2 = 0f;

        Move(tourelle, head);
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
