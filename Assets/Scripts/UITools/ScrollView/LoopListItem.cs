using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoopListItem : MonoBehaviour, IComparable<LoopListItem>
{
    [SerializeField]
    private int _Id = -1;
    
    private RectTransform _rect;

    private RectTransform _Rect
    {
        get
        {
            if (_rect == null)
            {
                _rect = GetComponent<RectTransform>();
            }

            return _rect;
        }
    }

  
    /// <summary>
    /// 这里存储的是 这个 item 上的  itembase 的 子类引用
    /// </summary>
    private List<ItemBase> _itemBase;


    private Func<int, List<LoopListItemModel>> _getData;
    private Func<int, bool> _getDataValid;
    //private int _startId, _endId;
    private RectTransform _content;
    private float _offset;
    private int _showItemNum;
    //private LoopListItemModel _model;

    public void Init(int id, float offsetY, int showItemNum, List<ItemBase> itemBase)
    {
        _content = transform.parent.GetComponent<RectTransform>();
        _offset = offsetY;
        _showItemNum = showItemNum;
        _itemBase = itemBase;
        _Rect.anchorMin = new Vector2(0, 1);
        _Rect.anchorMax = new Vector2(0, 1);
        _Rect.pivot = new Vector2(0, 1);
        ChangeId(id);
    }

    public void UpdateShowItemNum(int showItemNum)
    {
        _showItemNum = showItemNum;
    }

    public int GetId()
    {
        return _Id;
    }

    public void UpdateId(int id)
    {
        _Id = id;
    }

    public void AddGetDataListener(Func<int, List<LoopListItemModel>> getData)
    {
        _getData = getData;
    }
    public void AddGetDataValidListener(Func<int, bool> getDataValid)
    {
        _getDataValid = getDataValid;
    }
    
    public void OnValueChange()
    {
        if (gameObject.activeSelf == false) return;
        
        int _startId, _endId = 0;
        UpdateIdRange(out _startId, out _endId);
        JudgeSelfId(_startId, _endId);
    }

    private void UpdateIdRange(out int _startId, out int _endId)
    {
        _startId = Mathf.FloorToInt(_content.anchoredPosition.y / (_Rect.rect.height + _offset));
        _endId = _startId + _showItemNum - 1;
    }

    private void JudgeSelfId(int _startId, int _endId)
    {
        int offset = 0;
        if (_Id < _startId)
        {
            offset = _startId - _Id - 1;
            ChangeId(_endId - offset);
        }
        else if (_Id > _endId)
        {
            offset = _Id - _endId - 1;
            ChangeId(_startId + offset);
        }
    }

    public void ChangeId(int id, bool force = false)
    {
        bool isvalid = JudgeIdValid(id);
//        if (isvalid == false)
//        {
//            SetActive(false);
//        }
        if (isvalid && id != _Id || force)
        {
            //SetActive(true);
            _Id = id;
            SetPos();
            
            int length = _itemBase.Count;

            List<LoopListItemModel> temp = _getData(_Id);
            int tempLength = temp.Count;
            int i = 0;
            
            for (; i < tempLength; i++)
            {
                _itemBase[i].SetActive(_itemBase[i].GameObject, true);
                _itemBase[i].OnUpdateUI(temp[i]);
            }

            for (; i < length; i++)
            {
                _itemBase[i].SetActive(_itemBase[i].GameObject, false);
            } 
        }

    }
    

    public void SetActive(bool isActive)
    {
        if (gameObject.activeSelf == isActive)
        {
            
        }
        else
        {
            gameObject.SetActive(isActive);
        }
    }

    private void SetPos()
    {
        _Rect.anchoredPosition = new Vector2(0, -_Id * (_Rect.rect.height + _offset));
    }

    private bool JudgeIdValid(int id)
    {
        return _getDataValid(id);
    }

    public void DestroyChildren()
    {
        int length = _itemBase.Count;
        for (int i = 0; i < length; i++)
        {
            UIManager.Instance.DestroyItem(_itemBase[i]);
        }
        Destroy(gameObject);
    }

    public int CompareTo(LoopListItem other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        return _Id.CompareTo(other._Id);
    }
}
