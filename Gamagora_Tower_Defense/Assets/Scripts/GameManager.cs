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

    // Player
    public bool WaveIsOver;
    public bool Win;
    public int HP { get; private set; }
    public int Score { get; private set; }
    public int BestScore { get; private set; }
    public int Money { get; private set; }
    public int BaseMoney;

    private bool _waveIsStarted;
    private static readonly string BestScoreKey = "BestScore";

    // Weapons
    private bool _placing_weapon;
    private static GameObject[] _weapons_list;
    private ArrayList _weapons; // TODO Weapons PullManager
    private GameObject _temp_weapon;
    bool _weapon_selected;
    private Camera _main_camera; // Tourelle automatique ou manuelle

    // Enemies
    public int InitialEnemySize;
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
        _waveIsStarted = false;
        _main_camera = null;
        SpawnManager1.Init(_ennemy_list[0], InitialEnemySize);
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

                _waveIsStarted = false;
            }

            RefreshUI();
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
                    _temp_weapon.GetComponent<SphereCollider>().enabled = true;
                    _temp_weapon.GetComponent<Rigidbody>().isKinematic = false;
                    _temp_weapon.GetComponent<Weapon>().enabled = true;
                    _weapons.Add(_temp_weapon);
                    Money -= _temp_weapon.GetComponent<Weapon>().Price;
                    RefreshUI();
                    _placing_weapon = false;
                    _temp_weapon = null;
                }
                else if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
                {
                    _placing_weapon = false;
                    Destroy(_temp_weapon);
                    _temp_weapon = null;
                }
            }
            else if (Input.GetMouseButtonDown(0))
            {
                if (pos.collider.tag == "Weapon")
                {
                    GameObject weapon = pos.collider.gameObject;
                    if (weapon != _temp_weapon)
                    {
                        if (_temp_weapon != null)
                            SetColor(_temp_weapon);

                        _temp_weapon = weapon;
                        _temp_weapon.transform.FindChild("Base").FindChild("Temperature").GetComponent<Renderer>().material.SetColor("_Color", Color.white);
                        _temp_weapon.transform.FindChild("Base").FindChild("Temperature").GetComponent<Renderer>().material.SetFloat("_Emission", 2f);
                        _temp_weapon.transform.FindChild("Base").FindChild("Temperature").GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.white * _temp_weapon.GetComponent<Weapon>().GetSelectedIntensity());
                        _weapon_selected = true;
                        RefreshUI();
                    }
                }
                else if (_weapon_selected && !EventSystem.current.IsPointerOverGameObject())
                {
                    SetColor(_temp_weapon);
                    _temp_weapon = null;
                    _weapon_selected = false;
                    RefreshUI();
                }
            }
        }
	}

    public void SetColor(GameObject weapon)
    {
        weapon.transform.FindChild("Base").FindChild("Temperature").GetComponent<Renderer>().material.SetColor("_Color", _temp_weapon.GetComponent<Weapon>().GetColor());
        weapon.transform.FindChild("Base").FindChild("Temperature").GetComponent<Renderer>().material.SetFloat("_Emission", 2f);
        weapon.transform.FindChild("Base").FindChild("Temperature").GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.black);
    }

    private bool IsOver()
    {
        return  _waveIsStarted && SpawnManager1.NbEnemies == 0 && SpawnManager1.SpawnFinished;
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

            _temp_weapon.transform.FindChild("Camera").GetComponent<Camera>().enabled = auto;

            transform.FindChild("Canvas").FindChild("ButtonCamera").FindChild("Text").GetComponent<Text>().text = text_button;

            _temp_weapon.GetComponent<Weapon>().Auto = !auto;

            if (!auto)
                _main_camera = null;
        }
    }

    public void InitGame()
    {
        Money = BaseMoney;
        WaveIsOver = false;
        Win = true;
        transform.FindChild("Canvas").FindChild("ButtonCamera").GetComponent<Button>().interactable = false;
        if (PlayerPrefs.HasKey(BestScoreKey))
            BestScore = PlayerPrefs.GetInt(BestScoreKey);

        _weapons.Clear();

        transform.FindChild("Canvas").FindChild("ButtonRetry").GetComponent<Button>().interactable = false;

        HP = 100;
        Score = 0;
        _wave = 0;

        SpawnManager1.NewWave(_wave);
        _waveIsStarted = true;

        RefreshUI();
    }

    private void RefreshUI()
    {
        string best_score = BestScore >= 0 ? string.Format("Meilleur score : {0}", BestScore) : "";
        transform.FindChild("Canvas").FindChild("Money").GetComponent<Text>().text = string.Format("Argent : {0}", Money);
        transform.FindChild("Canvas").FindChild("BestScore").GetComponent<Text>().text = best_score;
        transform.FindChild("Canvas").FindChild("HP").GetComponent<Text>().text = string.Format("HP : {0}", HP);
        transform.FindChild("Canvas").FindChild("Score").GetComponent<Text>().text = string.Format("Score : {0}", Score);
        transform.FindChild("Canvas").FindChild("Wave").GetComponent<Text>().text = string.Format("Vague : {0}", _wave);

        transform.FindChild("Canvas").FindChild("ButtonGatling").GetComponent<Button>().interactable = Money >= _weapons_list[1].GetComponent<Weapon>().Price;
        transform.FindChild("Canvas").FindChild("ButtonShotgun").GetComponent<Button>().interactable = Money >= _weapons_list[5].GetComponent<Weapon>().Price;
        transform.FindChild("Canvas").FindChild("ButtonFlamethrower").GetComponent<Button>().interactable = Money >= _weapons_list[0].GetComponent<Weapon>().Price;
        transform.FindChild("Canvas").FindChild("ButtonLaserBlast").GetComponent<Button>().interactable = Money >= _weapons_list[3].GetComponent<Weapon>().Price;

        transform.FindChild("Canvas").FindChild("ButtonCamera").GetComponent<Button>().interactable = _weapon_selected;
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

        if (HP <= 0)
        {
            Win = false;
            WaveIsOver = true;
        }

        RefreshUI();
    }

    public void SetDead(GameObject enemy)
    {
        Score += enemy.GetComponent<Enemy>().Points;
        Money += enemy.GetComponent<Enemy>().Points;

        RefreshUI();

        if (enemy.GetComponent<Little1>() != null)
            SpawnManager1.SetDead(enemy);

        for(int i = 0; i < _weapons.Count; i++)
        {
            ((GameObject)_weapons[i]).GetComponent<Weapon>().RemoveTarget(enemy);
        }
    }

    public void NewWeapon(int nb)
    {
        if (_placing_weapon == false && Money >=_weapons_list[nb].GetComponent<Weapon>().Price)
        {
            _placing_weapon = true;
            RaycastHit pos = GetMouseRayPos();
            if (_temp_weapon != null)
                SetColor(_temp_weapon);
            _temp_weapon = (GameObject)Instantiate(_weapons_list[nb], pos.point, Quaternion.Euler(0, 0, 0));
            _temp_weapon.transform.FindChild("Base").FindChild("Temperature").GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
            UnityEditor.PrefabUtility.ResetToPrefabState(_temp_weapon);
            _temp_weapon.GetComponent<SphereCollider>().enabled = false;
            _temp_weapon.GetComponent<Weapon>().enabled = false;
            _temp_weapon.SetActive(true);
        }
    }
}
