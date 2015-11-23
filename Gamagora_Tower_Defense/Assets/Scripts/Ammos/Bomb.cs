using UnityEngine;
using System.Collections;

public class Bomb : Ammo
{
    public GameObject Target;

    public Bomb() : base()
    {

    }

    protected override void Update()
    {
        base.Update();

        if (Target != null)
        { 
            // Position de l'ennemi
            Vector3 pos = Target.transform.position;

            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Vector3 toOther = Target.transform.TransformDirection(Vector3.forward);

            float str = Vector3.Dot(forward, toOther);
            Vector3 target_pos = Target.transform.position;
            target_pos.y = transform.position.y;
            float dist = Vector3.Distance(transform.position, target_pos);
            float min_target = Target.GetComponent<SphereCollider>().radius;
            float target_speed = Target.GetComponent<Enemy>().Speed;

            // TODO régler
            if (str > 0)
                str *= target_speed * 0.5f * dist / 7f;
            else
                str *= dist > 5f ? dist / 20f : dist * target_speed;

            GetComponent<Rigidbody>().AddForce(transform.forward * str, ForceMode.Force);
        }
    }

    protected void OnTriggerEnter(Collider other)
    {
        Transform p = transform.FindChild("Particle");
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().AddForce(Vector3.zero);
        GetComponent<Rigidbody>().isKinematic = true;
        p.GetComponent<ParticleSystem>().enableEmission = true;
        p.GetComponent<ParticleSystem>().Play(true);

        transform.FindChild("Base").gameObject.SetActive(false);
        transform.GetComponent<BoxCollider>().enabled = false;
        Target = null;
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
