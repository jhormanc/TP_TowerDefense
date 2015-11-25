using UnityEngine;
using System.Collections;

public class Ammo : MonoBehaviour
{
    public float Degats;
    public GameObject Source;
    public bool RayShoot;
    public GameObject HitEffect;

    private GameObject _hit_effect;

    // Use this for initialization
    protected virtual void Awake()
    {
	
	}
	
	// Update is called once per frame
	protected virtual void Update()
    {
	
	}

    public virtual void SpawnEffect(Vector3 pos, Quaternion rot)
    {
        _hit_effect = (GameObject)Instantiate(HitEffect, pos, rot);
        _hit_effect.GetComponent<ParticleSystem>().Play(true);

        StartCoroutine(Stop());
    }

    IEnumerator Stop()
    {
        yield return new WaitForSeconds(0.5f);

        if (_hit_effect != null)
            Destroy(_hit_effect);
        gameObject.SetActive(false);
    }
}
