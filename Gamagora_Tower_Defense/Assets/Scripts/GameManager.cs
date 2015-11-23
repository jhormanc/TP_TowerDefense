using UnityEngine;
using System.Collections;

public class GameManager : Singleton<GameManager>
{
    // Guarantee this will be always a singleton only - can't use the constructor!
    protected GameManager() { } 

    public Spawn SpawnManager;

    public GameObject Terrain;

    private int _wave;

    // Weapons
    private bool _placing_weapon;

    private static GameObject[] _weapons_list;
    private ArrayList _weapons;
    private GameObject _temp_weapon;

    // Use this for initialization
    void Awake()
    {
        SpawnManager = GetComponent<Spawn>();
        _weapons_list = Resources.LoadAll<GameObject>("Prefabs/Weapons");
        _weapons = new ArrayList();
        _wave = 1;
        _placing_weapon = false;
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

        if(_placing_weapon)
        {
            Vector3 pos = GetMouseRayPos();
  
            if(pos != Vector3.zero)
                _temp_weapon.transform.position = pos;

            if (Input.GetMouseButtonDown(0))
            {
                _temp_weapon.GetComponent<BoxCollider>().enabled = true;
                _temp_weapon.GetComponent<SphereCollider>().enabled = true;
                _temp_weapon.GetComponent<Rigidbody>().isKinematic = false;
                _temp_weapon.GetComponent<Weapon>().enabled = true;
                _weapons.Add(_temp_weapon);
                _placing_weapon = false;
            }
            else if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                _placing_weapon = false;
                Destroy(_temp_weapon);
            }
        }
	}

    private void InitGame()
    {
        //NewWeapon(3, new Vector3(18.86f, 0f, 31.49f));
        SpawnManager.NewWave(_wave);
    }

    private Vector3 GetMouseRayPos()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit_info;

        if (Physics.Raycast(ray, out hit_info, 1000f))
        {
            print(hit_info.collider.tag);
            if(hit_info.collider.tag == "Terrain")
                return hit_info.point;
        }

        return Vector3.zero;
    }

    public void SetDead(GameObject enemy)
    {
        SpawnManager.SetDead(enemy);
        for(int i = 0; i < _weapons.Count; i++)
        {
            ((GameObject)_weapons[i]).GetComponent<Tourelle>().RemoveTarget(enemy);
        }
    }

    public void NewWeapon(int nb)
    {
        if (_placing_weapon == false)
        {
            Vector3 pos = GetMouseRayPos();
            _temp_weapon = (GameObject)Instantiate(_weapons_list[nb], pos, Quaternion.Euler(0, 0, 0));
            UnityEditor.PrefabUtility.ResetToPrefabState(_temp_weapon);
            _temp_weapon.GetComponent<BoxCollider>().enabled = false;
            _temp_weapon.GetComponent<SphereCollider>().enabled = false;
            _temp_weapon.GetComponent<Rigidbody>().isKinematic = true;
            _temp_weapon.GetComponent<Weapon>().enabled = false;
            _temp_weapon.SetActive(true);
            _placing_weapon = true;
        }
    }

    //public void NewWeapon(int nb, Vector3 pos)
    //{
    //    GameObject weapon = (GameObject)Instantiate(_weapons_list[nb], pos, Quaternion.Euler(0, 0, 0));
    //    _weapons.Add(weapon);
    //}
}
