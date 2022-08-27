// Creater : xjm


using System;
using System.Collections.Generic;


public class LoadingUI : BasicUI
{
    public override UiId GetUiId()
    {
        return UiId.LoadingPanel;
    }


    private int num = 1;
    
    private LoadingPanel m_loadingPanel;
    public override void Awake(object param1 = null, object param2 = null, object param3 = null)
    {
        base.Awake(param1, param2, param3);
        m_loadingPanel = Transform.GetComponent<LoadingPanel>();

        List<LoopListItemModel> list = new List<LoopListItemModel>();
        num = 100;
        for (int i = 0; i < num; i++)
        {
            FruitModel mode = new FruitModel(i);
            list.Add(mode);
        }


        m_loadingPanel.LoopListsc.Init(list, ItemId.FruitItem ,5);
        m_loadingPanel.LoopListsc.MoveToModelIndex(0);
        
        AddButtonClickListener(m_loadingPanel.zero, () =>
        {
            List<LoopListItemModel> lists = new List<LoopListItemModel>();
            num = 0;
            for (int i = 0; i < num; i++)
            {
                FruitModel mode = new FruitModel(i);
                lists.Add(mode);
            }
            m_loadingPanel.LoopListsc.UpdateModelsData(lists);
        });
        
        
        AddButtonClickListener(m_loadingPanel.add, () =>
        {
            List<LoopListItemModel> lists = new List<LoopListItemModel>();
            num = 100;
            for (int i = 0; i < num; i++)
            {
                FruitModel mode = new FruitModel(i);
                lists.Add(mode);
            }
            m_loadingPanel.LoopListsc.UpdateModelsData(lists);
        });
        
        AddButtonClickListener(m_loadingPanel.mul, () =>
        {
            List<LoopListItemModel> lists = new List<LoopListItemModel>();
            num = 50;
            for (int i = 0; i < num; i++)
            {
                FruitModel mode = new FruitModel(i);
                lists.Add(mode);
            }
            m_loadingPanel.LoopListsc.UpdateModelsData(lists);
        });
        
        AddButtonClickListener(m_loadingPanel.add1, () =>
        {
            List<LoopListItemModel> lists = new List<LoopListItemModel>();
            num++;
            for (int i = 0; i < num; i++)
            {
                FruitModel mode = new FruitModel(i);
                lists.Add(mode);
            }
            m_loadingPanel.LoopListsc.UpdateModelsData(lists);
        });
        
        AddButtonClickListener(m_loadingPanel.mul1, () =>
        {
            List<LoopListItemModel> lists = new List<LoopListItemModel>();
            num--;
            for (int i = 0; i < num; i++)
            {
                FruitModel mode = new FruitModel(i);
                lists.Add(mode);
            }
            m_loadingPanel.LoopListsc.UpdateModelsData(lists);
        });
        
    }

    public override void OnClose()
    {
        base.OnClose();
        m_loadingPanel.LoopListsc.DistroyChildren();
    }
}



public class FruitModel : LoopListItemModel
{
    public Random ran = new Random();

    public float num;
    
    public FruitModel(int id) : base(id)
    {
        num = ran.Next(100);
    }
    
    
}
