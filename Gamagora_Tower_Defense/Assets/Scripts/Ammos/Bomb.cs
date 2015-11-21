using UnityEngine;
using System.Collections;

public class Bomb : Ammo
{
    public Bomb() : base()
    {

    }

    protected void OnTriggerEnter(Collider other)
    {
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().AddForce(Vector3.zero);
        GetComponent<Rigidbody>().isKinematic = true;
        transform.FindChild("Particle").GetComponent<ParticleSystem>().Stop(true);
        transform.FindChild("Particle").GetComponent<ParticleSystem>().Play(true);
        transform.FindChild("Base").gameObject.SetActive(false);
    }

    void OnParticleCollision(GameObject other)
    {
        Rigidbody body = other.GetComponent<Rigidbody>();
        if (body && body.tag == "Enemy")
        {
            Enemy enemy = body.gameObject.GetComponent<Enemy>();
            float degats = transform.parent.gameObject.GetComponent<Weapon>().CalculateDamage(gameObject, enemy);
            enemy.ReceiveDamage(degats);
        }
    }
}
