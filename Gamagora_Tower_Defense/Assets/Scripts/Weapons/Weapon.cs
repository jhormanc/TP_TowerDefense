﻿using UnityEngine;
using System.Collections;
using System;

public class Weapon : MonoBehaviour
{
    public class EnemySort : IComparer
    {
        private AimType _type;

        public EnemySort(AimType type)
        {
            _type = type;
        }

        public int Compare(object x, object y)
        {
            if(_type == AimType.First)
                return ((GameObject)x).GetComponent<Enemy>().GetDistFromTarget().CompareTo(((GameObject)y).GetComponent<Enemy>().GetDistFromTarget());
            else if(_type == AimType.Last)
                return ((GameObject)y).GetComponent<Enemy>().GetDistFromTarget().CompareTo(((GameObject)x).GetComponent<Enemy>().GetDistFromTarget());
            else
                return ((GameObject)y).GetComponent<Enemy>().GetStrength().CompareTo(((GameObject)x).GetComponent<Enemy>().GetStrength());
        }
    }

    public enum AimType { First, Last, Strongest };

    // Values that will be set in the Inspector
    public float Range;
    public GameObject Bullet;
    public float FireRate;
    public float BulletSpeed;
    public float Degats;
    public int BulletsSize;
    public bool Auto; // Visée auto ou manuelle
    public AimType Aim;

    // Values for internal use
    protected static readonly float DeltaRot = 0.3f;

    // Visée
    protected static readonly float RotationSpeed = 3f; // Rotation de l'arme lors d'un mouvement clavier
    protected Quaternion _lookRotation;
    protected Vector3 _direction;
    protected float _angleRotation;

    // Shoot
    protected bool _fire, _lastFire, _allowFire;

    // Bullets
    protected PullManager _bullets;

    // Ennemis
    protected ArrayList _targets;

    // Use this for initialization
    protected virtual void Awake()
    {
        _direction = new Vector3();
        _angleRotation = 0f;
        _fire = false;
        _allowFire = true;
        Auto = true;
        _targets = new ArrayList();
        GetComponent<SphereCollider>().radius = Range;
        FireRate = Mathf.Abs(FireRate);
        if (FireRate == 0f)
            FireRate = 1f;

        if (Bullet != null && BulletsSize > 0)
        {
            _bullets = ScriptableObject.CreateInstance<PullManager>();
            _bullets.Init(Bullet, BulletsSize);
        }
        else
            _bullets = null;

        EmitParticle(false);
        Move();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if ((Auto && !_lastFire && _targets.Count > 0 && CanFire()) || (!Auto && Input.GetKeyDown(KeyCode.F)))
        {
            _fire = true;
        }
        else if ((Auto && _lastFire && (_targets.Count == 0 || !CanFire())) || (!Auto && Input.GetKeyUp(KeyCode.F)))
        {
            _fire = false;
            SetFire();
        }

        _lastFire = _fire;

        Move();
  
        if (_fire && _allowFire && CanFire())
        {
            SetFire();
            Fire();
        }
    }

    void FixedUpdate()
    {

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Enemy")
        {
            _targets.Add(other.transform.gameObject);
            SortTargets();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Enemy")
            _targets.Remove(other.transform.gameObject);
    }

    bool CanFire()
    {
        Transform tourelle = transform.FindChild("Base").FindChild("Tourelle");
        Transform head = tourelle.FindChild("Head");

        return !Auto || Quaternion.Angle(head != null ? head.rotation : tourelle.rotation, _lookRotation) < 5f;
    }

    void SetFire()
    {
        EmitParticle(_fire);
        AudioSource audio = GetComponent<AudioSource>();
        if (audio != null)
        {
            if (_fire)
                audio.Play();
            else
                audio.Stop();
        }
    }

    protected virtual void Fire()
    {
        Transform shoot_point = transform.FindChild("Base").FindChild("Tourelle").FindChild("Cannon").FindChild("Shoot");
        GameObject bullet = null;

        if(_bullets != null)
            bullet = _bullets.GetNextObj();

        StartCoroutine(Fire(shoot_point, bullet, true));
    }

    public void ChangeAim(AimType new_aim)
    {
        Aim = new_aim;
    }

    protected IEnumerator Fire(Transform shoot_point, GameObject bullet, bool ray_shoot)
    {
        RaycastHit hit_info;
        bool fire_bullet = bullet != null;

        _allowFire = false;

        if (fire_bullet)
        {
            bullet.SetActive(true);
            bullet.GetComponent<Ammo>().Source = gameObject;
            bullet.transform.position = shoot_point.position;
            bullet.transform.rotation = shoot_point.rotation;
            bullet.transform.forward = shoot_point.forward;
            bullet.GetComponent<Rigidbody>().velocity = shoot_point.forward;
            bullet.GetComponent<Rigidbody>().AddForce(Vector3.zero);
            bullet.GetComponent<Rigidbody>().AddForce(shoot_point.forward * BulletSpeed);
        }

        if (ray_shoot)
        {
            bool hit = Physics.Raycast(shoot_point.position, shoot_point.forward, out hit_info, 2000);

            if (hit && hit_info.collider.tag == "Enemy")
            {
                Enemy enemy = hit_info.collider.gameObject.transform.GetComponent<Enemy>();
                if (enemy != null && !enemy.IsDead())
                {
                    float dmg = CalculateDamage(bullet, enemy);
                    enemy.ReceiveDamage(dmg);
                    bullet.GetComponent<Ammo>().SpawnEffect(hit_info.point, hit_info.transform.rotation);
                }
            }
        }

        yield return new WaitForSeconds(1f / FireRate);

        _allowFire = true;
    }

    public virtual float CalculateDamage(GameObject bullet, Enemy enemy)
    {
        float ret = 0f;

        if (bullet != null)
            ret = bullet.GetComponent<Ammo>().Degats;
        else
            ret = 1f;

        return ret * Degats;
    }

    protected void Move(Transform tourelle, Transform head = null, Transform look_point = null, Vector3 target = default(Vector3))
    {
        if (Auto)
        {
            float speed = 1f;
            Transform t = GetTarget();

            if (t != null)
                speed = t.gameObject.GetComponent<Enemy>().Speed;

            if (target == Vector3.zero)
            {
                if(t != null)
                    target = t.position;
            }

            if (target != Vector3.zero)
            {
                // Find the vector pointing from our position to the target
                _direction = (target - (look_point != null ? look_point.position : head != null ? head.position : tourelle.position)).normalized;

                // Create the rotation we need to be in to look at the target
                _lookRotation = Quaternion.LookRotation(_direction);

                // Rotate us over time according to speed until we are in the required rotation
                speed = speed * 10f;

                if (Quaternion.Angle(tourelle.rotation, _lookRotation) < 5f)
                    speed *= 2f;

                tourelle.rotation = Quaternion.Slerp(tourelle.rotation, _lookRotation, Time.deltaTime * speed);

                if (head != null)
                {
                    tourelle.rotation = Quaternion.Euler(new Vector3(0f, tourelle.rotation.eulerAngles.y, 0f));
                    head.rotation = Quaternion.Slerp(head.rotation, _lookRotation, Time.deltaTime * speed);
                }                    
            }
        }
        else
        {
            Transform cam = transform.FindChild("Camera").GetComponent<Camera>().transform;
            Transform h = (head != null ? head : tourelle.FindChild("Cannon"));
            Vector3 dir = (h.position - cam.position);

            if (Input.GetKey(KeyCode.Z) && _angleRotation < 90f)
            {
                _angleRotation += DeltaRot * RotationSpeed;

                if(head != null)
                    head.Rotate(new Vector3(DeltaRot * RotationSpeed, 0, 0));
                else
                    tourelle.Rotate(new Vector3(DeltaRot * RotationSpeed, 0, 0));
            }
            else if (Input.GetKey(KeyCode.S) && _angleRotation > -45f)
            {
                _angleRotation -= DeltaRot * RotationSpeed;

                if (head != null)
                    head.Rotate(new Vector3(-DeltaRot * RotationSpeed, 0, 0));
                else
                    tourelle.Rotate(new Vector3(-DeltaRot * RotationSpeed, 0, 0));
            }

            if (Input.GetKey(KeyCode.D))
                tourelle.RotateAround(tourelle.position, new Vector3(0, 1, 0), DeltaRot * RotationSpeed);
            else if (Input.GetKey(KeyCode.Q))
                tourelle.RotateAround(tourelle.position, new Vector3(0, 1, 0), -DeltaRot * RotationSpeed);

            
            Vector3 aim = h.position + h.transform.forward * 5f;
            Quaternion cam_look = Quaternion.LookRotation((aim - cam.position).normalized);
            Vector3 cam_pos = h.position - dir.magnitude * h.forward;

            float speed = 5f;

            cam.rotation = Quaternion.Slerp(cam.rotation, cam_look, Time.deltaTime * speed);
            cam.position = Vector3.Slerp(cam.position, cam_pos + cam.up, Time.deltaTime * speed);
        }
    }

    protected virtual void Move()
    {
        Transform tourelle = transform.FindChild("Base").FindChild("Tourelle");
        Transform look = tourelle.FindChild("Cannon").FindChild("Shoot");

        Move(tourelle, null, look);
    }

    protected virtual void EmitParticle(bool emit)
    {
        Transform p = transform.FindChild("Base").FindChild("Tourelle").FindChild("Particle");
        p.GetComponent<ParticleSystem>().enableEmission = emit;
        p.FindChild("Smoke").GetComponent<ParticleSystem>().enableEmission = emit;
        p.FindChild("Sparks").GetComponent<ParticleSystem>().enableEmission = emit;
    }

    protected Transform GetTarget()
    {
        if (_targets.Count > 0 && _targets[0] != null)
            return ((GameObject)_targets[0]).transform;
        return null;
    }

    public void RemoveTarget(GameObject enemy)
    {
        if (_targets.Count > 0)
        {
            _targets.Remove(enemy);
        }
    }

    protected void SortTargets()
    {
        _targets.Sort(0, _targets.Count, new EnemySort(Aim));
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, Range);
    }
}
