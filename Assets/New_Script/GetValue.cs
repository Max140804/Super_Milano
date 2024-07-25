using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetValue : MonoBehaviour
{
    public int topValue;
    public int bottomValue;
    public bool isTop;

    private void Start()
    {
        CardData val = GetComponentInParent<CardData>();

        topValue = val.topValue;
        bottomValue = val.bottomValue;
    }

    private void Update()
    {
        Cell cell = GetComponentInParent<Cell>();
        if(isTop && cell!= null)
        {
            cell.cellValue = topValue;
        }
        else if(!isTop && cell!= null)
        {
            cell.cellValue = bottomValue;
        }
    }

}
