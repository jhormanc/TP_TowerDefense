using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameManager : Singleton<GameManager>
{
    // Guarantee this will be always a singleton only - can't use the constructor!
    protected GameManager() { } 

    public Spawn SpawnManager1;
    public GameObject Terrain;
    public bool WaveIsOver;
    public bool Win;
    public int HP { get; private set; }
    public int Score { get; private set; }
    public int BestScore { get; private set; }

    private static readonly string BestScoreKey = "BestScore";

    // Weapons
    private bool _placing_weapon;
    private static GameObject[] _weapons_list;
    private ArrayList _weapons;
    private GameObject _temp_weapon;
    bool _weapon_selected;
    private Camera _main_camera; // Tourelle automatique ou manuelle

    // Enemies
    private static GameObject[] _ennemy_list;
    private int _wave;

    // Use this for initialization
    void Awake()
    {
        SpawnManager1 = GetComponent<Spawn>();
        _weapons_list = Resources.LoadAll<GameObject>("Prefabs/Weapons");
        _ennemy_list = Resources.LoadAll<GameObject>("Prefabs/Enemies");
        _wave = 1;
        _weapons = new ArrayList();
        _weapon_selected = false;
        _placing_weapon = false;
        _main_camera = null;
        SpawnManager1.Init(_ennemy_list[0]);
        BestScore = -1;

        InitGame();
    }
	
	// Update is called once per frame
	void Update()
    {
	    if(IsOver())
        {
            if (Win)
            {
                _wave++;
                SpawnManager1.NewWave(_wave);
            }
            else
            {
                transform.FindChild("Canvas").FindChild("ButtonRetry").GetComponent<Button>().interactable = true;
                transform.FindChild("Canvas").FindChild("ButtonGatling").GetComponent<Button>().interactable = false;
                transform.FindChild("Canvas").FindChild("ButtonCamera").GetComponent<Button>().interactable = false;

                if (Score > BestScore)
                    PlayerPrefs.SetInt(BestScoreKey, Score);
            }
        }

        if (_main_camera == null)
        {
            RaycastHit pos = GetMouseRayPos();

            if (_placing_weapon)
            {
                if (pos.collider != null && (pos.collider.tag == "Terrain"))
                {
                    _temp_weapon.transform.position = pos.point;
                }

                if (Input.GetMouseButtonDown(0))
                {
                    //_temp_weapon.GetComponent<BoxCollider>().enabled = true;
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
            else if (Input.GetMouseButtonDown(0))
            {
                if (pos.collider != null)
                {
                    if (pos.collider.tag == "Weapon")
                    {
                        if (pos.collider.gameObject != _temp_weapon)
                            pos.collider.gameObject.transform.FindChild("SelectionWeapon").GetComponent<ParticleSystem>().Emit(1);

                        _temp_weapon = pos.collider.gameObject;
                        _weapon_selected = true;
                        transform.FindChild("Canvas").FindChild("ButtonCamera").GetComponent<Button>().interactable = true;
                    }
                    else if (_weapon_selected && !EventSystem.current.IsPointerOverGameObject())
                    {
                        _temp_weapon = null;
                        _weapon_selected = false;
                        transform.FindChild("Canvas").FindChild("ButtonCamera").GetComponent<Button>().interactable = false;
                    }
                }
            }
        }
	}

    private bool IsOver()
    {
        return SpawnManager1.NbEnemies == 0;
    }

    public void ChangeCamera()
    {
        if (_temp_weapon != null)
        {
            bool auto = _temp_weapon.GetComponent<Weapon>().Auto;
            
            string text_button = auto ? "Retour" : "Zoom";

            if (auto)
                _main_camera = Camera.main;

            _main_camera.enabled = !auto;

            _temp_weapon.transform.FindChild("Camera").GetComponent<Camera>().enabled = auto; // .FindChild("Base").FindChild("Tourelle")

            transform.FindChild("Canvas").FindChild("ButtonCamera").FindChild("Text").GetComponent<Text>().text = text_button;

            _temp_weapon.GetComponent<Weapon>().Auto = !auto;

            if (!auto)
                _main_camera = null;
        }
    }

    public void InitGame()
    {
        WaveIsOver = false;
        Win = false;
        transform.FindChild("Canvas").FindChild("ButtonCamera").GetComponent<Button>().interactable = false;
        if (PlayerPrefs.HasKey(BestScoreKey))
            BestScore = PlayerPrefs.GetInt(BestScoreKey);

        _weapons.Clear();

        transform.FindChild("Canvas").FindChild("ButtonRetry").GetComponent<Button>().interactable = false;
        transform.FindChild("Canvas").FindChild("ButtonGatling").GetComponent<Button>().interactable = true;
        HP = 100;
        Score = 0;
        _wave = 0;

        SpawnManager1.NewWave(_wave);

        RefreshUI();
    }

    private void RefreshUI()
    {
        string best_score = BestScore >= 0 ? string.Format("Meilleur score : {0}", BestScore) : "";
        transform.FindChild("Canvas").FindChild("BestScore").GetComponent<Text>().text = best_score;
        transform.FindChild("Canvas").FindChild("HP").GetComponent<Text>().text = string.Format("HP : {0}", HP);
        transform.FindChild("Canvas").FindChild("Score").GetComponent<Text>().text = string.Format("Score : {0}", Score);
        transform.FindChild("Canvas").FindChild("Wave").GetComponent<Text>().text = string.Format("Vague : {0}", _wave);
    }

    private RaycastHit GetMouseRayPos()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit_info;

        if (Physics.Raycast(ray, out hit_info, 1000f))
        {
            return hit_info;
        }
        
        return default(RaycastHit);
    }

    public void ReceiveDamage(int dmg, GameObject enemy)
    {
        HP -= dmg;

        if (enemy.GetComponent<Little1>() != null)
            SpawnManager1.SetDead(enemy);

        RefreshUI();

        if (HP <= 0)
        {
            Win = false;
            WaveIsOver = true;
        }
    }

    public void SetDead(GameObject enemy)
    {
        Score += enemy.GetComponent<Enemy>().Points;

        RefreshUI();

        if (enemy.GetComponent<Little1>() != null)
            SpawnManager1.SetDead(enemy);

        for(int i = 0; i < _weapons.Count; i++)
        {
            ((GameObject)_weapons[i]).GetComponent<Tourelle>().RemoveTarget(enemy);
        }
    }

    public void NewWeapon(int nb)
    {
        if (_placing_weapon == false)
        {
            _placing_weapon = true;
            RaycastHit pos = GetMouseRayPos();
            _temp_weapon = (GameObject)Instantiate(_weapons_list[nb], pos.point, Quaternion.Euler(0, 0, 0));
            UnityEditor.PrefabUtility.ResetToPrefabState(_temp_weapon);
            //_temp_weapon.GetComponent<BoxCollider>().enabled = false;
            _temp_weapon.GetComponent<SphereCollider>().enabled = false;
            //_temp_weapon.GetComponent<Rigidbody>().isKinematic = true;
            _temp_weapon.GetComponent<Weapon>().enabled = false;
            _temp_weapon.SetActive(true);
        }
    }

    //public void NewWeapon(int nb, Vector3 pos)
    //{
    //    GameObject weapon = (GameObject)Instantiate(_weapons_list[nb], pos, Quaternion.Euler(0, 0, 0));
    //    _weapons.Add(weapon);
    //}
}
