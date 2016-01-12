using UnityEngine;
using System.Collections;

public class EnemyWatcher : MonoBehaviour
{
    private Weapon source;
    private SphereCollider coll;

    // Use this for initialization
    void Awake ()
    {
        
    }
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    public void Init(Weapon src, float range)
    {
        coll = GetComponent<SphereCollider>();
        source = src;
        coll.radius = range * 0.5f;
    }

    void OnTriggerEnter(Collider other)
    {
        print(other.gameObject.name);
        if (other.tag == "Enemy")
            source.AddEnemy(other.transform.gameObject);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Enemy")
            source.RemoveEnemy(other.transform.gameObject);
    }
}