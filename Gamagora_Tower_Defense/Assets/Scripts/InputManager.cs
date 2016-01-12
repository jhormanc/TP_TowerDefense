using UnityEngine;
using System.Collections;

public class InputManager : Singleton<InputManager>
{
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
    }
	
	// Update is called once per frame
	void Update()
    {
        float vertical = Input.GetAxisRaw("Vertical");

        if (vertical != 0)
        {
            if (isAxisInUse == false)
            {
                ui_manager.SelectButton(vertical);
                isAxisInUse = true;
            }
        }

        if (vertical == 0)
        {
            isAxisInUse = false;
        }
    }
}
