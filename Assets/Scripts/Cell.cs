using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CellBase
{
    public class Cell
    {
        public GameObject cellObject;
        protected Color cellColor;
        protected Vector3 cellPosition;
        
        public void SetColor(float r, float g, float b)
        {
            cellColor = new Color(r, g, b);
            cellObject.GetComponent<SpriteRenderer>().color = cellColor;
        }

        public Color GetColor()
        {
            return cellColor;
        }
                
        public void SetCellObject(GameObject _cellObject)
        {
            cellObject = _cellObject;
        }

        public GameObject GetCellObject()
        {
            return cellObject;
        }
        
        public void SetCellPosition(Vector3 _cellPosition)
        {
            cellPosition = _cellPosition;
        }

        public Vector3 GetCellPosition()
        {
            return cellPosition;
        }
    }
}