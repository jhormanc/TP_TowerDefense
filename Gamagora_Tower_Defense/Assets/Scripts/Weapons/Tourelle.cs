using UnityEngine;
using System.Collections;

public class Tourelle : MonoBehaviour
{
    // Values that will be set in the Inspector
    public float RotationSpeed;
    public float BulletSpeed;
    public float Range;
    public GameObject Bullet;
    
    // Values for internal use
    private static readonly float DeltaRot = 0.3f;
    private static readonly int BulletsSize = 100;

    // Visée
    private Quaternion _lookRotation;
    private Vector3 _direction;
    private float _angleRotation;
    private bool _auto;

    // Shoot
    private bool _fire, _lastFire;

    // Bullets
    private GameObject[] _bullets;
    private int _bullet_nb;

    // Ennemis
    private ArrayList _targets;
    private int _id_target;

    // Use this for initialization
    void Start()
    {
        _direction = new Vector3();
        _angleRotation = 0F;
        _fire = false;
        _auto = true;
        _bullets = new GameObject[BulletsSize];
        _bullet_nb = 0;
        _targets = new ArrayList();
        _id_target = 0;
        this.GetComponent<SphereCollider>().radius = Range;

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
        if (Input.GetKeyDown(KeyCode.A))
            _auto = !_auto;

        if (_targets.Count == 0)
        {
            _fire = false;
            EmitParticle(_fire);
            AudioSource audio = GetComponent<AudioSource>();
            audio.Stop();
        }

        Move();



        if (_fire)
            Fire();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Enemy")
        {
            _targets.Add(other.transform);
            _fire = true;
            EmitParticle(true);
            AudioSource audio = GetComponent<AudioSource>();
            audio.Play();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Enemy")
            _targets.Remove(other.transform);
    }

    void Fire()
    {
        Transform canon = transform.FindChild("Base").FindChild("Tourelle").FindChild("Canon");
        Transform shoot_point = canon.FindChild("Shoot");
        GameObject bullet = _bullets[_bullet_nb];

        bullet.GetComponent<Bullet>().Source = this;
        bullet.transform.position = shoot_point.position;
        bullet.transform.rotation = shoot_point.rotation;
        bullet.SetActive(true);
        bullet.GetComponent<Rigidbody>().velocity = shoot_point.forward * BulletSpeed;
        bullet.GetComponent<TrailRenderer>().enabled = true;
        StartCoroutine(DisableBulletEffect(_bullet_nb, 0.5f));

        canon.Rotate(0, 0, 20);

        if (_bullet_nb < BulletsSize - 1)
            _bullet_nb++;
        else
            _bullet_nb = 0;
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
                if(_auto)
                    tourelle.rotation = _lookRotation;
                else
                    Quaternion.Slerp(tourelle.rotation, _lookRotation, Time.deltaTime * RotationSpeed);
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
            return (Transform)_targets[id];
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

}
