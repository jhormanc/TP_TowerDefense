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

    void OnTriggerEnter(Collider other)
    {
        print(other.tag);
    }
}
