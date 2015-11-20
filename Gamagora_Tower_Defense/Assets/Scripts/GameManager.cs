using UnityEngine;
using System.Collections;

public class GameManager : Singleton<GameManager>
{
    // Guarantee this will be always a singleton only - can't use the constructor!
    protected GameManager() { } 

    public Spawn SpawnManager;

    private int _wave;

	// Use this for initialization
	void Awake()
    {
        SpawnManager = GetComponent<Spawn>();
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

    void InitGame()
    {
        SpawnManager.NewWave(_wave);
    }
}
