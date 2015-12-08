using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundManager : Singleton<SoundManager>
{
    private Dictionary<string, AudioData> _data;

    // Use this for initialization
    void Awake()
    {
        _data = new Dictionary<string, AudioData>();
    }
	
	// Update is called once per frame
	void Update()
    {
	
	}
}
