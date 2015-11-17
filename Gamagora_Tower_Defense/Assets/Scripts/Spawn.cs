using UnityEngine;
using System.Collections;

public class Spawn : MonoBehaviour
{
    public static GameObject[] _ennemy_list;

	// Use this for initialization
	void Start()
    {
        _ennemy_list = Resources.LoadAll<GameObject>("Prefabs/Enemies");
    }
	
	// Update is called once per frame
	void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            Instantiate((GameObject)_ennemy_list[0], transform.position, transform.rotation);

	}
}
