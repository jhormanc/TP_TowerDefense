using UnityEngine;
using System.Collections;
using Pathfinding;

public class Enemy : MonoBehaviour
{
    public float HP;
    public bool Dead { get; private set; }
    public float Speed;
    public int Degats;
    public int Points;
    public GameObject Target;
    public GameObject Origin;
    public float TimeBeforeExplode;

    // Vie
    protected float _health;
    protected bool _dead;

    // Déplacements
    protected bool _move;
    //protected Vector3 _last_pos;
    protected Quaternion _lookRotation;

    // IA
    //The max distance from the AI to a waypoint for it to continue to the next waypoint
    public float nextWaypointDistance = 3f;
    public Path Path; // The calculated path
    private Seeker _seeker;
    private int _waypoint;

    protected GameManager Manager;

    public Enemy()
    {

    }

    // Use this for initialization
    void Awake()
    {
        _move = true;
        //transform.FindChild("Flash").GetComponent<ParticleSystem>().enableEmission = false;
        //_last_pos = transform.position;
        _health = HP;
        Dead = false;
        Manager = GameManager.Instance;
        _waypoint = 0;
        _seeker = GetComponent<Seeker>();
    }

    void OnEnable()
    {
        
    }

    public void Init(GameObject source, GameObject target)
    {
        Origin = source;
        Target = target;
        GetComponent<Rigidbody>().isKinematic = true;
        transform.FindChild("Flash").GetComponent<ParticleSystem>().Stop();
        transform.FindChild("Particle").gameObject.SetActive(true);
        transform.FindChild("Particle").GetComponent<ParticleSystem>().Play();
        if (Origin != null)
            transform.position = Origin.transform.position;
        _health = HP;
        Dead = false;

        AstarPath.OnGraphsUpdated += RecalculatePath;
        _seeker.pathCallback += OnPathComplete;
        _seeker.StartPath(transform.position, Target.transform.position);
    }

    public void OnDisable()
    {
        _seeker.pathCallback -= OnPathComplete;
        AstarPath.OnGraphsUpdated -= RecalculatePath;
    }

    public void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            Path = p;
            //Reset the waypoint counter
            _waypoint = 0;
        }
        else
            Debug.Log("Pathfinding Error : " + p.errorLog);
    }

    public void RecalculatePath(AstarPath script)
    {
        _seeker.StartPath(transform.position, Target.transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        if (_health < 0f)
        {
            Die();
        }
        else
        {
            _move = true;

            if (_move)
                Move();
        }
    }

    public void ReceiveDamage(float damage)
    {
        _health -= damage;
    }

    public bool IsDead()
    {
        return _health < 0f;
    }

    protected void Die()
    {
        if (!Dead)
        {
            Manager.SetDead(gameObject);
            Dead = true;
            StartCoroutine(StartDead());
        }
    }

    protected IEnumerator StartDead()
    {
        GetComponent<Rigidbody>().isKinematic = false;
        yield return new WaitForSeconds(TimeBeforeExplode);

        transform.FindChild("Particle").GetComponent<ParticleSystem>().Stop();
        transform.FindChild("Particle").gameObject.SetActive(false);
        transform.FindChild("Flash").GetComponent<ParticleSystem>().Play();
        yield return new WaitForSeconds(0.3f);
        Manager.RemoveEnemy(gameObject);
    }

    protected void Move()
    {
        if (Path == null)
        {
            // We have no path to move after yet
            return;
        }
        if (_waypoint >= Path.vectorPath.Count)
        {
            if (Vector3.Distance(transform.position, Target.transform.position) <= nextWaypointDistance)
            {
                Manager.ReceiveDamage(Degats, gameObject);
            }
            return;
        }

        // Direction to the next waypoint
        Vector3 dir = (Path.vectorPath[_waypoint] - transform.position).normalized;
        float speed = Speed * Time.deltaTime;

        _lookRotation = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, _lookRotation, speed);

        float f = Time.realtimeSinceStartup * 4f * Speed;
        Vector3 delta_h = 0.2f * Vector3.up * Mathf.Cos(f);

        transform.position = Vector3.Lerp(transform.position, transform.position + dir + Vector3.up * 0.5f + delta_h, speed);

        // Check if we are close enough to the next waypoint
        // If we are, proceed to follow the next waypoint
        if (Vector3.Distance(transform.position, Path.vectorPath[_waypoint]) < nextWaypointDistance)
        {
            _waypoint++;
            return;
        }
    }

    public float GetDistFromTarget()
    {
        return Vector3.Distance(transform.position, Target.transform.position);
    }

    public float GetStrength()
    {
        return _health * Degats;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Bullet")
        {
            GameObject bullet = collision.gameObject;
            if (!bullet.GetComponent<Ammo>().RayShoot)
            {
                ReceiveDamage(bullet.GetComponent<Ammo>().Source.GetComponent<Weapon>().CalculateDamage(bullet, this));
                bullet.GetComponent<Ammo>().SpawnEffect(collision.contacts[0].point, collision.transform.rotation);
            }
            bullet.SetActive(false);
        }
    }

    ////void OnParticleCollision(GameObject other)
    ////{

    ////}

    //void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawLine(_last_pos, transform.position);
    //}
}
