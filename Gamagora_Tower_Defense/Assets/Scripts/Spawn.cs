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

    // Use this for initialization
    void Awake()
    {
        Instantiate<GameObject>(StartPoint);
        Instantiate<GameObject>(EndPoint);
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
        for (int i = 0; i < SpawnSize; i++)
        {
            GameObject e = _spawn.GetNextObj();
            e.GetComponent<Enemy>().Target = EndPoint;
            NbEnemies++;

            yield return new WaitForSeconds(SpawnDelay);
        }
    }
}
