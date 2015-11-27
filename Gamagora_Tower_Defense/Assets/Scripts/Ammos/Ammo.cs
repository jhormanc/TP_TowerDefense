using UnityEngine;
using System.Collections;

public class Ammo : MonoBehaviour
{
    public float Degats;
    public GameObject Source;
    public bool RayShoot;
    public GameObject HitEffect;
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
    }

    public virtual void SpawnEffect(Vector3 pos, Quaternion rot)
    {
        if (_hit_effect != null)
        {
            _hit_effect.transform.position = pos;
            _hit_effect.transform.rotation = rot;
            _hit_effect.GetComponent<ParticleSystem>().Play(true);
            if (ExplosionRadius > 0f)
                Explode();
        }
        transform.FindChild("Base").gameObject.SetActive(false);
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
                enemy.ReceiveDamage(Source.GetComponent<Weapon>().CalculateDamage(gameObject, enemy, true));
            }
        }
    }
}
