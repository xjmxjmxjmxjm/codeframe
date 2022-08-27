// Creater : xjm


using UnityEngine;
using UnityEngine.UI;
using Util;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;
using Slider = UnityEngine.UI.Slider;
using Toggle = UnityEngine.UI.Toggle;

public class LoadingPanel:MonoBehaviour
{
    private ScrollView _sc;
    public ScrollView sc
    {
        get
        {
            if (_sc == null)
            {
                _sc = transform.Find("scrov_sc").GetComponent<ScrollView>();
            }
            return _sc;
        }
    }
    private Transform _Transsc;
    public Transform Transsc
    {
        get
        {
            if (_Transsc == null)
            {
                _Transsc = transform.Find("scrov_sc");
            }
            return _Transsc;
        }
    }
    private LoopList _LoopListsc;
    public LoopList LoopListsc
    {
        get
        {
            if (_LoopListsc == null)
            {
                _LoopListsc = transform.Find("scrov_sc").GetOrAddComponent<LoopList>();
            }
            return _LoopListsc;
        }
    }

    private Button _zero;
    public Button zero
    {
        get
        {
            if (_zero == null)
            {
                _zero = transform.Find("btn_zero").GetComponent<Button>();
            }
            return _zero;
        }
    }

    private Button _add;
    public Button add
    {
        get
        {
            if (_add == null)
            {
                _add = transform.Find("btn_add").GetComponent<Button>();
            }
            return _add;
        }
    }

    private Button _mul;
    public Button mul
    {
        get
        {
            if (_mul == null)
            {
                _mul = transform.Find("btn_mul").GetComponent<Button>();
            }
            return _mul;
        }
    }

    private Button _add1;
    public Button add1
    {
        get
        {
            if (_add1 == null)
            {
                _add1 = transform.Find("btn_add1").GetComponent<Button>();
            }
            return _add1;
        }
    }

    private Button _mul1;
    public Button mul1
    {
        get
        {
            if (_mul1 == null)
            {
                _mul1 = transform.Find("btn_mul1").GetComponent<Button>();
            }
            return _mul1;
        }
    }

}
