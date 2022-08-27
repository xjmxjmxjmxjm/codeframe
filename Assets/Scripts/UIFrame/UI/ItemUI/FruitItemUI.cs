// Creater : xjm


using System;
using System.Collections.Generic;


public class FruitItemUI:ItemBase
{
    public override ItemId GetItemId()
    {
        return ItemId.FruitItem;
    }

    public override void OnUpdateUI(object param1 = null, object param2 = null, object param3 = null)
    {
        base.OnUpdateUI(param1, param2, param3);
        FruitModel model = param1 as FruitModel;

        FruitItem item = Transform.GetComponent<FruitItem>();
        item.id.text = model._Id.ToString();
        item.ran.text = model.num.ToString();
    }
}
