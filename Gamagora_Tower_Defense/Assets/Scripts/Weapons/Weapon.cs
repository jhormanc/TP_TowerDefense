using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Pathfinding;

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

    protected GameManager Manager;

    // Values that will be set in the Inspector
    public float Range;
    public GameObject Bullet;
    public float FireRate;
    public float BulletSpeed;
    public float Degats;
    public int BulletsSize; // Nombre de bullets dans le pull
    public bool Auto; // Visée auto ou manuelle
    public AimType Aim;
    public int Level { get; private set; } // Niveaux de la tourelle
    public int LevelUpPrice; // Prix pour lvl up
    public int Price; // Prix d'achat
    public int NodesSize; // Nombre de nodes dans le pull
    public GameObject Node; // Prefab d'un node
    public int MaxWeaponsInTheSameTime;
    public int Id; // Ordre dans le dossier Prefab/Weapons

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

    // Nodes
    protected PullManager _nodes;

    // Use this for initialization
    protected virtual void Awake()
    {
        _direction = new Vector3();
        _angleRotation = 0f;
        _fire = false;
        _allowFire = true;
        Auto = true;
        Level = 1;
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

        if (Node != null && NodesSize > 0)
        {
            _nodes = ScriptableObject.CreateInstance<PullManager>();
            _nodes.Init(Node, NodesSize);
        }
        else
            _nodes = null;

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
        if (_targets.Count > 0 && ((GameObject)_targets[0]).GetComponent<Enemy>().IsTargetable() == false)
            SortTargets();
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
        if (!Auto)
            return true;

        Transform tourelle = transform.FindChild("Base").FindChild("Tourelle");
        Transform head = tourelle.FindChild("Head");

        return _targets.Count > 0 && ((GameObject)_targets[0]).GetComponent<Enemy>().IsTargetable() && Quaternion.Angle(head != null ? head.rotation : tourelle.rotation, _lookRotation) < 5f;
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

        if (_bullets != null)
        {
            bullet = _bullets.GetNextObj();
            bullet.SetActive(true);
        }

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
            bullet.GetComponent<Ammo>().Source = gameObject;
            bullet.SetActive(true);
            bullet.transform.position = shoot_point.position;
            bullet.transform.rotation = shoot_point.rotation;
            bullet.transform.forward = shoot_point.forward;
            bullet.GetComponent<Rigidbody>().velocity = shoot_point.forward;
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

    public virtual float CalculateDamage(GameObject bullet, Enemy enemy, bool explode = false)
    {
        float ret = 1f;

        if (bullet != null)
            ret = !explode ? bullet.GetComponent<Ammo>().Degats : bullet.GetComponent<Ammo>().DegatsExplode;

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

    protected virtual void LvlUp()
    {
        Degats *= 1.2f;
        Range *= 1.2f;
        Level++;
        Manager.SetColor(gameObject);
    }

    protected virtual void EmitParticle(bool emit)
    {
        Transform p = transform.FindChild("Base").FindChild("Tourelle").FindChild("Particle");
        p.GetComponent<ParticleSystem>().enableEmission = emit;
        p.FindChild("Smoke").GetComponent<ParticleSystem>().enableEmission = emit;
        p.FindChild("Sparks").GetComponent<ParticleSystem>().enableEmission = emit;
    }

    public Transform GetTarget()
    {
        Transform target = null;
        int i = 0;
        List<GameObject> dead_enemies = new List<GameObject>();

        if (_targets.Count > 0 && _targets[i] != null)
        {
            GameObject enemy = (GameObject)_targets[i];

            while (enemy != null && enemy.GetComponent<Enemy>().Dead)
            {
                dead_enemies.Add(enemy);
                i++;
                enemy = _targets.Count > i ? (GameObject)_targets[i] : null;
            }

            foreach(GameObject e in dead_enemies)
            {
                RemoveTarget(e);
            }

            if (enemy != null)
                target = enemy.transform;
        }

        return target;
    }

    public Color GetColor()
    {
        float i = Level - 1;
        float red, green, blue;

        if (Level < 10)
        {
            red = 0f;
            green = 0.8f - i * 0.1f;
            blue = i * 0.1f;
        }
        else if (Level < 19)
        {
            i = i - 10;
            red = i * 0.1f;
            green = 0f;
            blue = 0.8f - i * 0.1f;
        }
        else if(Level < 29)
        {
            i = i - 20;
            red = 0.8f - i * 0.1f;
            green = 0f;
            blue = 0f;
        }
        else
        {
            red = 0f;
            green = 0f;
            blue = 0f;
        }

        return new Color(red, green, blue);
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

    public void ShowNodes(List<GraphNode> nodes, bool show, Color color = default(Color))
    {
        _nodes.RemoveAll();
        if (show)
        {
            foreach (GraphNode n in nodes)
            {
                GameObject node = _nodes.GetNextObj();
                node.SetActive(true);
                node.transform.position = (Vector3)n.position;
                node.transform.FindChild("Base").GetComponent<Renderer>().material.SetColor("_Color", color == default(Color) ? Color.grey : color);

            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, Range);
    }
}
