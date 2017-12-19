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
        public const int maxSugar = 20; //possible upper limit for sugar
        private int regrowthRate; //how much resources grow back in a single timestep
        public bool isTakenBasic; //state of a grid - if it's occupied by a basic agent
        public bool isTakenAttacker;  //state of a grid - if it's occupied by a attacker agent

        public ResourceCell(int _sugar, int _maxCellSugar, int _regrowthRate, GameObject _cellObject, Vector3 _cellPosition)
        {            
            sugar = _sugar;
            maxCellSugar = _maxCellSugar;
            regrowthRate = _regrowthRate;
            isTakenBasic = false;
            isTakenAttacker = false;

            cellObject = _cellObject;
            cellPosition = _cellPosition;

            SetColor(0.0f, (float)sugar / (float)maxSugar, 0.0f);
        }

        public void SetSugar(int _sugar)
        {
            sugar = _sugar;
            SetColor(0.0f, (float)sugar / (float)maxSugar, 0.0f);
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
