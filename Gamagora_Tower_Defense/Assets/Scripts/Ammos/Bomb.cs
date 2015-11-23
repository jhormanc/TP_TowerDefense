using UnityEngine;
using System.Collections;

public class Bomb : Ammo
{
    public GameObject Target;

    private static readonly float LerpTime = 10f;
    private float _lerp;

    public Bomb() : base()
    {
        _lerp = 0f;
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

            str *= Target.GetComponent<Enemy>().Speed;

            if (str > 0)
                str *= Vector3.Distance(transform.position, Target.transform.position) / 10f;
            else
                str *= 1f;


            GetComponent<Rigidbody>().AddForce(transform.forward * str, ForceMode.Force);

            // On vise plus loin dans la direction où avance l'ennemi
            //pos = pos + Target.transform.forward * Target.GetComponent<Enemy>().Speed * Vector3.Distance(transform.position, pos);

            //// Direction de la bombe vers l'ennemi
            //Vector3 direction = (pos - transform.position).normalized;
            //Vector3 pt = transform.position + direction;

            //if (transform.position != pt)
            //{
            //    _lerp += Time.deltaTime / LerpTime;
            //    transform.position = Vector3.Lerp(transform.position, pt, _lerp);
            //}
            //else
            //    _lerp = 0f;
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
