using UnityEngine;
using System.Collections;

public class Tourelle : Weapon
{
    private Quaternion _canonRotation;

    public Tourelle() : base()
    {

    }

    protected override void Update()
    {
        base.Update();

        Transform canon = transform.FindChild("Base").FindChild("Tourelle").FindChild("Cannon");

        canon.localRotation = Quaternion.Slerp(canon.localRotation, _canonRotation, Time.deltaTime * FireRate);

    }

    protected override void Fire()
    {
        Transform canon = transform.FindChild("Base").FindChild("Tourelle").FindChild("Cannon");
        GameObject bullet = null;

        if (_bullets != null)
        {
            bullet = _bullets.GetNextObj();
            bullet.GetComponent<TrailRenderer>().enabled = true;
        }

        StartCoroutine(Fire(canon.FindChild("Shoot"), bullet, true));
        StartCoroutine(DisableBulletEffect(bullet, 0.5f));

        _canonRotation = canon.localRotation * Quaternion.Euler(0f, 0f, 45f);
    }

    protected IEnumerator DisableBulletEffect(GameObject bullet, float delayTime)
    {
        yield return new WaitForSeconds(delayTime);

        if(bullet != null)
            bullet.GetComponent<TrailRenderer>().enabled = false;
    }
}
