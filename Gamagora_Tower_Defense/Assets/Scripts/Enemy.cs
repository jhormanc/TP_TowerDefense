using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public float HP;
    public float Speed;

    // Déplacements
    private bool _move;
    private Transform _target;

    // Use this for initialization
    void Start()
    {
        _move = false;
        _target = GameObject.Find("EndPoint").transform;
        transform.FindChild("Flash").GetComponent<ParticleSystem>().enableEmission = false;
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
        transform.FindChild("Flash").GetComponent<ParticleSystem>().enableEmission = true;
        transform.FindChild("Flash").GetComponent<ParticleSystem>().Emit(1);
        DestroyObject(gameObject, 0.5f);
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
}
