using UnityEngine;
using System.Collections;

public class Bullet : Ammo
{
    public Bullet() : base()
    {

    }

    protected void OnTriggerEnter(Collider other)
    {
        GetComponent<TrailRenderer>().enabled = false;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        GetComponent<TrailRenderer>().enabled = true;
    }

    protected void OnDisable()
    {
        GetComponent<TrailRenderer>().enabled = false;
    }
}
