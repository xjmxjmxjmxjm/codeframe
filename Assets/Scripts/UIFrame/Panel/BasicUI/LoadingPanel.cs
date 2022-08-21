// Creater : xjm


using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;
using Slider = UnityEngine.UI.Slider;
using Toggle = UnityEngine.UI.Toggle;

public class LoadingPanel:MonoBehaviour
{
    private Image _RoleImg;
    public Image RoleImg
    {
        get
        {
            if (_RoleImg == null)
            {
                _RoleImg = transform.Find("img_RoleImg").GetComponent<Image>();
            }
            return _RoleImg;
        }
    }

    private Scrollbar _fdsf;
    public Scrollbar fdsf
    {
        get
        {
            if (_fdsf == null)
            {
                _fdsf = transform.Find("img_RoleImg/LoadingPanel/scrob_fdsf").GetComponent<Scrollbar>();
            }
            return _fdsf;
        }
    }

    private Button _fsdfds;
    public Button fsdfds
    {
        get
        {
            if (_fsdfds == null)
            {
                _fsdfds = transform.Find("img_RoleImg/LoadingPanel/scrob_fdsf/btn_fsdfds").GetComponent<Button>();
            }
            return _fsdfds;
        }
    }

    private Slider _uuu;
    public Slider uuu
    {
        get
        {
            if (_uuu == null)
            {
                _uuu = transform.Find("img_RoleImg/LoadingPanel/scrob_fdsf/btn_fsdfds/sli_uuu").GetComponent<Slider>();
            }
            return _uuu;
        }
    }

    private ScrollView _scrov;
    public ScrollView scrov
    {
        get
        {
            if (_scrov == null)
            {
                _scrov = transform.Find("img_RoleImg/LoadingPanel/scrob_fdsf/btn_fsdfds/sli_uuu/s_fsdfsd/scrov_scrov").GetComponent<ScrollView>();
            }
            return _scrov;
        }
    }

    private Text _RoleName;
    public Text RoleName
    {
        get
        {
            if (_RoleName == null)
            {
                _RoleName = transform.Find("fsdf/text_RoleName").GetComponent<Text>();
            }
            return _RoleName;
        }
    }

}
