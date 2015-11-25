using UnityEngine;
using System.Collections;

public class Ammo : MonoBehaviour
{
    public float Degats;
    public GameObject Source;
    public bool RayShoot;
    public GameObject HitEffect;
    public float DelayAfterHit;

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

    public virtual void SpawnEffect(Vector3 pos, Quaternion rot)
    {
        _hit_effect.transform.position = pos;
        _hit_effect.transform.rotation = rot;
        _hit_effect.GetComponent<ParticleSystem>().Play(true);
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
}
