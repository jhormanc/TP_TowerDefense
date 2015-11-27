using UnityEngine;
using System.Collections;

public class Spawn : MonoBehaviour
{
    public GameObject StartPoint;
    public GameObject EndPoint;
    public float SpawnDelay;

    public int NbEnemies { get; private set; }

    private int SpawnSize = 10;
    private PullManager _spawn;
    private GameObject _start, _end;

    // Use this for initialization
    void Awake()
    {
        _start =  (GameObject)Instantiate(StartPoint, StartPoint.transform.position, StartPoint.transform.rotation);
        _end = (GameObject)Instantiate(EndPoint, EndPoint.transform.position, EndPoint.transform.rotation);
        transform.position = StartPoint.transform.position;
        _spawn = null;
        NbEnemies = 0;
    }

    public void Init(GameObject enemy)
    {
        if (enemy != null)
        {
            _spawn = ScriptableObject.CreateInstance<PullManager>();
            _spawn.Init(enemy, SpawnSize);
        }
    }

    public void NewWave(int wave)
    {
        StartCoroutine(SpawnEnnemis(wave));
    }

    public void SetDead(GameObject enemy)
    {
        _spawn.RemoveObj(enemy);
        NbEnemies--;
    }

    private IEnumerator SpawnEnnemis(int wave)
    {
        NbEnemies = SpawnSize;
        for (int i = 0; i < SpawnSize; i++)
        {
            GameObject e = _spawn.GetNextObj();
            e.GetComponent<Enemy>().Target = _end;
            e.GetComponent<Enemy>().Origin = _start;
            e.SetActive(true);

            yield return new WaitForSeconds(SpawnDelay);
        }
    }
}
