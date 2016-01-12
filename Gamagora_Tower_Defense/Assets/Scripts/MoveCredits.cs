using UnityEngine;
using System.Collections;

public class MoveCredits : MonoBehaviour
{
    private RectTransform _txt_credits;
    private float screen_height;

    // Use this for initialization
    void Awake ()
    {
        _txt_credits = transform.FindChild("Panel").FindChild("CreditsText").GetComponent<RectTransform>();
        screen_height = Screen.height;
        InitTextPos(-_txt_credits.rect.height);
    }
	
	// Update is called once per frame
	void Update ()
    {
        _txt_credits.localPosition += new Vector3(0f, 1f, 0f);

        if (_txt_credits.localPosition.y > screen_height * 0.5f + _txt_credits.rect.height)
            InitTextPos(-screen_height * 0.5f - _txt_credits.rect.height);
    }

    private void InitTextPos(float y)
    {
        _txt_credits.localPosition = new Vector3(_txt_credits.localPosition.x, y, _txt_credits.localPosition.z);
    }
}
