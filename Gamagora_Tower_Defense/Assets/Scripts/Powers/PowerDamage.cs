using UnityEngine;
using System.Collections;

public class PowerDamage : MonoBehaviour
{
    public float Damage;
    public float ExplosionRadius;

    // Use this for initialization
    void Awake()
    {
	
	}
	
	// Update is called once per frame
	void Update()
    {
	
	}

    protected virtual void OnParticleCollision(GameObject other)
    {
        if (other.tag == "Enemy")
        {
            Enemy enemy = other.gameObject.GetComponent<Enemy>();
            enemy.ReceiveDamage(Damage);
        }
    }
}
