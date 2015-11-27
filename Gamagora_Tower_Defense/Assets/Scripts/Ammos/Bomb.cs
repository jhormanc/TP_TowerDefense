using UnityEngine;
using System.Collections;

public class Bomb : Ammo
{
    private float _time;
    private bool _explode_delay;

    public Bomb() : base()
    {

    }

    protected override void Awake()
    {
        base.Awake();
        _time = 0f;
       _explode_delay = false;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        _time = 0f;
        _explode_delay = false;
    }

    protected override void Update()
    {
        base.Update();

        if(_explode_delay)
        {
            _time += Time.deltaTime;

            if (_time > TimeToExplode)
                SpawnEffect(transform.position, transform.rotation);
        }
    }

    protected override void OnCollisionEnter(Collision col)
    {
        if(col.gameObject.tag == "Terrain")
        {
            _explode_delay = true;
        }
    }
}
