using UnityEngine;
using System.Collections;

public class ParticleDamage : MonoBehaviour
{
    public GameObject Source;
    public GameObject Bullet;

	// Use this for initialization
	void Awake()
    {

	}
	
	// Update is called once per frame
	void Update()
    {
	
	}

    void OnParticleCollision(GameObject other)
    {
        if(other.tag == "Enemy")
        {
            Enemy enemy = other.gameObject.GetComponent<Enemy>();
            enemy.ReceiveDamage(Source.GetComponent<Weapon>().CalculateDamage(Bullet, enemy));
        }
    }

    public float GetSpeed()
    {
        return GetComponent<ParticleSystem>().startSpeed;
    }
}
