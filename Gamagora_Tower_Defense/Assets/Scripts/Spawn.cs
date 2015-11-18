using UnityEngine;
using System.Collections;

public class Spawn : MonoBehaviour
{
    public static GameObject[] _ennemy_list;
    public float SpawnDelay;

    private static readonly int SpawnSize = 10;
    private ArrayList _spawn;
    private int _id_spawn;

	// Use this for initialization
	void Awake()
    {
        _ennemy_list = Resources.LoadAll<GameObject>("Prefabs/Enemies");
        _id_spawn = 0;
        _spawn = new ArrayList();

        for (int i = 0; i < SpawnSize; i++)
        {
            _spawn.Add(Instantiate(_ennemy_list[0], transform.position, transform.rotation));
            ((GameObject)_spawn[i]).SetActive(false);
        }
        StartCoroutine(SpawnEnnemis());

    }
	
	// Update is called once per frame
	void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            Instantiate((GameObject)_ennemy_list[0], transform.position, transform.rotation);
	}

    IEnumerator SpawnEnnemis()
    {
        for (int i = 0; i < SpawnSize; i++)
        {
            GameObject e = ((GameObject)_spawn[i]);
            UnityEditor.PrefabUtility.ResetToPrefabState(e);
            e.SetActive(true);

            yield return new WaitForSeconds(SpawnDelay);
        }
    }
}
