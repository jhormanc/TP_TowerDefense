using UnityEngine;
using System.Collections;

public class InputManager : Singleton<InputManager>
{
    public bool Cancel { get; private set; }
    public bool Submit { get; private set; }

    private UIManager ui_manager;
    private GameManager game_manager;
    private bool isAxisInUse;

    // Guarantee this will be always a singleton only - can't use the constructor!
    protected InputManager() { }

    // Use this for initialization
    void Awake()
    {
        ui_manager = UIManager.Instance;
        game_manager = GameManager.Instance;
        isAxisInUse = false;
        Cancel = false;
        Submit = false;
    }
	
	// Update is called once per frame
	void Update()
    {
        float vertical = Input.GetAxisRaw("Vertical");

        if (vertical != 0)
        {
            if (isAxisInUse == false)
            {
                isAxisInUse = true;
            }
        }

        if (vertical == 0)
        {
            isAxisInUse = false;
        }
  
        //Cancel = Input.GetKeyDown("Cancel");
        //Submit = Input.GetKeyDown("Submit");


    }
}
