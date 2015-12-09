using UnityEngine;
using System;
using System.Collections;

public class RocketLauncher : Weapon
{
    // distance maximum d'erreur possible pour le missile
    public float MissileErrorMax;
    private int _selected_cannon = 1;

    // Transform
    private Transform _cannon1;
    private Transform _cannon2;
    private Transform _cannon3;
    private Transform _cannon4;
    private Transform _cannon5;
    private Transform _cannon6;

    private ParticleSystem _particle1;
    private ParticleSystem _particle2;
    private ParticleSystem _particle3;
    private ParticleSystem _particle4;
    private ParticleSystem _particle5;
    private ParticleSystem _particle6;

    public RocketLauncher() : base()
    {

    }

    protected override void Awake()
    {
        base.Awake();
        _cannon1 = _head.FindChild("Cannon_1");
        _cannon2 = _head.FindChild("Cannon_2");
        _cannon3 = _head.FindChild("Cannon_3");
        _cannon4 = _head.FindChild("Cannon_4");
        _cannon5 = _head.FindChild("Cannon_5");
        _cannon6 = _head.FindChild("Cannon_6");
        _particle1 = _cannon1.FindChild("Particle").GetComponent<ParticleSystem>();
        _particle2 = _cannon2.FindChild("Particle").GetComponent<ParticleSystem>();
        _particle3 = _cannon3.FindChild("Particle").GetComponent<ParticleSystem>();
        _particle4 = _cannon4.FindChild("Particle").GetComponent<ParticleSystem>();
        _particle5 = _cannon5.FindChild("Particle").GetComponent<ParticleSystem>();
        _particle6 = _cannon6.FindChild("Particle").GetComponent<ParticleSystem>();
    }

    protected void OnEnable()
    {
        _selected_cannon = 1;
    }

    protected override void Move()
    {
        Transform target = GetTarget();
        Vector3 target_pos = Vector3.zero;

        if (target != null)
        {
            target_pos = target.position;
            float dist = Vector3.Distance(_head.position, target_pos);
            float h = 0.1f * dist * dist - 5f * dist + 80f;

            // Angle de vision max vers le haut
            if (h > 40f)
                h = 40f;

            target_pos += (target.position - _head.position).magnitude
                             * target.forward
                             * target.GetComponent<Enemy>().Speed
                             / (BulletSpeed);

            // Change target y position
            target_pos = target_pos + new Vector3(0f, h, 0f);
            Debug.DrawLine(target_pos, target_pos + target.forward);
        }

        Move(_tourelle, _head, null, target_pos);
    }



    protected override void Fire()
    {
        Transform shoot = GetCannon();
        GameObject bullet = _bullets.GetNextObj();

        bullet.GetComponent<Rocket>().Init(MissileErrorMax);
        EmitParticle(true);
        StartCoroutine(Fire(shoot, bullet, false));

        _selected_cannon++;
        if (_selected_cannon > 6)
            _selected_cannon = 1;
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
        if (_selected_cannon == 1)
            return _cannon1;
        else if (_selected_cannon == 2)
            return _cannon2;
        else if (_selected_cannon == 3)
            return _cannon3;
        else if (_selected_cannon == 4)
            return _cannon4;
        else if (_selected_cannon == 5)
            return _cannon5;
        else if (_selected_cannon == 6)
            return _cannon6;
        else
            return _cannon1;
    }

    private ParticleSystem GetParticles()
    {
        if (_selected_cannon == 1)
            return _particle1;
        else if (_selected_cannon == 2)
            return _particle2;
        else if (_selected_cannon == 3)
            return _particle3;
        else if (_selected_cannon == 4)
            return _particle4;
        else if (_selected_cannon == 5)
            return _particle5;
        else if (_selected_cannon == 6)
            return _particle6;
        else
            return _particle1;
    }

    public override Audio_Type GetNewWeaponAudioType()
    {
        return Audio_Type.NewRocketLauncher;
    }

    protected override void PlayFireSound(bool stop = false)
    {
        if (stop)
        {
            _soundManager.Stop(_key_shoot_sound);
            _key_shoot_sound = -1;
        }
        else
        {
            Hashtable param = new Hashtable();
            param.Add("position", _tourelle.position);
            //param.Add("pitch", FireRate / 7f);
            param.Add("loop", false);
            param.Add("spatialBlend", 0.5f);
            _key_shoot_sound = _soundManager.PlayAudio(Audio_Type.RocketLauncherShoot, param);
            StartCoroutine(StopSound(0.5f, _key_shoot_sound));
        }
    }

    private IEnumerator StopSound(float time, int key)
    {
        yield return new WaitForSeconds(time);

        _soundManager.Stop(key);
    }
}
