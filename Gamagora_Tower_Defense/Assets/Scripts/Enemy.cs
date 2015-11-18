using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public float HP;
    public float Speed;

    // Déplacements
    private bool _move;
    private Transform _target;
    private Vector3 _last_pos;

    // Use this for initialization
    void Awake()
    {
        _move = true;
        _target = GameObject.Find("EndPoint").transform;
        transform.FindChild("Flash").GetComponent<ParticleSystem>().enableEmission = false;
        _last_pos = transform.position;
    }
	
	// Update is called once per frame
	void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
            _move = !_move;

        if(_move)
            Move();
    }

    public void ReceiveDamage(float damage, Tourelle source)
    {
        HP -= damage;

        if (HP < 0f)
        {
            if(source != null)
                source.RemoveTarget();
            Die();
        }
    }

    public bool IsDead()
    {
        return HP < 0f;
    }

    private void Die()
    {
        transform.FindChild("Particle").GetComponent<ParticleSystem>().enableEmission = false;
        transform.FindChild("Particle").GetComponent<ParticleSystem>().Stop();
        transform.FindChild("Flash").GetComponent<ParticleSystem>().enableEmission = true;
        transform.FindChild("Flash").GetComponent<ParticleSystem>().Emit(10);
        Destroy(gameObject, 0.1f);
    }

    private void Move()
    {
        float step = Speed * Time.deltaTime;

        transform.position = Vector3.MoveTowards(transform.position, _target.position, step);

        //if (Input.GetKey(KeyCode.Z))
        //    transform.position += new Vector3(DeltaPos, 0, 0);
        //else if (Input.GetKey(KeyCode.S))
        //    transform.position -= new Vector3(DeltaPos, 0, 0);

        //if (Input.GetKey(KeyCode.D))
        //    transform.position += new Vector3(0, 0, DeltaPos);
        //else if (Input.GetKey(KeyCode.Q))
        //    transform.position -= new Vector3(0, 0, DeltaPos);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(_last_pos, transform.position);
    }
}
