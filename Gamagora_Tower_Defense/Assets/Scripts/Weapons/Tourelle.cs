using UnityEngine;
using System.Collections;

public class Tourelle : MonoBehaviour
{
    // Values that will be set in the Inspector
    public float Range;
    public GameObject Bullet;
    public int FireRate;
    
    // Values for internal use
    private static readonly float DeltaRot = 0.3f;
    private static readonly int BulletsSize = 100;

    // Visée
    private static readonly float RotationSpeed = 3f; // Rotation de la tourelle lors d'un mouvement clavier
    private Quaternion _lookRotation;
    private Vector3 _direction;
    private float _angleRotation;
    private bool _auto;

    // Shoot
    private bool _fire, _lastFire, _allowFire;
    private Quaternion _canonRotation;

    // Bullets
    private static readonly float BulletSpeed = 5000f;
    private GameObject[] _bullets;
    private int _bullet_nb;

    // Ennemis
    private ArrayList _targets;
    private int _id_target;

    // Use this for initialization
    void Awake()
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

        for (int i = 0; i < BulletsSize; i++)
        {
            _bullets[i] = (GameObject)Instantiate(Bullet);
            _bullets[i].SetActive(false);
        }
        
        EmitParticle(false);
        Move();
    }
	
	// Update is called once per frame
	void Update()
    {
        Transform canon = transform.FindChild("Base").FindChild("Tourelle").FindChild("Canon");

        if (Input.GetKeyDown(KeyCode.A))
        {
            _auto = !_auto;
            if (_auto == false)
            {
                _fire = false;
                SetFire();
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

        canon.localRotation = Quaternion.Slerp(canon.localRotation, _canonRotation, Time.deltaTime * FireRate);
    }

    void FixedUpdate()
    {
        if (_fire && _allowFire && CanFire())
        {
            SetFire();
            StartCoroutine(Fire());
        }
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

        return !_auto || Quaternion.Angle(tourelle.rotation, _lookRotation) < 5f;
    }

    void SetFire()
    {
        EmitParticle(_fire);
        AudioSource audio = GetComponent<AudioSource>();
        if (_fire)
            audio.Play();
        else
            audio.Stop();
    }

    IEnumerator Fire()
    {
        _allowFire = false;
        
        RaycastHit hit_info;
        Transform canon = transform.FindChild("Base").FindChild("Tourelle").FindChild("Canon");
        Transform shoot_point = canon.FindChild("Shoot");
        GameObject bullet = _bullets[_bullet_nb];

        bullet.GetComponent<Bullet>().Source = this;
        bullet.SetActive(true);
        bullet.transform.position = shoot_point.position;
        bullet.transform.rotation = shoot_point.rotation;
        bullet.transform.forward = shoot_point.forward;
        bullet.GetComponent<Rigidbody>().velocity = shoot_point.forward;
        bullet.GetComponent<Rigidbody>().AddForce(Vector3.zero);
        bullet.GetComponent<Rigidbody>().AddForce(shoot_point.forward * BulletSpeed);

        bullet.GetComponent<TrailRenderer>().enabled = true;
        
        StartCoroutine(DisableBulletEffect(_bullet_nb, 0.5f));

        _canonRotation = canon.localRotation * Quaternion.Euler(0, 0, 45);

        bool hit = Physics.Raycast(shoot_point.position, shoot_point.forward, out hit_info, 2000);

        if (hit && hit_info.collider.tag == "Enemy")
        {
            Enemy enemy = hit_info.collider.gameObject.transform.GetComponent<Enemy>();
            if (enemy != null && !enemy.IsDead())
                enemy.ReceiveDamage(bullet.GetComponent<Bullet>().Degats, this);
        }

        if (_bullet_nb < BulletsSize - 1)
            _bullet_nb++;
        else
            _bullet_nb = 0;

        yield return new WaitForSeconds(FireRate != 0 ? 1f / Mathf.Abs((float)FireRate) : 0f);
        _allowFire = true;
    }

    void Move()
    {
        Transform tourelle = transform.FindChild("Base").FindChild("Tourelle");

        if (_auto)
        {
            Transform target = GetTarget();

            if (target != null)
            {
                // Find the vector pointing from our position to the target
                _direction = (target.position - tourelle.FindChild("Canon").FindChild("Shoot").position).normalized;

                // Create the rotation we need to be in to look at the target
                _lookRotation = Quaternion.LookRotation(_direction);

                // Rotate us over time according to speed until we are in the required rotation
                float speed = 3f;

                if (Quaternion.Angle(tourelle.rotation, _lookRotation) < 5f)
                    speed *= 5f;

                tourelle.rotation = Quaternion.Slerp(tourelle.rotation, _lookRotation, Time.deltaTime * speed);
            }
        }
        else
        {
            if (Input.GetKey(KeyCode.Z) && _angleRotation < 45f)
            {
                _angleRotation += DeltaRot * RotationSpeed;
                tourelle.Rotate(new Vector3(DeltaRot * RotationSpeed, 0, 0));
            }
            else if (Input.GetKey(KeyCode.S) && _angleRotation > -45f)
            {
                _angleRotation -= DeltaRot * RotationSpeed;
                tourelle.Rotate(new Vector3(-DeltaRot * RotationSpeed, 0, 0));
            }

            if (Input.GetKey(KeyCode.D))
                tourelle.RotateAround(tourelle.position, new Vector3(0, 1, 0), DeltaRot * RotationSpeed);
            else if (Input.GetKey(KeyCode.Q))
                tourelle.RotateAround(tourelle.position, new Vector3(0, 1, 0), -DeltaRot * RotationSpeed);
        }
    }

    void EmitParticle(bool emit)
    {
        Transform p = transform.FindChild("Base").FindChild("Tourelle").FindChild("Particule");
        p.GetComponent<ParticleSystem>().enableEmission = emit;
        p.FindChild("Smoke").GetComponent<ParticleSystem>().enableEmission = emit;
        p.FindChild("Sparks").GetComponent<ParticleSystem>().enableEmission = emit;
    }

    IEnumerator DisableBulletEffect(int bullet_nb, float delayTime)
    {
        int i = bullet_nb;
        yield return new WaitForSeconds(delayTime);
        
        _bullets[i].GetComponent<TrailRenderer>().enabled = false;
    }

    Transform GetTarget()
    {
        int id = GetIdTarget();
        if (_targets.Count > 0 && _targets[id] != null)
            return ((GameObject)_targets[id]).transform;
        return null;
    }

    public void RemoveTarget()
    {
        if(_targets.Count > 0)
            _targets.RemoveAt(GetIdTarget());
    }

    private int GetIdTarget()
    {
        int id = 0;

        switch(_id_target)
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
