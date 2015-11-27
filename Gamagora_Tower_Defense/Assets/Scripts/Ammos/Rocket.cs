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
    private Vector3 _direction;
    private Vector3 _delta;
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
        _time = 0f;
        _lerp = 5f;
        _targeting = false;
        Target = null;
        base.OnEnable();
    }

    protected override void Update()
    {
        _time += Time.deltaTime;

        if (!_targeting)
        {
            if (_time > SecondsToTarget)
            {
                _targeting = true;
                Target = Source.GetComponent<Weapon>().GetTarget();
                _targetting_effect.GetComponent<ParticleSystem>().Play();
            }
        }

        if (TimeToExplode > 0f && _time > TimeToExplode)
        {
            Disable();
        }
    }

    protected void FixedUpdate()
    {
        if (_targeting)
        {
            float speed = 7f;

            Vector3 target_pos = Target != null ? Target.position : Vector3.zero;

            if (target_pos != Vector3.zero)
            {
                float enemy_speed = Target.GetComponent<Enemy>().Speed;
                target_pos += _delta;

                speed *= enemy_speed;
                _direction = target_pos - transform.position;

                if (transform.position.y < target_pos.y)
                    _direction = Vector3.down;

                // Get the angle between transform.forward and target delta
                float angle_diff = Vector3.Angle(transform.forward, _direction);

                // Get its cross product, which is the axis of rotation to get from one vector to the other
                Vector3 cross = Vector3.Cross(transform.forward, _direction);

                // Apply torque along that axis according to the magnitude of the angle.
                GetComponent<Rigidbody>().AddTorque(cross * angle_diff * speed * Time.deltaTime);
            }

            transform.Translate(0, 0, speed * Time.deltaTime, Space.Self);
        }
        else
        {
            float angle_down = Vector3.Angle(transform.forward, Vector3.down);
            Vector3 cross_down = Vector3.Cross(transform.forward, Vector3.down);
            GetComponent<Rigidbody>().AddTorque(cross_down * 0.3f * angle_down * Time.deltaTime);
        }
    }

    public override void SpawnEffect(Vector3 pos, Quaternion rot, bool stop = true)
    {
        _targetting_effect.GetComponent<ParticleSystem>().Stop();
        base.SpawnEffect(pos, rot, stop);
    }

    public void Init(float error)
    {
        _delta = new Vector3(Random.Range(-error, error), 0f, Random.Range(-error, error));
    }
}
