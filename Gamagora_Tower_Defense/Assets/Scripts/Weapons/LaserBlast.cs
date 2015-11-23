using UnityEngine;
using System.Collections;

public class LaserBlast : Weapon
{
    public LaserBlast() : base()
    {

    }

    protected override void Awake()
    {
        Transform head = transform.FindChild("Base").FindChild("Tourelle").FindChild("Head");
        base.Awake();
    }

    protected override void Fire()
    {
        Transform shoot = transform.FindChild("Base").FindChild("Tourelle").FindChild("Head").FindChild("Cannon");
        GameObject bullet = _bullets[_bullet_nb];
        bullet.transform.FindChild("Particle").GetComponent<ParticleSystem>().enableEmission = false;
        bullet.transform.FindChild("Particle").GetComponent<ParticleSystem>().Stop(true);
        bullet.GetComponent<Rigidbody>().isKinematic = false;
        bullet.GetComponent<BoxCollider>().enabled = true;
        bullet.GetComponent<Bomb>().Target = GetTarget().gameObject;
        StartCoroutine(Fire(shoot, bullet, false));
    }

    protected override void Move()
    {
        Transform tourelle = transform.FindChild("Base").FindChild("Tourelle");
        Transform head = tourelle.FindChild("Head");
        Transform target = GetTarget();
        Vector3 target_pos = Vector3.zero;

        if (target != null)
        {


            target_pos = target.position;
            float dist = Vector3.Distance(head.position, target_pos);
            float h = (dist * dist / 15f);
            // Angle de vision max vers le haut
            if (h > 15f)
                h = 15f;
            
            //target_pos = target.position - head.position;
            target_pos +=  dist // (target.position - head.position).magnitude
                             * target.forward
                             * target.GetComponent<Enemy>().Speed
                             / (BulletSpeed * 0.02f);

            //Vector3 l = target.forward * target.GetComponent<Enemy>().Speed * dist / 5f;

            // Change bullet speed
            BulletSpeed = 300f + dist * 17f;

            // Change target y position
            target_pos = target_pos + new Vector3(0f, h, 0f);
            Debug.DrawLine(target_pos, target_pos + target.forward);
        }

        Move(tourelle, head, null, target_pos);
        
    }

    //protected override void Move(Transform tourelle, Transform head = null, Transform look_point = null, Vector3 target = default(Vector3))
    //{
    //    if(_auto)
    //    {
    //        if(target != Vector3.zero)
    //        {
    //            _direction = target;
    //            _lookRotation = Quaternion.LookRotation(_direction);

    //            // Rotate us over time according to speed until we are in the required rotation
    //            float speed = 3f;

    //            if (Quaternion.Angle(tourelle.rotation, _lookRotation) < 5f)
    //                speed *= 5f;

    //            tourelle.rotation = Quaternion.Slerp(tourelle.rotation, _lookRotation, Time.deltaTime * speed);

    //            if (head != null)
    //            {
    //                tourelle.rotation = Quaternion.Euler(new Vector3(0f, tourelle.rotation.eulerAngles.y, 0f));
    //                head.rotation = Quaternion.Slerp(head.rotation, _lookRotation, Time.deltaTime * speed);
    //            }
    //        }
            
    //    }
    //    else
    //    {
    //        if (Input.GetKey(KeyCode.Z) && _angleRotation < 90f)
    //        {
    //            _angleRotation += DeltaRot * RotationSpeed;

    //            if (head != null)
    //                head.Rotate(new Vector3(DeltaRot * RotationSpeed, 0, 0));
    //            else
    //                tourelle.Rotate(new Vector3(DeltaRot * RotationSpeed, 0, 0));
    //        }
    //        else if (Input.GetKey(KeyCode.S) && _angleRotation > -45f)
    //        {
    //            _angleRotation -= DeltaRot * RotationSpeed;

    //            if (head != null)
    //                head.Rotate(new Vector3(-DeltaRot * RotationSpeed, 0, 0));
    //            else
    //                tourelle.Rotate(new Vector3(DeltaRot * RotationSpeed, 0, 0));
    //        }

    //        if (Input.GetKey(KeyCode.D))
    //            tourelle.RotateAround(tourelle.position, new Vector3(0, 1, 0), DeltaRot * RotationSpeed);
    //        else if (Input.GetKey(KeyCode.Q))
    //            tourelle.RotateAround(tourelle.position, new Vector3(0, 1, 0), -DeltaRot * RotationSpeed);
    //    }
    //}

    protected override void EmitParticle(bool emit)
    {
        Transform p = transform.FindChild("Base").FindChild("Tourelle").FindChild("Head").FindChild("Cannon").FindChild("Particle");

        p.GetComponent<ParticleSystem>().enableEmission = emit;

        if (emit)
            p.GetComponent<ParticleSystem>().Emit(1);
    }
}
