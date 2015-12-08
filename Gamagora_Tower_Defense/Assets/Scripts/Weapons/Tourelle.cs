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

    public override Audio_Type GetNewWeaponAudioType()
    {
        return Audio_Type.NewGatling;
    }

    protected override void PlayFireSound(bool stop = false)
    {
        if (_key_shoot_sound >= 0 && stop)
        {
            _soundManager.stop(_key_shoot_sound);
            _key_shoot_sound = -1;
        }
        else if(_key_shoot_sound == -1 && !stop)
        {
            Hashtable param = new Hashtable();
            param.Add("position", _tourelle.position);
            //param.Add("pitch", FireRate / 1.6f);
            param.Add("pitch", FireRate / 7f);
            //param.Add("delayedtime", 1f / FireRate);
            param.Add("loop", true);
            param.Add("spatialBlend", 0.5f);
            param.Add("reverbZoneMix", Random.Range(0f, 200f));
            _key_shoot_sound = _soundManager.PlayAudio(Audio_Type.GatlingShoot, param);
        }
    }
}
