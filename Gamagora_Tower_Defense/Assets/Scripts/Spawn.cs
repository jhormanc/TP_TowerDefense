using UnityEngine;
using System.Collections;

public class Spawn : MonoBehaviour
{
    public GameObject StartPoint;
    public GameObject EndPoint;
    public static GameObject[] _ennemy_list;
    public float SpawnDelay;
    public bool WaveIsOver;
    public bool Win;

    private static readonly int SpawnSize = 10;
    private ArrayList _spawn;
    private int _id_spawn;

	// Use this for initialization
	void Awake()
    {
        Instantiate<GameObject>(StartPoint);
        Instantiate<GameObject>(EndPoint);
        transform.position = StartPoint.transform.position;
        WaveIsOver = false;
        Win = false;
        _id_spawn = 0;
        _spawn = new ArrayList();
        _ennemy_list = Resources.LoadAll<GameObject>("Prefabs/Enemies");
        for (int i = 0; i < SpawnSize; i++)
        {
            _spawn.Add((Enemy)Instantiate(_ennemy_list[0].GetComponent<Enemy>(), transform.position, transform.rotation));
            ((Enemy)_spawn[i]).gameObject.SetActive(false);
        }
    }
	
	// Update is called once per frame
	void Update()
    {

	}

    public void NewWave(int wave)
    {
        WaveIsOver = false;
        Win = false;
        StartCoroutine(SpawnEnnemis());
    }

    IEnumerator SpawnEnnemis()
    {
        for (int i = 0; i < _spawn.Count; i++)
        {
            Enemy e = (Enemy)_spawn[i];
            UnityEditor.PrefabUtility.ResetToPrefabState(e);
            e.Target = EndPoint;
            e.gameObject.SetActive(true);

            yield return new WaitForSeconds(SpawnDelay);
        }
    }
}
