using UnityEngine;
using System.Collections;

public class Bomb : Ammo
{
    public Bomb() : base()
    {

    }

    protected override void Awake()
    {
        base.Awake();


    }

    protected void OnTriggerEnter(Collider other)
    {
        
    }
}
