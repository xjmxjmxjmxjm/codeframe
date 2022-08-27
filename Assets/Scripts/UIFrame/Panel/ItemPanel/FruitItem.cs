// Creater : xjm


using UnityEngine;
using UnityEngine.UI;
using Util;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;
using Slider = UnityEngine.UI.Slider;
using Toggle = UnityEngine.UI.Toggle;

public class FruitItem:MonoBehaviour
{
    private Text _id;
    public Text id
    {
        get
        {
            if (_id == null)
            {
                _id = transform.Find("Image/text_id").GetComponent<Text>();
            }
            return _id;
        }
    }

    private Text _ran;
    public Text ran
    {
        get
        {
            if (_ran == null)
            {
                _ran = transform.Find("Image/text_ran").GetComponent<Text>();
            }
            return _ran;
        }
    }

}
