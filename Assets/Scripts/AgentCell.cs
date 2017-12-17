using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CellBase;
using CellResource;

namespace CellAgent
{
    public class AgentCell : Cell
    {
        private int life; //current state of sugar, if 0 then agent dies, for simplicity life does not regenerate; sorry ants, plan your life better!
        private int capacity; //how much sugar an agent can have
        private int backpack;
        private int grab; //how much sugar an agent grabs per turn
        private int metabolism; //how much sugar an agent burns per turn
        private int range; //range of vision and motion
        public bool dead;

        private int x, y; //position described in grid indexes

        /*TO DO
        private uint attack;
        private uint defense;
        private uint tradeRate;
        */        

        public AgentCell(int _life, int _capacity, int _grab, int _metabolism, int _range, GameObject _cellObject, Color _cellColor)
        {
            life = _life;
            capacity = _capacity;
            grab = _grab;
            metabolism = _metabolism;
            range = _range;

            cellObject = _cellObject;
            cellPosition = new Vector3(0.0f, 0.0f, 0.0f);
            cellColor = _cellColor;
            dead = false;
            backpack = 0;

            cellObject.GetComponent<SpriteRenderer>().color = cellColor;
        }

        //only used for initialising agents on the grid
        public void SetAgentOnGrid(ref List<List<ResourceCell>> grid, int _x, int _y)
        {
            x = _x;
            y = _y;

            cellObject.transform.position = grid[x][y].GetCellPosition();
            grid[x][y].isTaken = true;
        }

        public struct NeighbouringCellData
        {
            public ResourceCell gridCell;
            public int _x;
            public int _y;

            public NeighbouringCellData(ResourceCell _gridCell, int __x, int __y)
            {
                gridCell = _gridCell;
                _x = __x;
                _y = __y;
            }
        };

        private void MoveAgent(ref List<List<ResourceCell>> grid, int width, int height)
        {
            //look around and gather data about environment
            int vision = 1;
            List<NeighbouringCellData> nearestEnvironment = new List<NeighbouringCellData>();

            //agent looks right
            while (vision < range && x + vision < width)
            {
                NeighbouringCellData neighbouringCell = new NeighbouringCellData ( grid[x + vision][y], x + vision, y );
                nearestEnvironment.Add(neighbouringCell);
                vision++;
            }

            vision = 1;
            //agent looks left
            while (vision < range && x - vision >= 0)
            {
                NeighbouringCellData neighbouringCell = new NeighbouringCellData(grid[x - vision][y], x - vision, y);
                nearestEnvironment.Add(neighbouringCell);
                vision++;
            }

            vision = 1;
            //agent looks up
            while (vision < range && y + vision < height)
            {
                NeighbouringCellData neighbouringCell = new NeighbouringCellData(grid[x][y + vision], x, y + vision);
                nearestEnvironment.Add(neighbouringCell);
                vision++;
            }

            vision = 1;
            //agent looks down
            while (vision < range && y - vision >= 0)
            {
                NeighbouringCellData neighbouringCell = new NeighbouringCellData(grid[x][y - vision], x, y - vision);
                nearestEnvironment.Add(neighbouringCell);
                vision++;
            }

            //eliminate taken cells
            for(int i = 0; i < nearestEnvironment.Count; i++)
            {
                if(nearestEnvironment[i].gridCell.isTaken)
                {
                    nearestEnvironment.RemoveAt(i);
                }
            }

            //find max sugar cell
            int max = 0;
            int maxIndex = 0;
            for (int i = 0; i < nearestEnvironment.Count; i++)
            {
                if(max < nearestEnvironment[i].gridCell.GetSugar())
                {
                    max = nearestEnvironment[i].gridCell.GetSugar();
                }
            }

            for (int i = 0; i < nearestEnvironment.Count; i++)
            {
                if (nearestEnvironment[i].gridCell.GetSugar() != max)
                {
                    nearestEnvironment.RemoveAt(i);
                }
            }

            System.Random rnd = new System.Random();
            maxIndex = rnd.Next(nearestEnvironment.Count);

            //move agent to a new cell on the grid
            grid[x][y].isTaken = false;
            
            x = nearestEnvironment[maxIndex]._x;
            y = nearestEnvironment[maxIndex]._y;

            grid[x][y].isTaken = true;
            cellObject.transform.position = grid[x][y].GetCellPosition();
        }

        public void UpdateAgent(ref List<List<ResourceCell>> grid, int width, int height)
        {
            //checking for agent's pulse
            if (life < 0)
            {
                dead = true;
            } else
            {
                //eating
                if(backpack > 0)
                {
                    backpack -= metabolism;
                } else
                {
                    life -= backpack;
                    life -= metabolism;
                    backpack = 0;
                }

                //getting new sugar from ground
                int currentCellSugar = grid[x][y].GetSugar();
                if (currentCellSugar > grab)
                {
                    if(capacity > (backpack + grab))
                    {
                        grid[x][y].SetSugar(currentCellSugar - grab);
                        backpack += grab;
                    } else
                    {
                        grid[x][y].SetSugar(currentCellSugar - (capacity - backpack));
                        backpack = capacity;
                    }
                    
                } else
                {
                    int tmpGrab = currentCellSugar;
                    if (capacity > (backpack + tmpGrab))
                    {
                        grid[x][y].SetSugar(currentCellSugar - tmpGrab);
                        backpack += tmpGrab;
                    }
                    else
                    {
                        grid[x][y].SetSugar(currentCellSugar - (capacity - backpack));
                        backpack = capacity;
                    }
                }

                //agent's movement
                MoveAgent(ref grid, width, height);
            }            
        }
    }
}