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

    public Spawn SpawnManager1;
    public List<Spawn> SpawnManager;

    public GameObject Terrain;
    public GameObject NewWeaponEffect;

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
    public int InitialEnemySize;
    private static GameObject[] _ennemy_list;
    private int _wave;
    

    // Use this for initialization
    void Awake()
    {
         SpawnManager1 = GetComponent<Spawn>();
        _weapons_list = Resources.LoadAll<GameObject>("Prefabs/Weapons");
        _ennemy_list = Resources.LoadAll<GameObject>("Prefabs/Enemies");
        if (NewWeaponEffect != null)
            _newWeaponEffect = Instantiate(NewWeaponEffect);
        _wave = 1;

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
            RaycastHit pos;

            if (_placing_weapon)
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
                                //_weapons.Add(_temp_weapon);
                                Money -= _temp_weapon.GetComponent<Weapon>().Price;

                                StartCoroutine(StartWeaponColor(_temp_weapon));
                            }

                            RefreshUI();
                            _temp_weapon.GetComponent<Weapon>().ShowNodes(nodes, false);
                            ShowGrid(false);
                            _placing_weapon = false;
                            _new_weapon = false;
                            _temp_weapon = null;
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
            else if (Input.GetMouseButtonDown(0))
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
        _main_camera = null;
        Camera.main.enabled = true;

        transform.FindChild("Canvas").FindChild("ButtonCamera").GetComponent<Button>().interactable = false;
        if (PlayerPrefs.HasKey(BestScoreKey))
            BestScore = PlayerPrefs.GetInt(BestScoreKey);

        foreach (PullManager pull in _weapons)
            pull.RemoveAll();

        transform.FindChild("Canvas").FindChild("ButtonRetry").GetComponent<Button>().interactable = false;

        HP = 100;
        Score = 0;
        _wave = 0;

        AstarPath.active.Scan();

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

        // TODO List Pull
        if (enemy.GetComponent<Little1>() != null)
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
        if (enemy.GetComponent<Little1>() != null)
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
}
