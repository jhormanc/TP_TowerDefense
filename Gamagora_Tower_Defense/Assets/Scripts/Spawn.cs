using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Spawn : MonoBehaviour
{
    public GameObject StartPoint;
    public GameObject EndPoint;
    public float SpawnDelay;

    public int NbEnemies { get; private set; }
    public bool SpawnFinished { get; private set; }

    public int SpawnSize { get; private set; }
    private List<PullManager> _spawns;
    private GameObject _start, _end;

    // Use this for initialization
    void Awake()
    {
        _start =  (GameObject)Instantiate(StartPoint, StartPoint.transform.position, StartPoint.transform.rotation);
        _end = (GameObject)Instantiate(EndPoint, EndPoint.transform.position, EndPoint.transform.rotation);
        transform.position = StartPoint.transform.position;
        SpawnFinished = false;
        NbEnemies = 0;
        _spawns = new List<PullManager>();
    }

    public void Init(GameObject[] enemies, int size)
    {
        SpawnSize = size;
        foreach(GameObject e in enemies)
        {
            PullManager spawn = ScriptableObject.CreateInstance<PullManager>();
            spawn.Init(e, SpawnSize);
            _spawns.Add(spawn);
        }
    }

    public void NewWave(int wave)
    {
        SpawnFinished = false;
        StartCoroutine(SpawnEnnemis(wave));
    }

    public void SetDead(GameObject enemy)
    {
        int id = enemy.GetComponent<Enemy>().Id;

        _spawns[id].RemoveObj(enemy);
        NbEnemies--;
    }

    private IEnumerator SpawnEnnemis(int wave)
    {
        NbEnemies = SpawnSize;

        GameObject o = _spawns[1].GetNextObj();
        o.SetActive(true);
        o.GetComponent<Enemy>().Init(_start, _end);

        yield return new WaitForSeconds(SpawnDelay);

        for (int i = 0; i < SpawnSize - 1; i++)
        {
            GameObject e = _spawns[0].GetNextObj();
            e.SetActive(true);
            e.GetComponent<Enemy>().Init(_start, _end);
            
            yield return new WaitForSeconds(SpawnDelay);
        }

        SpawnFinished = true;
    }
}
