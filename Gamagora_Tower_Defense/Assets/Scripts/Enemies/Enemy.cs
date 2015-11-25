using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public float HP;
    public float Speed;
    public int Degats;
    public int Points;
    public GameObject Target;

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
        Target = null;
        _move = true;
        transform.FindChild("Flash").GetComponent<ParticleSystem>().enableEmission = false;
        _last_pos = transform.position;

        Manager = GameManager.Instance;
    }
	
	// Update is called once per frame
	void Update()
    {
        if (HP < 0f)
        {
            StartCoroutine(Die());
        }

        _move = true;

        if(_move)
            Move();
    }

    public void ReceiveDamage(float damage)
    {
        HP -= damage;
    }

    public bool IsDead()
    {
        return HP < 0f;
    }

    protected IEnumerator Die()
    {
        transform.FindChild("Particle").GetComponent<ParticleSystem>().enableEmission = false;
        transform.FindChild("Particle").GetComponent<ParticleSystem>().Stop();
        transform.FindChild("Flash").GetComponent<ParticleSystem>().enableEmission = true;
        transform.FindChild("Flash").GetComponent<ParticleSystem>().Emit(10);
        yield return new WaitForSeconds(0.25f);
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
        return HP * Degats;
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

    void OnParticleCollision(GameObject other)
    {
        // TODO Test Collisions
        print(other.tag);
        //if (other && other.tag == "Bullet")
        //{
        //    Enemy enemy = other.gameObject.GetComponent<Enemy>();
        //    float degats = transform.parent.gameObject.GetComponent<Weapon>().CalculateDamage(gameObject, enemy);
        //    enemy.ReceiveDamage(degats);
        //    print(degats);
        //}
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(_last_pos, transform.position);
    }
}
