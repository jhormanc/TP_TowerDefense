using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public float HP;
    public float Speed;
    public int Degats;
    public int Points;
    public GameObject Target;
    public GameObject Origin;

    // Vie
    protected float _health;

    // Déplacements
    protected bool _move;
    protected Vector3 _last_pos;
    protected Quaternion _lookRotation;

    protected GameManager Manager;

    public Enemy()
    {

    }

    // Use this for initialization
    void Awake()
    {
        _move = true;
        //transform.FindChild("Flash").GetComponent<ParticleSystem>().enableEmission = false;
        _last_pos = transform.position;
        _health = HP;

        Manager = GameManager.Instance;
    }

    void OnEnable()
    {
        _health = HP;
        transform.FindChild("Flash").GetComponent<ParticleSystem>().Stop();
        transform.FindChild("Particle").gameObject.SetActive(true);
        transform.FindChild("Particle").GetComponent<ParticleSystem>().Play();
        if (Origin != null)
            transform.position = Origin.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (_health < 0f)
        {
            StartCoroutine(Die());
        }

        _move = true;

        if(_move)
            Move();
    }

    public void ReceiveDamage(float damage)
    {
        _health -= damage;
    }

    public bool IsDead()
    {
        return _health < 0f;
    }

    protected IEnumerator Die()
    {
        transform.FindChild("Particle").GetComponent<ParticleSystem>().Stop();
        transform.FindChild("Particle").gameObject.SetActive(false);
        transform.FindChild("Flash").GetComponent<ParticleSystem>().Play();
        yield return new WaitForSeconds(0.3f);
        Manager.SetDead(gameObject);
    }

    protected void Move()
    {
        if(Target != null)
        {
            float step = Speed * Time.deltaTime;

            Vector3 direction = (Target.transform.position - transform.position).normalized;

            _lookRotation = Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(transform.rotation, _lookRotation, step);
            transform.position = Vector3.MoveTowards(transform.position, Target.transform.position, step);

            if(Vector3.Distance(transform.position, Target.transform.position) <= 1f)
            {
                Manager.ReceiveDamage(Degats, gameObject);
            }
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

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(_last_pos, transform.position);
    }
}
