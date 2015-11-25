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
}
