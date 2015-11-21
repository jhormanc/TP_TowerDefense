using UnityEngine;
using System.Collections;

public class GameManager : Singleton<GameManager>
{
    // Guarantee this will be always a singleton only - can't use the constructor!
    protected GameManager() { } 

    public Spawn SpawnManager;

    private static GameObject[] _weapons_list;
    private ArrayList _weapons;

    private int _wave;

	// Use this for initialization
	void Awake()
    {
        SpawnManager = GetComponent<Spawn>();
        _weapons_list = Resources.LoadAll<GameObject>("Prefabs/Weapons");
        _weapons = new ArrayList();
        _wave = 1;
        InitGame();
    }
	
	// Update is called once per frame
	void Update()
    {
	    if(SpawnManager.WaveIsOver)
        {
            if (SpawnManager.Win)
            {
                _wave++;
                SpawnManager.NewWave(_wave);
            }
        }
	}

    private void InitGame()
    {
        NewWeapon(3, new Vector3(18.86f, 0f, 31.49f));
        SpawnManager.NewWave(_wave);
    }

    public void SetDead(GameObject enemy)
    {
        SpawnManager.SetDead(enemy);
        for(int i = 0; i < _weapons.Count; i++)
        {
            ((GameObject)_weapons[i]).GetComponent<Tourelle>().RemoveTarget(enemy);
        }
    }

    public void NewWeapon(int nb, Vector3 pos)
    {
        GameObject weapon = (GameObject)Instantiate(_weapons_list[nb], pos, Quaternion.Euler(0, 0, 0));
        _weapons.Add(weapon);
    }
}
