

public class LoadingUI : BasicUI
{
    public override UiId GetUiId()
    {
        return UiId.LoadingPanel;
    }


    private LoadingPanel m_loadingPanel;
    public override void Awake(object param1 = null, object param2 = null, object param3 = null)
    {
        base.Awake(param1, param2, param3);
        m_loadingPanel = Transform.GetComponent<LoadingPanel>();
    }

    public void Test()
    {
        AddButtonClickListener(m_loadingPanel.fsdfds, () => { });
        ResourceManager.Instance.ReleaseResource(m_loadingPanel.RoleImg);
        UIManager.Instance.HideWnd(this);
    }
}