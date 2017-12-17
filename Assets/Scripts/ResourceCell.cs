using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CellBase;

namespace CellResource
{
    public class ResourceCell : Cell
    {
        private int sugar; //<0,20>
        private int maxCellSugar; //single cell potential
        private const int maxSugar = 20; //possible upper limit for sugar
        private int regrowthRate;
        public bool isTaken;
                
        public ResourceCell(int _sugar, int _maxCellSugar, int _regrowthRate, GameObject _cellObject, Vector3 _cellPosition)
        {            
            sugar = _sugar;
            maxCellSugar = _maxCellSugar;
            regrowthRate = _regrowthRate;
            isTaken = false;

            cellObject = _cellObject;
            cellPosition = _cellPosition;

            cellColor = new Color(0.0f, (float)sugar/(float)maxSugar, 0.0f);
            cellObject.GetComponent<SpriteRenderer>().color = cellColor;
        }

        public void SetSugar(int _sugar)
        {
            sugar = _sugar;

            cellColor = new Color(0.0f, (float)sugar / (float)maxSugar, 0.0f);
            cellObject.GetComponent<SpriteRenderer>().color = cellColor;
        }

        public int GetSugar()
        {
            return sugar;
        }

        public void RegrowResources()
        {
            if(sugar <= (maxCellSugar - regrowthRate))
            {
                SetSugar(sugar + regrowthRate);
            } else
            {
                SetSugar(maxCellSugar);
            }
        }
    }
}
