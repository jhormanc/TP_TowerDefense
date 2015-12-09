using UnityEngine;
using System.Collections;

public class Rocket : Ammo
{
    public Transform Target;
    public float SecondsToTarget;
    public GameObject TargettingEffect;
    public int ChanceToMiss;

    private float _time;
    private bool _targeting = false;
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
            float speed = 5f;

            Vector3 target_pos = Target != null ? Target.position : Vector3.zero;

            if (target_pos != Vector3.zero)
            {
                float enemy_speed = Target.GetComponent<Enemy>().Speed;

                if(ChanceToMiss > 0 && ChanceToMiss <= 100)
                {
                    int rand = Random.Range(1, 100);

                    if (rand <= ChanceToMiss)
                        target_pos += _delta;
                }
               
                speed *= enemy_speed;
                _direction = target_pos - transform.position;

                if (transform.position.y < target_pos.y)
                    _direction = Vector3.down;

                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(_direction), speed * Time.deltaTime);
            }

            GetComponent<Rigidbody>().AddForce(transform.forward * 4f * speed * Time.deltaTime, ForceMode.Impulse);
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

    protected override void PlayExplosionSound()
    {
        Hashtable param = new Hashtable();
        param.Add("position", transform.position);
        param.Add("spatialBlend", 0.5f);
        param.Add("volume", 1f);
        int key = _soundManager.PlayAudio(Audio_Type.ExplosionMissile, param);
        _soundManager.Fade(key, 0.5f, 0f);
        StartCoroutine(StopSound(0.5f, key));
    }

    private IEnumerator StopSound(float time, int key)
    {
        yield return new WaitForSeconds(time);

        _soundManager.Stop(key);
    }
}
