using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Pathfinding;
using System.Collections.Generic;

public class GameManager : Singleton<GameManager>
{
    // Guarantee this will be always a singleton only - can't use the constructor!
    protected GameManager() { }

    private static readonly string BestScoreKey = "BestScore";

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

    // Pouvoirs
    public GameObject Fireball;
    public GameObject Freeze;
    public GameObject PowerSelection;
    private GameObject _powerSelection;
    private List<PullManager> _powers;
    private int _num_power;

    // Weapons
    public GameObject NewWeaponEffect;
    public GameObject AStar;
    private bool _placing_weapon;
    private bool _new_weapon;
    private static GameObject[] _weapons_list;
    private List<PullManager> _weapons;
    private GameObject _temp_weapon;
    private bool _weapon_selected;
    private Camera _main_camera; // Tourelle automatique ou manuelle
    private GameObject _newWeaponEffect;

    // Enemies
    public Spawn SpawnManager1;
    public int InitialEnemySize;
    private static GameObject[] _ennemy_list;
    private int _wave;

    // UI
    private Button _bt_gatling;
    private Button _bt_flamethrower;
    private Button _bt_shotgun;
    private Button _bt_laserblast;
    private Button _bt_rocketlauncher;
    private Button _bt_camera;
    private Button _bt_retry;
    private Button _bt_fireball;
    private Button _bt_freeze;
    private Text _txt_money;
    private Text _txt_score;
    private Text _txt_hp;
    private Text _txt_wave;
    private Text _txt_bestscore;


    // Use this for initialization
    void Awake()
    {
        Transform c = transform.FindChild("Canvas");
        _bt_gatling = c.FindChild("ButtonGatling").GetComponent<Button>();
        _bt_flamethrower = c.FindChild("ButtonFlamethrower").GetComponent<Button>();
        _bt_shotgun = c.FindChild("ButtonShotgun").GetComponent<Button>();
        _bt_laserblast = c.FindChild("ButtonLaserBlast").GetComponent<Button>();
        _bt_rocketlauncher = c.FindChild("ButtonRocketLauncher").GetComponent<Button>();

        _bt_camera = c.FindChild("ButtonCamera").GetComponent<Button>();
        _bt_retry = c.FindChild("ButtonRetry").GetComponent<Button>();
        _bt_fireball = c.FindChild("ButtonFireball").GetComponent<Button>();
        _bt_freeze = c.FindChild("ButtonFreeze").GetComponent<Button>();

        _txt_money = c.FindChild("Money").GetComponent<Text>();
        _txt_bestscore = c.FindChild("BestScore").GetComponent<Text>();
        _txt_hp = c.FindChild("HP").GetComponent<Text>();
        _txt_score = c.FindChild("Score").GetComponent<Text>();
        _txt_wave = c.FindChild("Wave").GetComponent<Text>();

        SpawnManager1 = GetComponent<Spawn>();
        _weapons_list = Resources.LoadAll<GameObject>("Prefabs/Weapons");
        _ennemy_list = Resources.LoadAll<GameObject>("Prefabs/Enemies");

        // Deux pouvoirs
        _powers = new List<PullManager>();

        if(Fireball != null)
        {
            PullManager power = ScriptableObject.CreateInstance<PullManager>();
            power.Init(Fireball, 5);
            _powers.Add(power);
        }

        if (Freeze != null)
        {
            PullManager power = ScriptableObject.CreateInstance<PullManager>();
            power.Init(Freeze, 5);
            _powers.Add(power);
        }

        if (NewWeaponEffect != null)
            _newWeaponEffect = Instantiate(NewWeaponEffect);

        if (PowerSelection != null)
        {
            _powerSelection = Instantiate(PowerSelection);
            _powerSelection.gameObject.SetActive(false);
        }

        _weapons = new List<PullManager>();
        foreach (GameObject obj in _weapons_list)
        {
            PullManager pull = ScriptableObject.CreateInstance<PullManager>();
            pull.Init(obj, obj.GetComponent<Weapon>().MaxWeaponsInTheSameTime);
            _weapons.Add(pull);
        }

        _weapon_selected = false;
        _placing_weapon = false;
        _new_weapon = false;
        _waveIsStarted = false;
        _main_camera = null;
        SpawnManager1.Init(_ennemy_list, InitialEnemySize);
        BestScore = -1;
        _num_power = -1;

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
                RefreshUI();
            }
            else
            {
                if (Score > BestScore)
                    PlayerPrefs.SetInt(BestScoreKey, Score);

                _waveIsStarted = false;

                RefreshUI(true);
            }

        }

        if (_main_camera == null) // Vue globale
        {
            RaycastHit pos;

            if (_placing_weapon) // En train de placer une tourelle
            {
                pos = GetMouseRayPos(false, 11, 1); // Terrain, TransparentFX (TerrainCollider)

                if (pos.collider != null)
                    _temp_weapon.transform.position = new Vector3(pos.point.x, 0f, pos.point.z);

                GridGraph grid = (GridGraph)AstarPath.active.graphs[0];
                List<GraphNode> nodes = grid.GetNodesInArea(_temp_weapon.GetComponent<Collider>().bounds);

                bool walkable = true;

                foreach(GraphNode n in nodes)
                {
                    if(!n.Walkable)
                    {
                        walkable = false;
                        break;
                    }
                }

                _temp_weapon.GetComponent<Weapon>().ShowNodes(nodes, true, walkable ? Color.green : Color.red);

                if (Input.GetMouseButtonDown(0))
                {
                    if (walkable)
                    {
                        var guo = new GraphUpdateObject(_temp_weapon.GetComponent<Collider>().bounds);
                        var spawnPointNode = AstarPath.active.GetNearest(SpawnManager1.GetComponent<Spawn>().StartPoint.transform.position).node;
                        var goalNode = AstarPath.active.GetNearest(SpawnManager1.GetComponent<Spawn>().EndPoint.transform.position).node;

                        if (GraphUpdateUtilities.UpdateGraphsNoBlock(guo, spawnPointNode, goalNode, false))
                        {
                            // Valid tower position
                            // Since the last parameter (which is called "alwaysRevert") in the method call was false
                            // The graph is now updated and the game can just continue

                            SetTransparent(_temp_weapon, false);

                            if (_new_weapon)
                            {
                                PlayNewWeaponEffect(_temp_weapon.transform, Color.white);
                                Money -= _temp_weapon.GetComponent<Weapon>().Price;

                                StartCoroutine(StartWeaponColor(_temp_weapon));
                            }

                            _temp_weapon.GetComponent<Weapon>().ShowNodes(nodes, false);
                            ShowGrid(false);
                            _placing_weapon = false;
                            _new_weapon = false;
                            _temp_weapon = null;
                            RefreshUI();
                        }
                        else
                        {
                            // Invalid tower position. It blocks the path between the spawn point and the goal
                            // The effect on the graph has been reverted
                            _temp_weapon.GetComponent<Weapon>().ShowNodes(nodes, true, Color.red);
                            PlayNewWeaponEffect(_temp_weapon.transform, Color.red);
                        }
                    }
                    else
                    {
                        _temp_weapon.GetComponent<Weapon>().ShowNodes(nodes, true, Color.red);
                        PlayNewWeaponEffect(_temp_weapon.transform, Color.red);
                    }
                }
                else if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
                {
                    _temp_weapon.GetComponent<Weapon>().ShowNodes(nodes, false);
                    ShowGrid(false);
                    _placing_weapon = false;
                    _weapons[_temp_weapon.GetComponent<Weapon>().Id].RemoveObj(_temp_weapon);
                    _temp_weapon = null;
                    _new_weapon = false;
                }
            }
            else if(_num_power >= 0) // Position du pouvoir en cours de sélection
            {
                pos = GetMouseRayPos(false, 11, 1); // Terrain, TransparentFX (TerrainCollider)
                Vector3 p = Vector3.zero;

                if (_powerSelection != null)
                {
                    if (pos.collider != null)
                    {
                        p = new Vector3(pos.point.x, 0f, pos.point.z);
                        _powerSelection.transform.position = p;
                    }

                    if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
                    {
                        if(Input.GetMouseButtonDown(0))
                        {
                            GameObject power = _powers[_num_power].GetNextObj();
                            if (_num_power == 0)
                            {
                                power.transform.position = Camera.main.transform.position - Vector3.forward * 8f;
                                Vector3 dir = _powerSelection.transform.position - power.transform.position;
                                power.transform.rotation = Quaternion.LookRotation(dir);
                            }
                            else if (_num_power == 1)
                                power.transform.position = _powerSelection.transform.position + Vector3.up;

                            power.SetActive(true);
                            power.GetComponent<ParticleSystem>().Play();
                        }

                        _powerSelection.gameObject.SetActive(false);
                        _num_power = -1;
                        RefreshUI();
                    }
                }
            }
            else if (Input.GetMouseButtonDown(0)) // Sélection d'un objet sur la map
            {
                pos = GetMouseRayPos(true, 9, 10); // Weapons, Enemies

                if (pos.collider != null && pos.collider.tag == "Weapon")
                {
                    GameObject weapon = pos.collider.gameObject;
                    if (weapon != _temp_weapon)
                    {
                        if (_temp_weapon != null)
                            SetColor(_temp_weapon);

                        _temp_weapon = weapon;
                        Color color = _temp_weapon.GetComponent<Weapon>().GetColor();
                        Material mat = _temp_weapon.transform.FindChild("Base").FindChild("Temperature").GetComponent<Renderer>().material;
                        mat.SetColor("_Color", color);
                        mat.SetFloat("_Emission", 1f);
                        mat.SetColor("_EmissionColor", color * 0.8f);
                        _weapon_selected = true;
                        RefreshUI();
                        ShowGrid(true);
                    }
                }
                else if (_weapon_selected && !EventSystem.current.IsPointerOverGameObject())
                {
                    SetColor(_temp_weapon);
                    _temp_weapon = null;
                    _weapon_selected = false;
                    RefreshUI();
                    ShowGrid(false);
                }
            }
        }
	}

    public void PlayNewWeaponEffect(Transform pos, Color color)
    {
        if (_newWeaponEffect != null)
        {
            _newWeaponEffect.GetComponent<ParticleSystem>().Stop();
            _newWeaponEffect.GetComponent<ParticleSystem>().startColor = color;

            _newWeaponEffect.transform.position = pos.position + new Vector3(0f, 0.2f, 0f);
            _newWeaponEffect.transform.rotation = pos.rotation;
            _newWeaponEffect.GetComponent<ParticleSystem>().Play();
        }
    }

    public void ShowGrid(bool show)
    {
        GridGraph grid = (GridGraph)AstarPath.active.graphs[0];
        foreach(PullManager pull in _weapons)
        {
            foreach (GameObject weapon in pull.GetAllActive())
            {
                List<GraphNode> nodes = grid.GetNodesInArea(weapon.GetComponent<Collider>().bounds);
                weapon.GetComponent<Weapon>().ShowNodes(nodes, show);
            }
        }
    }

    public void UpdateGraph(Bounds b)
    {
        var guo = new GraphUpdateObject(b);
        // Set some settings
        guo.updatePhysics = true;
        AstarPath.active.UpdateGraphs(guo);
    }

    public void SetTransparent(GameObject weapon, bool transparent)
    {
        if (weapon != null)
        {
            string mode = transparent ? "Transparent" : "Opaque";
            Transform b = weapon.transform.FindChild("Base");
            Material temperature = b.FindChild("Temperature").GetComponent<Renderer>().material;
            Material mat = b.GetComponent<Renderer>().material;

            General.SetupMaterialWithBlendMode(mat, mode);
            General.ChangeMaterialForAllChild(b, mat, new string[] { "Temperature" });
            General.SetupMaterialWithBlendMode(temperature, mode);

            if (transparent)
            {
                mat.SetAlpha(0.3f);
                temperature.SetColor("_Color", Color.white);
                temperature.SetAlpha(0.3f);
            }

            weapon.GetComponent<Weapon>().enabled = !transparent;
        }
    }

    public void SetColor(GameObject weapon)
    {
        if (weapon != null)
        {
            Material mat = weapon.transform.FindChild("Base").FindChild("Temperature").GetComponent<Renderer>().material;
            mat.SetColor("_Color", weapon.GetComponent<Weapon>().GetColor());
            mat.SetFloat("_Emission", 2f);
            mat.SetColor("_EmissionColor", Color.black);
        }
    }

    public IEnumerator StartWeaponColor(GameObject weapon)
    {
        if (weapon != null)
        {
            Material mat = weapon.transform.FindChild("Base").FindChild("Temperature").GetComponent<Renderer>().material;
            Color color = Color.black;

            mat.SetFloat("_Emission", 2f);
            mat.SetColor("_EmissionColor", color);

            for (int i = 1; i <= 80; i++)
            {
                color.g = i / 100f;
                mat.SetColor("_Color", color);

                yield return new WaitForSeconds(0.005f);
            }
        }
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

            _bt_camera.transform.FindChild("Text").GetComponent<Text>().text = text_button;

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
        _wave = 1;
        _main_camera = null;
        Camera.main.enabled = true;

        _bt_camera.interactable = false;
        if (PlayerPrefs.HasKey(BestScoreKey))
            BestScore = PlayerPrefs.GetInt(BestScoreKey);

        foreach (PullManager pull in _weapons)
            pull.RemoveAll();

        _bt_retry.interactable = false;

        HP = 100;
        Score = 0;
        _wave = 0;

        AstarPath.active.Scan();

        SpawnManager1.NewWave(_wave);
        _waveIsStarted = true;

        RefreshUI();
    }

    private void RefreshUI(bool end = false)
    {
        string best_score = BestScore >= 0 ? string.Format("Meilleur score : {0}", BestScore) : "";
        _txt_money.text = string.Format("Argent : {0}", Money);
        _txt_bestscore.text = best_score;
        _txt_hp.text = string.Format("HP : {0}", HP);
        _txt_score.text = string.Format("Score : {0}", Score);
        _txt_wave.text = string.Format("Vague : {0}", _wave);

        bool power_selected = _num_power >= 0;
        bool show_weapons = !end && !power_selected && !_weapon_selected;

        _bt_gatling.interactable = show_weapons
            && Money >= _weapons_list[1].GetComponent<Weapon>().Price 
            && _weapons[1].GetAllActive().Count < _weapons[1].Size;

        _bt_shotgun.interactable = show_weapons
            && Money >= _weapons_list[5].GetComponent<Weapon>().Price 
            && _weapons[5].GetAllActive().Count < _weapons[5].Size;

        _bt_flamethrower.interactable = show_weapons
            && Money >= _weapons_list[0].GetComponent<Weapon>().Price 
            && _weapons[0].GetAllActive().Count < _weapons[0].Size;

        _bt_laserblast.interactable = show_weapons
            && Money >= _weapons_list[3].GetComponent<Weapon>().Price 
            && _weapons[3].GetAllActive().Count < _weapons[3].Size;

        _bt_rocketlauncher.interactable = show_weapons
            && Money >= _weapons_list[4].GetComponent<Weapon>().Price 
            && _weapons[4].GetAllActive().Count < _weapons[4].Size;

        _bt_camera.interactable = _weapon_selected && !power_selected;

        _bt_fireball.interactable = !_weapon_selected && !power_selected;
        _bt_freeze.interactable = !_weapon_selected && !power_selected;

        _bt_retry.interactable = end;
    }

    private RaycastHit GetMouseRayPos(bool ignoreTrigger = true, int layer1 = -1, int layer2 = -1, int layer3 = -1)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit_info;
        int mask;

        if (layer1 >= 0 && layer2 >= 0 && layer3 >= 0)
            mask = 1 << layer1 | 1 << layer2 | 1 << layer3;
        else if (layer1 >= 0 && layer2 >= 0)
            mask = 1 << layer1 | 1 << layer2;
        else if (layer1 >= 0)
            mask = 1 << layer1;
        else
            mask = -1;

        if (Physics.Raycast(ray, out hit_info, Mathf.Infinity, mask, ignoreTrigger ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide))
        {
            return hit_info;
        }
        
        return default(RaycastHit);
    }

    public void ReceiveDamage(int dmg, GameObject enemy)
    {
        HP -= dmg;

        StartCoroutine(WaitForEnemyBoom(enemy));

        RemoveTargetFromAllWeapons(enemy);

        if (HP <= 0)
        {
            Win = false;
            WaveIsOver = true;
        }

        RefreshUI();
    }
    
    private IEnumerator WaitForEnemyBoom(GameObject enemy)
    {
        yield return new WaitForSeconds(2.5f);

        SpawnManager1.SetDead(enemy);
    }

    public void SetDead(GameObject enemy)
    {
        Score += enemy.GetComponent<Enemy>().Points;
        Money += enemy.GetComponent<Enemy>().Points;

        RefreshUI();

        RemoveTargetFromAllWeapons(enemy);
    }

    public void RemoveEnemy(GameObject enemy)
    {
        SpawnManager1.SetDead(enemy);
    }

    public void RemoveTargetFromAllWeapons(GameObject enemy)
    {
        foreach (PullManager pull in _weapons)
        {
            foreach (GameObject weapon in pull.GetAllActive())
                weapon.GetComponent<Weapon>().RemoveTarget(enemy);
        }
    }

    public void NewWeapon(int nb)
    {
        if (_placing_weapon == false && Money >=_weapons_list[nb].GetComponent<Weapon>().Price)
        {
            RaycastHit pos = GetMouseRayPos(true, 11, 1);

            if (_temp_weapon != null)
                SetColor(_temp_weapon);

            _temp_weapon = _weapons[nb].GetNextObj();
            _temp_weapon.transform.position = pos.point;
            _temp_weapon.SetActive(true);

            Material temperature = _temp_weapon.transform.FindChild("Base").FindChild("Temperature").GetComponent<Renderer>().material;
            temperature.EnableKeyword("_EMISSION");

            SetTransparent(_temp_weapon, true);
            ShowGrid(true);

            _placing_weapon = true;
            _new_weapon = true;
        }
    }

    // 0 = Fireball, 1 = Freeze
    public void BeginPower(int nb)
    {
        if(_powerSelection != null)
        {
            Color c;

            if (nb == 0)
                c = Color.red;
            else
                c = Color.blue;

            _powerSelection.gameObject.SetActive(true);
            _powerSelection.transform.FindChild("Base").GetComponent<Renderer>().material.SetColor("_Color", c);
        }

        _num_power = nb;
        RefreshUI();
    }
}
