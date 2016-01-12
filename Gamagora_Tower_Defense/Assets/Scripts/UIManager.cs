using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class UIManager : Singleton<UIManager>
{
    private List<Button> buttons;
    private int selected_button;

	// Use this for initialization
	void Awake ()
    {
        Init();
    }
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    public void Init()
    {
        buttons = new List<Button>(FindObjectsOfType<Button>()).FindAll(b => b.IsActive());
    }

    public void SelectButton(float v)
    {
        Debug.Log(v);
        if(v > 0)
        {
            if (selected_button < buttons.Count - 1)
                selected_button++;
            else
                selected_button = 0;
        }
        else
        {
            if (selected_button > 0)
                selected_button--;
            else
                selected_button = buttons.Count - 1;
        }

        EventSystem.current.SetSelectedGameObject(buttons[selected_button].gameObject, new BaseEventData(EventSystem.current));
    }
}
