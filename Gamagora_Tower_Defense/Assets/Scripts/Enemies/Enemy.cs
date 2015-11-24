using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public float HP;
    public float Speed;
    public float Degats;
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

        if (Input.GetKeyDown(KeyCode.A))
            _move = !_move;

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
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(_last_pos, transform.position);
    }
}
