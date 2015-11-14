using UnityEngine;
using System.Collections;

public class Tourelle : MonoBehaviour {

    //values that will be set in the Inspector
    public Transform Target;
    public float RotationSpeed;
    public float BulletSpeed;
    public Rigidbody Bullet;

    //values for internal use
    private Quaternion _lookRotation;
    private Vector3 _direction;

    private Vector3 _aim;
    private bool _fire;

    // Use this for initialization
    void Start () {
        _aim = new Vector3();
        _fire = true;
    }
	
	// Update is called once per frame
	void Update () {
        if (_fire)
        {
            Transform tourelle = transform.FindChild("Base").FindChild("Tourelle");

            //if (Input.GetKey(KeyCode.Z) && tourelle.rotation.x < -0.25F)
            //    _aim.x += 5;
            //else if (Input.GetKey(KeyCode.S) && tourelle.rotation.x > -0.9F)
            //    _aim.x -= 5;
            //else if (Input.GetKey(KeyCode.D))
            //    _aim.z += 5;
            //else if (Input.GetKey(KeyCode.Q))
            //    _aim.z -= 5;

            //tourelle.localRotation = Quaternion.Euler(_aim);
            //tourelle.Rotate(rot);

            //find the vector pointing from our position to the target
            _direction = (Target.position - tourelle.position).normalized;

            //create the rotation we need to be in to look at the target
            _lookRotation = Quaternion.LookRotation(_direction);

            //rotate us over time according to speed until we are in the required rotation
            tourelle.rotation = Quaternion.Slerp(tourelle.rotation, _lookRotation, Time.deltaTime * RotationSpeed);
    
            tourelle.FindChild("Canon").Rotate(0, 0, 20);

            Transform shoot_point = tourelle.FindChild("Canon").FindChild("Shoot");
            Rigidbody bullet = (Rigidbody)Instantiate(Bullet, shoot_point.position, shoot_point.rotation);
            bullet.velocity = shoot_point.forward * BulletSpeed;
        }
    }
}
