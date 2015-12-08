using UnityEngine;
using System.Collections;

public class Shotgun : Weapon
{
    // Gestion des deux canons
    private static readonly float LerpTime = 0.5f;
    private bool _selected_cannon;
    private Vector3 _z_cannon1, _z_cannon2;
    private float _lerp_pos1, _lerp_pos2;

    // Transform
    private Transform _cannon1;
    private Transform _cannon2;
    private ParticleSystem _particle1;
    private ParticleSystem _particle2;

    public Shotgun() : base()
    {
        _selected_cannon = true;
        _lerp_pos1 = 0f;
        _lerp_pos2 = 0f;
    }

    protected override void Awake()
    {
        _head = transform.FindChild("Base").FindChild("Tourelle").FindChild("Head");
        _cannon1 = _head.FindChild("Cannon_1");
        _cannon2 = _head.FindChild("Cannon_2");
        _particle1 = _cannon1.FindChild("Particle").GetComponent<ParticleSystem>();
        _particle2 = _cannon2.FindChild("Particle").GetComponent<ParticleSystem>();
        _z_cannon1 = _cannon1.localPosition;
        _z_cannon2 = _cannon2.localPosition;
        base.Awake();
    }

    protected override void Fire()
    {
        Transform canon = GetCannon();
        Transform shoot = canon.FindChild("Shoot");
        GameObject bullet = _bullets.GetNextObj();
        bullet.SetActive(true);

        canon.Translate(new Vector3(0f, 0f, -0.2f), Space.Self);
        StartCoroutine(Fire(shoot, bullet, false));      

        _selected_cannon = !_selected_cannon;
    }

    protected override void Move()
    {
        if (_cannon1.localPosition != _z_cannon1)
        {
            _lerp_pos1 += Time.deltaTime / LerpTime;
            _cannon1.localPosition = Vector3.Lerp(_cannon1.localPosition, _z_cannon1, _lerp_pos1);
        }
        else
            _lerp_pos1 = 0f;

        if (_cannon2.localPosition != _z_cannon2)
        {
            _lerp_pos2 += Time.deltaTime / LerpTime;
            _cannon2.localPosition = Vector3.Lerp(_cannon2.localPosition, _z_cannon2, _lerp_pos2);
        }
        else
            _lerp_pos2 = 0f;

        Transform target = GetTarget();
        Vector3 target_pos = Vector3.zero;

        if (target != null)
        {
            target_pos = target.position;

            target_pos += (target.position - _head.position).magnitude
                             * target.forward
                             * target.GetComponent<Enemy>().Speed
                             / (BulletSpeed * 0.15f);

            Debug.DrawLine(target_pos, target_pos + target.forward);
        }

        Move(_tourelle, _head, null, target_pos);
    }

    protected override void EmitParticle(bool emit)
    {
        ParticleSystem p = GetParticles();

        if(p != null)
        {
            p.enableEmission = emit;

            if (emit)
                p.Emit(100);
        }
    }

    private Transform GetCannon()
    {
        return _selected_cannon ? _cannon1 : _cannon2;
    }

    private ParticleSystem GetParticles()
    {
        return _selected_cannon ? _particle1 : _particle2;
    }

}
