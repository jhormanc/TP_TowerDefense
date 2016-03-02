using UnityEngine;
using System.Collections;
using Pathfinding;
using UnityEngine.UI;

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
    public GameObject Explosion;
    public int IdEnemy;

    // Vie
    protected float _health;
    protected bool _dead;

    // Déplacements
    protected bool _move;
    private bool _targetable;
    protected Quaternion _lookRotation;
    protected Rigidbody _rb;

    // IA
    //The max distance from the AI to a waypoint for it to continue to the next waypoint
    public float nextWaypointDistance;
    public Path Path; // The calculated path
    private Seeker _seeker;
    private int _waypoint;

    private GameObject _explosion;

    protected GameManager Manager;
    protected ParticleSystem _flash;
    protected ParticleSystem _particles;

    private SoundManager _soundManager;

    // UI
    private Transform _canvas;
    private Text _health_text;
    private Vector3 canvas_pos;

    public Enemy()
    {

    }

    // Use this for initialization
    protected virtual void Awake()
    {
        _soundManager = SoundManager.Instance;
        _move = true;
        _health = HP;
        Dead = false;
        Manager = GameManager.Instance;
        _waypoint = 0;
        _seeker = GetComponent<Seeker>();
        if (Explosion != null)
            _explosion = (GameObject)Instantiate(Explosion, transform.position, transform.rotation);

        _rb = GetComponent<Rigidbody>();
        _flash = transform.FindChild("Flash").GetComponent<ParticleSystem>();
        _particles = transform.FindChild("Particle").GetComponent<ParticleSystem>();
        _canvas = transform.FindChild("Canvas");
        _health_text = _canvas.FindChild("Life").GetComponent<Text>();
        canvas_pos = _canvas.localPosition;
    }

    protected virtual void OnEnable()
    {
        
    }

    public void Init(GameObject source, GameObject target)
    {
        Origin = source;
        Target = target;
 
        GetComponent<Rigidbody>().useGravity = false;
        _flash.Stop();
        _particles.gameObject.SetActive(true);
        _particles.Play();

        if (Origin != null)
            _rb.MovePosition(Origin.transform.position);

        _health = HP;
        _targetable = false;
        _move = true;
        Dead = false;
        _health_text.text = _health.ToString();
        _health_text.color = new Color(0f, 180f, 0f);

        AstarPath.OnGraphsUpdated += RecalculatePath;
        _seeker.pathCallback += OnPathComplete;
        _seeker.StartPath(GetComponent<Rigidbody>().position, Target.transform.position);
    }

    protected virtual void OnDisable()
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
        _seeker.StartPath(GetComponent<Rigidbody>().position, Target.transform.position);
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (IsDead())
        {
            Die();
        }
        else
        {
            _canvas.localPosition = canvas_pos;
            if (Camera.main != null)
                _canvas.LookAt(Camera.main.transform);
            if (_move)
                Move();
        }
    }

    public virtual void ReceiveDamage(float damage)
    {
        _health -= damage;

        if (_health < 0f)
            _health = 0f;

        if (_health > 0f)
        {
            _health_text.text = _health.ToString();

            if (_health > HP * 0.66f)
                _health_text.color = new Color(0f, 180f, 0f);
            else if (_health > HP * 0.33f)
                _health_text.color = new Color(180f, 180f, 0f);
            else
                _health_text.color = new Color(180f, 0f, 0f);
        }
        else
            _health_text.text = string.Empty;
    }

    public bool IsDead()
    {
        return _health <= 0f;
    }

    protected void Die()
    {
        if (!Dead)
        {
            Manager.SetDead(gameObject);
            _health_text.text = string.Empty;
            Dead = true;
            _move = false;
            _rb.velocity = Vector3.zero;
            StartCoroutine(StartDead());
        }
    }

    protected IEnumerator StartDead()
    {
        GetComponent<Rigidbody>().useGravity = true;
        yield return new WaitForSeconds(TimeBeforeExplode);
        Hashtable param = new Hashtable();
        param.Add("position", transform.position);
        //param.Add("pitch", 2f);
        param.Add("volume", 0.1f);
        int key = _soundManager.PlayAudio(Audio_Type.EnemyExplosion, param);
        _soundManager.Fade(key, 1f, 0f);
        _particles.Stop();
        _particles.gameObject.SetActive(false);
        _flash.Play();
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
            if (Vector3.Distance(_rb.position, Target.transform.position) <= nextWaypointDistance)
            {
                Explode();
            }
            return;
        }

        Vector3 target = Path.vectorPath[_waypoint];

        // Check if we are close enough to the next waypoint
        // If we are, proceed to follow the next waypoint
        if (Vector3.Distance(_rb.position, target) < nextWaypointDistance)
        {
            _waypoint++;
            return;
        }

        //float f = Time.realtimeSinceStartup * 4f * Speed;
        //Vector3 delta_h = 0.2f * Vector3.up * Mathf.Cos(f);

        target = target + Vector3.up * 1.5f;// + delta_h;
        // Direction to the next waypoint
        Vector3 dir = (target - _rb.position).normalized;
        float speed = Speed * Time.deltaTime;

        _lookRotation = Quaternion.LookRotation(dir);

        _rb.MoveRotation(_lookRotation);
        _rb.velocity = 50f * dir * speed;
    }

    protected virtual void Explode()
    {
        if (_explosion != null)
        {
            _particles.Stop(true);
            _particles.gameObject.SetActive(false);
            _explosion.transform.position = _rb.position;
            _explosion.transform.rotation = _rb.rotation;
            _explosion.GetComponent<ParticleSystem>().Play(true);
        }

        Hashtable param = new Hashtable();
        param.Add("position", transform.position);
        param.Add("spatialBlend", 0.5f);
        param.Add("volume", 1f);
        int key = _soundManager.PlayAudio(Audio_Type.ExplosionBomb, param);
        _soundManager.Fade(key, 1f, 0f);

        Manager.ReceiveDamage(Degats, gameObject);
        _rb.velocity = Vector3.zero;
        _health_text.text = string.Empty;
        _move = false;
    }

    public bool IsTargetable()
    {
        return _targetable;
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

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "TerrainColliderStart")
            _targetable = true;
        else if (other.gameObject.tag == "TerrainColliderEnd")
            _targetable = false;
    }

    public void Freeze(float time)
    {
        _move = false;
        _rb.velocity = Vector3.zero;
        StartCoroutine(WaitEndFreeze(time));
    }

    private IEnumerator WaitEndFreeze(float time)
    {
        yield return new WaitForSeconds(time);
        _move = true;
    }
}
