using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.UI;

public class CircleImage : Image
{
    [SerializeField]
    public int segements = 100;

    //[SerializeField]
    //private float showPercent = 0f;
    
    //[SerializeField]
    //private float _alpha = 0f;


    private List<Vector2> m_oCross;

   
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        
        
        m_oCross = new List<Vector2>();
        
        float width = rectTransform.rect.width;
        float height = rectTransform.rect.height;
        //int realSegments = (int)(showPercent * segements);
        
        Vector4 uv = overrideSprite != null ? DataUtility.GetOuterUV(overrideSprite) : Vector4.zero;
        float uvWidth = uv.z - uv.x;
        float uvHeight = uv.w - uv.y;
        Vector2 uvCenter = new Vector2(uvWidth * 0.5f, uvHeight * 0.5f);
        Vector2 convertRotia = new Vector2(uvWidth / width, uvHeight / height);

        float radian = (Mathf.PI * 2) / segements;
        float radius = width * 0.5f;
        
        UIVertex origin = new UIVertex();
        //byte tempColor = (byte) (255 * showPercent);
        origin.color = color;//new Color32(tempColor, tempColor, tempColor, 255);

        Vector2 orginPos = new Vector2((0.5f - rectTransform.pivot.x) * width, (0.5f - rectTransform.pivot.y) * height);
        Vector2 vertPos = Vector2.zero;
        origin.position = orginPos;
        origin.uv0 = new Vector2(vertPos.x * convertRotia.x + uvCenter.x, vertPos.y * convertRotia.y + uvCenter.y);
        vh.AddVert(origin);

        
        float curRadian = 0;
        Vector2 posTemp;

        for (int i = 0; i < segements + 1; i++)
        {
            float x = Mathf.Cos(curRadian) * radius;
            float y = Mathf.Sin(curRadian) * radius;
            curRadian += radian;
            
            UIVertex tempOrigin = new UIVertex();
            /*if (i < vertexCount)
            {
                tempOrigin.color = color;
            }
            else
            {
                tempOrigin.color = new Color32(60, 60, 60, 255);
            }*/
            tempOrigin.color = color;
            posTemp = new Vector2(x, y);
            tempOrigin.position = new Vector2(x + orginPos.x, y + orginPos.y);
            tempOrigin.uv0 = new Vector2(posTemp.x * convertRotia.x + uvCenter.x, posTemp.y * convertRotia.y + uvCenter.y);
            vh.AddVert(tempOrigin);
            m_oCross.Add(new Vector2(x + orginPos.x, y + orginPos.y));
        }

        int id = 1;
        //int resCount = segements;
//        if (_alpha != 0)
//        {
//            resCount += 4;
//        }
        for (int i = 0; i < segements; i++)
        {
            vh.AddTriangle(id, 0, id + 1);
            id++;
        }
        
       
    }

    public override bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, eventCamera,
            out localPoint);

        return IsValid(localPoint);
    }

    private bool IsValid(Vector2 localPoint)
    {
        return GetCrossPointNum(localPoint, m_oCross) % 2 == 1;
    }

    private int GetCrossPointNum(Vector2 localPoint, List<Vector2> verts)
    {
        Vector3 vert1 = Vector3.zero;
        Vector3 vert2 = Vector3.zero;
        int num = 0;
        int count = verts.Count;
        for (int i = 0; i < count; i++)
        {
            vert1 = verts[i];
            vert2 = verts[(i + 1) % count];
            if (IsYInRange(localPoint, vert1, vert2))
            {
                if (localPoint.x < GetX(vert1, vert2, localPoint.y))
                {
                    num++;
                }
            }
        }

        return num;
    }

    private bool IsYInRange(Vector2 localPoint, Vector3 vert1, Vector3 vert2)
    {
        if (vert1.y > vert2.y)
        {
            return vert1.y > localPoint.y && vert2.y < localPoint.y;
        }
        else
        {
            return vert2.y > localPoint.y && vert1.y < localPoint.y;
        }
    }

    private float GetX(Vector3 vert1, Vector3 vert2, float y)
    {
        float k = (vert1.y - vert2.y) / (vert1.x - vert2.x);
        return vert1.x + (y - vert1.y) / k;
    }
}










