using UnityEngine;
using System.Collections;

public class Spawn : MonoBehaviour
{
    public GameObject StartPoint;
    public GameObject EndPoint;
    public float SpawnDelay;
    public bool WaveIsOver;
    public bool Win;

    private static readonly int SpawnSize = 10;
    private static GameObject[] _ennemy_list;
    private ArrayList _spawn;

	// Use this for initialization
	void Awake()
    {
        Instantiate<GameObject>(StartPoint);
        Instantiate<GameObject>(EndPoint);
        transform.position = StartPoint.transform.position;
        WaveIsOver = false;
        Win = false;
        _spawn = new ArrayList();
        _ennemy_list = Resources.LoadAll<GameObject>("Prefabs/Enemies");
        for (int i = 0; i < SpawnSize; i++)
        {
            _spawn.Add((GameObject)Instantiate(_ennemy_list[0], transform.position, transform.rotation));
            ((GameObject)_spawn[i]).SetActive(false);
        }
    }
	
	// Update is called once per frame
	void Update()
    {

	}

    void FixedUpdate()
    {

    }

    public void NewWave(int wave)
    {
        WaveIsOver = false;
        Win = false;
        StartCoroutine(SpawnEnnemis());
    }

    public void SetDead(GameObject enemy)
    {
        _spawn.Remove(enemy);
    }

    IEnumerator SpawnEnnemis()
    {
        for (int i = 0; i < _spawn.Count; i++)
        {
            GameObject e = (GameObject)_spawn[i];
            UnityEditor.PrefabUtility.ResetToPrefabState(e);
            e.GetComponent<Enemy>().Target = EndPoint;
            e.SetActive(true);

            yield return new WaitForSeconds(SpawnDelay);
        }
    }
}
