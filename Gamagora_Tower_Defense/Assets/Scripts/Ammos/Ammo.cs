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

    protected Transform _base;
    protected SoundManager _soundManager;
    
    // Use this for initialization
    protected virtual void Awake()
    {
        _soundManager = SoundManager.Instance;
        _hit_effect = (GameObject)Instantiate(HitEffect, transform.position, transform.rotation);
        _base = transform.FindChild("Base");
    }
	
	// Update is called once per frame
	protected virtual void Update()
    {
	
	}

    protected virtual void OnEnable()
    {
        _base.gameObject.SetActive(true);
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
            _hit_effect.SetActive(true);
            _hit_effect.transform.position = pos;
            _hit_effect.transform.rotation = rot;
            _hit_effect.GetComponent<ParticleSystem>().Play(true);
            if (ExplosionRadius > 0f)
                Explode();
        }
        GetComponent<Collider>().enabled = false;
        _base.gameObject.SetActive(false);

        if(stop && gameObject.activeSelf)
            Invoke("Stop", DelayAfterHit);
    }

    void Stop()
    {
        if (_hit_effect != null)
        {
            _hit_effect.GetComponent<ParticleSystem>().Stop(true);
            _hit_effect.SetActive(false);
        }
            
        gameObject.SetActive(false);
    }

    protected virtual void Explode()
    {
        RaycastHit[] hits = Physics.SphereCastAll(new Ray(_hit_effect.transform.position, _hit_effect.transform.forward), ExplosionRadius);
        PlayExplosionSound();
        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];

            if(hit.collider.tag == "Enemy")
            {
                Enemy enemy = hit.collider.gameObject.GetComponent<Enemy>();
                float force = DegatsExplode;

                enemy.GetComponent<Rigidbody>().AddExplosionForce(force, transform.position, ExplosionRadius, 0f, ForceMode.Impulse); 
                enemy.ReceiveDamage(Source.GetComponent<Weapon>().CalculateDamage(gameObject, enemy, true));
            }
        }
    }

    protected virtual void PlayExplosionSound()
    {
        // à implémenter dans les classes filles
    }
}
