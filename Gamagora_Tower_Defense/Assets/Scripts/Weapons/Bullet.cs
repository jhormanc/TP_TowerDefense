using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour
{
    public float Degats;
    public Tourelle Source = null;

	// Use this for initialization
	void Awake()
    {
	
	}
	
	// Update is called once per frame
	void Update()
    {
	
	}

    //void OnTriggerEnter(Collider other)
    //{
    //    if (other.tag == "Enemy")
    //    {
    //        Enemy target = other.GetComponent<Enemy>();
    //        if (target != null)
    //            target.ReceiveDamage(Degats, Source);
    //        gameObject.SetActive(false);
    //    }
    //}
}
