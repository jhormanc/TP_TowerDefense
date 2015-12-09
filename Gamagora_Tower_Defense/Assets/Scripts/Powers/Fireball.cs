using UnityEngine;
using System.Collections;

public class Fireball : PowerDamage
{
    protected override void OnParticleCollision(GameObject other)
    {
        base.OnParticleCollision(other);

        RaycastHit[] hits = Physics.SphereCastAll(new Ray(transform.position, transform.forward), ExplosionRadius);
        PlayExplosionSound();

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];

            if (hit.collider.tag == "Enemy")
            {
                Enemy enemy = hit.collider.gameObject.GetComponent<Enemy>();
                Rigidbody rb = enemy.GetComponent<Rigidbody>();
                float force = ExplosionRadius * Damage;

                enemy.ReceiveDamage(Damage);
                rb.AddExplosionForce(force, transform.position, ExplosionRadius * 10f, 0f, ForceMode.Impulse);
            }
        }
    }

    protected void PlayExplosionSound()
    {
        Hashtable param = new Hashtable();
        param.Add("position", transform.position);
        param.Add("spatialBlend", 0.5f);
        param.Add("volume", 1f);
        int key = SoundManager.Instance.PlayAudio(Audio_Type.ExplosionBomb, param);
        SoundManager.Instance.Fade(key, 1f, 0f);
    }
}
