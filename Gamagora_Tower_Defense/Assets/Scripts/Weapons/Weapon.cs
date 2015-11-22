using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour
{
    // Values that will be set in the Inspector
    public float Range;
    public GameObject Bullet;
    public float FireRate;
    public float BulletSpeed;
    public float Degats;
    public int BulletsSize;

    // Values for internal use
    protected static readonly float DeltaRot = 0.3f;

    // Visée
    protected static readonly float RotationSpeed = 3f; // Rotation de l'arme lors d'un mouvement clavier
    protected Quaternion _lookRotation;
    protected Vector3 _direction;
    protected float _angleRotation;
    protected bool _auto;

    // Shoot
    protected bool _fire, _lastFire, _allowFire;

    // Bullets
    protected GameObject[] _bullets;
    protected int _bullet_nb;

    // Ennemis
    protected ArrayList _targets;
    protected int _id_target;

    // Use this for initialization
    protected virtual void Awake()
    {
        _direction = new Vector3();
        _angleRotation = 0f;
        _fire = false;
        _allowFire = true;
        _auto = true;
        _bullets = new GameObject[BulletsSize];
        _bullet_nb = 0;
        _targets = new ArrayList();
        _id_target = 0;
        GetComponent<SphereCollider>().radius = Range;

        if (Bullet != null)
        {
            for (int i = 0; i < BulletsSize; i++)
            {
                _bullets[i] = (GameObject)Instantiate(Bullet);
                _bullets[i].SetActive(false);
            }
        }

        EmitParticle(false);
        Move();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            _auto = !_auto;

            if (_auto == false)
            {
                _fire = false;
                SetFire();
                Transform tourelle = transform.FindChild("Base").FindChild("Tourelle");
                Transform head = tourelle.FindChild("head");
                _angleRotation = head != null ? head.rotation.x : tourelle.rotation.x;
            }
        }

        if ((_auto && !_lastFire && _targets.Count > 0 && CanFire()) || (!_auto && Input.GetKeyDown(KeyCode.F)))
        {
            _fire = true;
        }
        else if ((_auto && _lastFire && (_targets.Count == 0 || !CanFire())) || (!_auto && Input.GetKeyUp(KeyCode.F)))
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

        return !_auto || Quaternion.Angle(head != null ? head.rotation : tourelle.rotation, _lookRotation) < 5f;
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

        if(Bullet != null)
            bullet = _bullets[_bullet_nb];

        StartCoroutine(Fire(shoot_point, bullet, true));

    }

    protected IEnumerator Fire(Transform shoot_point, GameObject bullet, bool ray_shoot)
    {
        RaycastHit hit_info;
        bool fire_bullet = bullet != null;

        _allowFire = false;

        if (fire_bullet)
        {
            bullet.SetActive(true);
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
                }
            }
        }

        if (fire_bullet)
        {
            if (_bullet_nb < BulletsSize - 1)
                _bullet_nb++;
            else
                _bullet_nb = 0;
        }

        yield return new WaitForSeconds(FireRate != 0f ? 1f / Mathf.Abs(FireRate) : 0f);

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
        if (_auto)
        {
            if (target == Vector3.zero)
            {
                Transform t = GetTarget();
                if(t != null)
                    target = t.position;
            }

            if (target != null)
            {
                // Find the vector pointing from our position to the target
                _direction = (target - (look_point != null ? look_point.position : head != null ? head.position : tourelle.position)).normalized;

                // Create the rotation we need to be in to look at the target
                _lookRotation = Quaternion.LookRotation(_direction);

                // Rotate us over time according to speed until we are in the required rotation
                float speed = 3f;

                if (Quaternion.Angle(tourelle.rotation, _lookRotation) < 5f)
                    speed *= 5f;

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
                    tourelle.Rotate(new Vector3(DeltaRot * RotationSpeed, 0, 0));
            }

            if (Input.GetKey(KeyCode.D))
                tourelle.RotateAround(tourelle.position, new Vector3(0, 1, 0), DeltaRot * RotationSpeed);
            else if (Input.GetKey(KeyCode.Q))
                tourelle.RotateAround(tourelle.position, new Vector3(0, 1, 0), -DeltaRot * RotationSpeed);
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

    protected IEnumerator DisableBulletEffect(int bullet_nb, float delayTime)
    {
        int i = bullet_nb;
        yield return new WaitForSeconds(delayTime);

        _bullets[i].GetComponent<TrailRenderer>().enabled = false;
    }

    protected Transform GetTarget()
    {
        int id = GetIdTarget();
        if (_targets.Count > 0 && _targets[id] != null)
            return ((GameObject)_targets[id]).transform;
        return null;
    }

    public void RemoveTarget(GameObject enemy)
    {
        if (_targets.Count > 0)
            _targets.Remove(enemy);
    }

    protected int GetIdTarget()
    {
        int id = 0;

        switch (_id_target)
        {
            case 0:
                id = 0;
                break;
            case 1:
                id = _targets.Count - 1;
                break;
        }

        return id;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, Range);
    }
}
