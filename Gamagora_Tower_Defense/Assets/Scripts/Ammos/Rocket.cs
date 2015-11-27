using UnityEngine;
using System.Collections;

public class Rocket : Ammo
{
    public Transform Target;
    public float SecondsToTarget;
    public GameObject TargettingEffect;

    private float _time;
    private bool _targeting = false;
    private float _lerp;
    private Quaternion _lookRotation;
    private Vector3 _direction;
    private GameObject _targetting_effect;

    public Rocket() : base()
    {

    }

    protected override void Awake()
    {
        base.Awake();
        if (TargettingEffect != null)
        {
            Transform pt = transform.FindChild("MuzzleFlash");
            _targetting_effect = (GameObject)Instantiate(TargettingEffect, pt.position, pt.rotation);
            _targetting_effect.transform.parent = transform;
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        _time = 0f;
        _lerp = 5f;
        _targeting = false;
        Target = null;
        _targetting_effect.GetComponent<ParticleSystem>().Stop();
    }

    protected override void Update()
    {
        if (_targeting)
        {
            float speed = 7f;

            Transform new_target = Source.GetComponent<Weapon>().GetTarget();

            if (Target == null && new_target != null)
                _targetting_effect.GetComponent<ParticleSystem>().Play();
            else if (Target != null && new_target == null)
                _targetting_effect.GetComponent<ParticleSystem>().Stop();

            Target = new_target;

            if (Target != null)
            {
                speed *= Target.gameObject.GetComponent<Enemy>().Speed;
                _direction = (Target.position - transform.position).normalized;
                _lookRotation = Quaternion.LookRotation(_direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, _lookRotation, Time.deltaTime * _lerp);
            }
            else
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(transform.forward), Time.deltaTime * _lerp);

            transform.Translate(0, 0, speed * Time.deltaTime, Space.Self);
        }

        if (TimeToExplode > 0f && _time > TimeToExplode)
        {
            Disable();
        }
    }

    protected void FixedUpdate()
    {
        _time += Time.deltaTime;

        if (!_targeting)
        {
            if (_time > SecondsToTarget)
            {
                _targeting = true;
            }
        }
    }
}
