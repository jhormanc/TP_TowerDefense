using UnityEngine;
using System.Collections;

public class Ammo : MonoBehaviour
{
    public float Degats;
    public GameObject Source;
    public bool RayShoot;
    public GameObject HitEffect;
    // Time
    public float DelayAfterHit;
    // Projectiles explosifs
    public float ExplosionRadius;
    public float TimeToExplode;
    public float DegatsExplode;

    protected GameObject _hit_effect;
    
    // Use this for initialization
    protected virtual void Awake()
    {
        _hit_effect = Instantiate(HitEffect);
    }
	
	// Update is called once per frame
	protected virtual void Update()
    {
	
	}

    protected virtual void OnEnable()
    {
        transform.FindChild("Base").gameObject.SetActive(true);
        GetComponent<Collider>().enabled = true;
    }

    protected virtual void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag != "UI" && col.gameObject.tag != "TerrainCollider")
            Disable();
    }

    protected virtual void Disable()
    {
        SpawnEffect(transform.position, transform.rotation);
    }

    public virtual void SpawnEffect(Vector3 pos, Quaternion rot, bool stop = true)
    {
        if (_hit_effect != null && !_hit_effect.GetComponent<ParticleSystem>().isPlaying)
        {
            _hit_effect.transform.position = pos;
            _hit_effect.transform.rotation = rot;
            _hit_effect.GetComponent<ParticleSystem>().Play(true);
            if (ExplosionRadius > 0f)
                Explode();
        }
        GetComponent<Collider>().enabled = false;
        transform.FindChild("Base").gameObject.SetActive(false);

        if(stop && gameObject.activeSelf)
            StartCoroutine(Stop());
    }

    IEnumerator Stop()
    {
        yield return new WaitForSeconds(DelayAfterHit);

        if (_hit_effect != null)
            _hit_effect.GetComponent<ParticleSystem>().Stop(true);
        gameObject.SetActive(false);
    }

    protected virtual void Explode()
    {
        RaycastHit[] hits = Physics.SphereCastAll(new Ray(_hit_effect.transform.position, _hit_effect.transform.forward), ExplosionRadius);

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];

            if(hit.collider.tag == "Enemy")
            {
                Enemy enemy = hit.collider.gameObject.GetComponent<Enemy>();
                float force = (hit.distance != 0f ? 1f / hit.distance : 10f) * ExplosionRadius * DegatsExplode;
                enemy.GetComponent<Rigidbody>().AddExplosionForce(force, transform.position, ExplosionRadius); 
                enemy.ReceiveDamage(Source.GetComponent<Weapon>().CalculateDamage(gameObject, enemy, true));
            }
        }
    }
}
