using UnityEngine;
using System.Collections;

public class Bullet : Ammo
{
    public GameObject Target;

    public Bullet() : base()
    {

    }

    protected override void Update()
    {
        if(Target != null)
        {
            if(Vector3.Distance(transform.position, Target.transform.position) < 1.5f)
            {
                transform.FindChild("Particle").GetComponent<ParticleSystem>().Emit(10);
                StartCoroutine(Stop());
            }
        }
    }

    IEnumerator Stop()
    {
        yield return new WaitForSeconds(0.2f);
        transform.GetComponent<TrailRenderer>().enabled = false;
        gameObject.SetActive(false);
    }
}
