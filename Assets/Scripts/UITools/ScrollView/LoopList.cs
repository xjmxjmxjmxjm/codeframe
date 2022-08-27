using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LoopList : MonoBehaviour
{
    /// <summary>
    /// 没行的间隔
    /// </summary>
    [HideInInspector]
    public float OffsetY = 5;
    /// <summary>
    /// 没列的间隔
    /// </summary>
    [HideInInspector]
    public float OffsetX = 5;
    /// <summary>
    /// 一行有几个
    /// </summary>
    [HideInInspector]
    public int RowCount = 1;
    /// <summary>
    /// item的名称
    /// </summary>
    private ItemId ItemName;
    
    
    private float _itemWidth;
    private float _itemHieght;
    private RectTransform _content;
    private List<LoopListItem> _items;
    private List<LoopListItemModel> _models;


    //private ItemBase _itemBase;
    
    
    /// <summary>
    /// 初始化 loop
    /// </summary>
    /// <param name="models"></param>
    /// <param name="itemName"></param>
    /// <param name="rowCount"></param>
    /// <param name="offsetX"></param>
    /// <param name="offsetY"></param>
    public void Init(List<LoopListItemModel> models, ItemId itemName, int rowCount = 1, float offsetX = 10, float offsetY = 10)
    {
//        if (models == null || models.Count <= 0)
//        {
//            LogUtil.LogError("models data is error!");
//            return;
//        }
//        if (offsetX < 0)
//        {
//            LogUtil.LogError("offsetX data is error!");
//            return;
//        }
//        if (offsetY < 0)
//        {
//            LogUtil.LogError("offsetY data is error!");
//            return;
//        }
        if (rowCount <= 0)
        {
            LogUtil.LogError("rowCount data is error!");
            return;
        }
       
        if (models == null)
        {
            //LogUtil.LogError("models data is error!");
            models = new List<LoopListItemModel>();
            //return;
        }
        
        _items = new List<LoopListItem>();
        _models = models;
        ItemName = itemName;
        RowCount = rowCount;
        OffsetX = offsetX;
        OffsetY = offsetY;
        
        //模拟数据获取
        //GetModel();
        
        
        _content = transform.Find("Viewport/Content").GetComponent<RectTransform>();
//        GameObject obj = Resources.Load<GameObject>("LoopListItem");
        ItemBase itemBase = UIManager.Instance.GetItem(ItemName, _content);
        //_itemBase = itemBase;
        RectTransform rect = itemBase.Transform.GetComponent<RectTransform>();
        _itemHieght = rect.rect.height;
        _itemWidth = rect.rect.width;
        UIManager.Instance.DestroyItem(itemBase);
        itemBase = null;

        int num = GetShowItemNum(_itemHieght, OffsetY);//根据显示的高度生成 多少个 item

        int maxLine = GetMaxLineByModels();//数据列表有可能是 0
        //maxLine = maxLine == 0 ? num : maxLine;  //如果根据数据计算出来0  还是取根据组件高度计算出来的数量
        num = num > maxLine ? maxLine : num; //如果根据高度计算出来的数量 大于了 数据计算出来的数量  就根据数据计算出来数量生成item

        if (num > 0)
        {
            SpawnItem(num);
        }
        SetContentSize();
        
        transform.GetComponent<ScrollRect>().onValueChanged.AddListener(ValueChange);
    }

    /// <summary>
    /// 更新数据
    /// </summary>
    /// <param name="models"></param>
    public void UpdateModelsData(List<LoopListItemModel> models)
    {
        int oldLenght = _models.Count;
        _models = models;
        
        SetContentSize();
        int num = _items.Count;
        int maxLine = GetMaxLineByModels();
        int numshow = GetShowItemNum(_itemHieght, OffsetY);//根据显示的高度生成 多少个 item
        int res = numshow > maxLine ? maxLine : numshow;

        if (num < res)
        {
            SpawnItem(res - num);
        }
        
        
        _items.Sort((x, y) => x.CompareTo(y)); //从小到大
        for (int i = 0; i < num; i++)
        {
            if (i < res)
            {
                _items[i].SetActive(true);
            }
            else
            {
                _items[i].SetActive(false);
            }
        }

        if (res <= 0) return;//数据变成0个了

        for (int i = 0; i < res; i++)
        {
            _items[i].UpdateShowItemNum(res);
        }


//        int maxId = _items[res - 1].GetId();

//        //代表数据减少了
//        if (oldLenght > _models.Count)
//        {
//            //这个代表 当前最后一个item的位置id 大于了 最大数据的id
//            if (maxId > maxLine - 1)
//            {
//                maxId = maxLine - 1;
//                for (int i = res - 1; i >= 0; i--)
//                {
//                    _items[i].UpdateId(maxId--);
//                }
//            }
//        }
//        //代表增加了
//        else
//        {
//            int startId = Mathf.FloorToInt(_content.anchoredPosition.y / (-_itemHieght + OffsetY));
//            int mul = 1;
//            for (int i = 0; i < res; i++)
//            {
//                int line = startId + i;
//                if (line > maxLine - 1)  //超过了最大的id行
//                {
//                    line = startId - mul;
//                    mul++;
//                }
//                _items[i].UpdateId(line);
//            }
//        }
        
        
        int startId = Mathf.FloorToInt(_content.anchoredPosition.y / (_itemHieght + OffsetY));
        int mul = 1;
        for (int i = 0; i < res; i++)
        {
            int line = startId + i;
            if (line > maxLine - 1)  //超过了最大的id行
            {
                line = startId - mul;
                mul++;
            }
            _items[i].UpdateId(line);
        }
        

        
        for (int i = 0; i < res; i++)
        {
            _items[i].ChangeId(_items[i].GetId(), true);
        } 
        
    }

    /// <summary>
    /// 移动到某一个数据行      传入列表的索引
    /// </summary>
    /// <param name="model"></param>
    public void MoveToModelIndex(int modelIndex)
    {
        int moveLine = (int)Mathf.Ceil(modelIndex / (float)RowCount);
        int maxLine = GetMaxLineByModels();
        if (moveLine > maxLine) return;
        //if (moveLine < 0) return;
        if (maxLine < 0) return;
        moveLine = moveLine - 1 < 0 ? 0 : moveLine - 1;
        float posY = moveLine * _itemHieght + moveLine * OffsetY;
        Vector3 srcPos = _content.transform.localPosition;
        posY = posY <= 0 ? 0 : posY;
        float height = _content.sizeDelta.y;
        posY = posY >= height ? height : posY;
        _content.transform.localPosition = new Vector3(srcPos.x, posY, srcPos.z);

        int length = _items.Count;
        int mul = 1;
        for (int i = 0; i < length; i++)
        {
            if (_items[i].gameObject.activeSelf)
            {
                int line = moveLine + i;
                if (line >= maxLine)  //超过了最大的id行
                {
                    line = moveLine - mul;
                    mul++;
                }
                _items[i].ChangeId(line, true);
            }
        } 
    }

    public void DistroyChildren()
    {
        int length = _items.Count;
        for (int i = 0; i < length; i++)
        {
            _items[i].DestroyChildren();
        }
    }


    /// <summary>
    /// 根据数据模型数量 计算得出生成多少行
    /// </summary>
    /// <returns></returns>
    private int GetMaxLineByModels()
    {
        return (int)Mathf.Ceil(_models.Count / (float)RowCount);
    }
 
    private void ValueChange(Vector2 data)
    {
        int len = _items.Count;
        for (int i = 0; i < len; i++)
        {
            _items[i].OnValueChange();
        } 
    }

    private int GetShowItemNum(float itemHeight, float offset)
    {
        float height = GetComponent<RectTransform>().rect.height;
        return Mathf.CeilToInt(height / (itemHeight + offset)) + 1;
    }

    private void SpawnItem(int num)
    {
        float offsetx = ItemOffSetX();
        for (int i = 0; i < num; i++)
        {
            _items.Add(CreateSingleItem(i, offsetx, num));
        }
    }

    private float ItemOffSetX()
    {
        return _itemWidth * 0.5f * (RowCount - 1);
    }

    private LoopListItem CreateSingleItem(int i, float offsetx, int num)
    {
        Transform temp = null;
        LoopListItem itemTemp = null;
        List<ItemBase> tempLst = null;
        ItemBase itemBase = null;
        
        GameObject obj = new GameObject();
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(RowCount * _itemWidth + (RowCount - 1) * OffsetX, _itemHieght);
        itemTemp = obj.AddComponent<LoopListItem>();
        itemTemp.AddGetDataListener(GetData);
        itemTemp.AddGetDataValidListener(GetDataValid);
        itemTemp.transform.SetParent(_content, false);
        tempLst = new List<ItemBase>();
        for (int j = 0; j < RowCount; j++)
        {
//            if (i == 0 && j == 0)
//            {
//                temp = _itemBase.Transform;
//                temp.SetParent(itemTemp.transform);
//                itemBase = _itemBase;
//            }
//            else
//            {
//                itemBase = UIManager.Instance.GetItem(ItemName, itemTemp.transform);
//                temp = itemBase.Transform;
//            }
            
            itemBase = UIManager.Instance.GetItem(ItemName, itemTemp.transform);
            temp = itemBase.Transform;
                
            temp.localPosition = new Vector3(j * _itemWidth + j * OffsetX - offsetx, 0, 0);
            tempLst.Add(itemBase);
        }
        
        
#if UNITY_EDITOR
        obj.name = itemBase.GameObject.name + "_Temp";
#endif
        
        itemTemp.Init(i, OffsetY, num, tempLst);
        return itemTemp;
    }

    
    private bool GetDataValid(int index)
    {
//        if (index < 0 || index >= _models.Count)
//        {
//            return false;
//        }
//
//        return true;
        if (index < 0) return false;
        if (_models.Count <= 0) return false;

        List<LoopListItemModel> temp = GetData(index);
        return temp.Count > 0;
        
        if (index == 0) return true;
        int i = index * RowCount - RowCount;
        if (i < 0 || i >= _models.Count)
        {
            return false;
        }

        return true;
    }

    private List<LoopListItemModel> GetData(int index)
    {
//        if (index < 0 || index >= (_models.Count / RowCount))
//        {
//            return null;
//        }
//
//        return _models[index];

        int modelCount = _models.Count;
        List<LoopListItemModel> temp = new List<LoopListItemModel>();
        int i = index * RowCount;
        int tempi = i;
        for (; i < tempi + RowCount; i++)
        {
            if (i < modelCount && i >= 0)
            {
                temp.Add(_models[i]);    
            }
            else
            {
                break;
            }
            
        }

        return temp;
    }

//    private void GetModel()
//    {
//        for (int i = 0; i < 100; i++)
//        {
//            _models.Add(new LoopListItemModel(i));
//        }
//    }

    private void SetContentSize()
    {
        float count = Mathf.Ceil((float)_models.Count / RowCount);
        count = count <= 0 ? 1 : count;
        float y = count * _itemHieght + (count - 1) * OffsetY;
        _content.sizeDelta = new Vector2(_content.sizeDelta.x, y);
    }
    
}
