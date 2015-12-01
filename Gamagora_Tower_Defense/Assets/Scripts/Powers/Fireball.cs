using UnityEngine;
using System.Collections;

public class Fireball : PowerDamage
{
    protected override void OnParticleCollision(GameObject other)
    {
        base.OnParticleCollision(other);

        RaycastHit[] hits = Physics.SphereCastAll(new Ray(transform.position, transform.forward), ExplosionRadius);

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];

            if (hit.collider.tag == "Enemy")
            {
                Enemy enemy = hit.collider.gameObject.GetComponent<Enemy>();
                Rigidbody rb = enemy.GetComponent<Rigidbody>();
                float m = (hit.transform.position - transform.position).magnitude;
                float force = (m > 0.2f ? 1f / m : 5f) * ExplosionRadius * Damage;

                enemy.ReceiveDamage(Damage);
                rb.AddExplosionForce(force * 500f, transform.position, ExplosionRadius * 10f, 1f, ForceMode.Force);
            }
        }
    }

}
