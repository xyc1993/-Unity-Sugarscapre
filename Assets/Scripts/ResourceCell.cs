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
        private int spice; //<0,20>
        private int maxCellSpice; //single cell potential
        public const int maxResource = 20; //possible upper limit for any resource
        private int regrowthRate; //how much resources grow back in a single timestep
        public bool isTakenBasic; //state of a grid - if it's occupied by a basic agent
        public bool isTakenAttacker;  //state of a grid - if it's occupied by an attacker agent
        public bool isTakenTrader;  //state of a grid - if it's occupied by a trader agent

        public ResourceCell(int _sugar, int _maxCellSugar, int _spice, int _maxCellSpice, int _regrowthRate, GameObject _cellObject, Vector3 _cellPosition)
        {            
            sugar = _sugar;
            maxCellSugar = _maxCellSugar;
            spice = _spice;
            maxCellSpice = _maxCellSpice;

            regrowthRate = _regrowthRate;
            isTakenBasic = false;
            isTakenAttacker = false;
            isTakenTrader = false;

            cellObject = _cellObject;
            cellPosition = _cellPosition;

            SetColor(0.0f, (float)sugar / (float)maxResource, (float)spice / (float)maxResource);
        }

        public void SetResources(int _sugar, int _spice)
        {
            sugar = _sugar;
            spice = _spice;
            SetColor(0.0f, (float)sugar / (float)maxResource, (float)spice / (float)maxResource);
        }

        public void SetSugar(int _sugar)
        {
            sugar = _sugar;
            SetColor(0.0f, (float)sugar / (float)maxResource, (float)spice / (float)maxResource);
        }

        public void SetSpice(int _spice)
        {
            spice = _spice;
            SetColor(0.0f, (float)sugar / (float)maxResource, (float)spice / (float)maxResource);
        }

        public int GetSugar()
        {
            return sugar;
        }

        public int GetSpice()
        {
            return spice;
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

            if (spice <= (maxCellSpice - regrowthRate))
            {
                SetSpice(spice + regrowthRate);
            }
            else
            {
                SetSpice(maxCellSpice);
            }
        }
    }
}
