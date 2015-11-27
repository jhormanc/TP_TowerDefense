using UnityEngine;
using System.Collections;

public class ParticleDamage : MonoBehaviour
{
    public GameObject Source;

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
            enemy.ReceiveDamage(Source.GetComponent<Weapon>().CalculateDamage(null, enemy));
        }
    }

    public float GetSpeed()
    {
        return GetComponent<ParticleSystem>().startSpeed;
    }
}
