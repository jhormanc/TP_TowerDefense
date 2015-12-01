using UnityEngine;
using System.Collections;

public class Freeze : PowerDamage
{
    public float FreezeTime;

    protected void OnEnable()
    {
        RaycastHit[] hits = Physics.SphereCastAll(new Ray(transform.position, transform.forward), ExplosionRadius);

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];

            if (hit.collider.tag == "Enemy")
            {
                Enemy enemy = hit.collider.gameObject.GetComponent<Enemy>();
                float m = (hit.transform.position - transform.position).magnitude;
                enemy.ReceiveDamage(Damage);
                enemy.Freeze(FreezeTime);
            }
        }
    }
}
