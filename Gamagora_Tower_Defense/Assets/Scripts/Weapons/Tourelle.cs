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

        _cannon.localRotation = Quaternion.Slerp(_cannon.localRotation, _canonRotation, Time.deltaTime * FireRate);

    }

    protected override void Fire()
    {
        GameObject bullet = null;

        if (_bullets != null)
        {
            bullet = _bullets.GetNextObj();
            bullet.GetComponent<TrailRenderer>().enabled = true;
            bullet.SetActive(true);
        }

        StartCoroutine(Fire(_shoot, bullet, true));
        StartCoroutine(DisableBulletEffect(bullet, 2f));

        _canonRotation = _cannon.localRotation * Quaternion.Euler(0f, 0f, 45f);
    }

    protected IEnumerator DisableBulletEffect(GameObject bullet, float delayTime)
    {
        yield return new WaitForSeconds(delayTime);

        if(bullet != null)
            bullet.GetComponent<TrailRenderer>().enabled = false;
    }

    protected override void LvlUp()
    {
        FireRate *= 1.5f;
        base.LvlUp();
    }
}
