using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CellBase;
using CellResource;

namespace CellAgent
{
    public class AgentCell : Cell
    {
        public enum Tribe
        {
            basic,
            attacker
        };

        private int life; //current state of sugar, if 0 then agent dies, for simplicity life does not regenerate; sorry ants, plan your life better!
        private int capacity; //how much sugar an agent can have in total
        private int backpack; //how much sugar an agent have at current timestep
        private int grab; //how much sugar an agent grabs per turn
        private int metabolism; //how much sugar an agent eats per turn
        private int range; //range of vision and motion
        public bool dead;

        private int x, y; //position described in grid indexes
        private string coordinates;

        private Tribe agentTribe;
        private int attack; //implementation in progress

        /*TO DO    
        private uint tradeRate;
        */

        public AgentCell(int _life, int _capacity, int _grab, int _metabolism, int _range, GameObject _cellObject, Tribe _agentTribe)
        {
            life = _life;
            capacity = _capacity;
            grab = _grab;
            metabolism = _metabolism;
            range = _range;

            cellObject = _cellObject;
            cellPosition = new Vector3(0.0f, 0.0f, 0.0f);                        
            dead = false;
            backpack = 0;

            agentTribe = _agentTribe;
            switch(agentTribe)
            {
                case Tribe.basic:
                    SetColor(1.0f, 1.0f, 0.0f);
                    attack = 2;
                    break;
                case Tribe.attacker:
                    SetColor(1.0f, 0.0f, 0.0f);
                    attack = 3;
                    break;
                default:
                    SetColor(0.0f, 0.0f, 0.0f);
                    break;
            }
        }

        private void OccupyCell(ref List<List<ResourceCell>> grid, int _x, int _y)
        {
            switch (agentTribe)
            {
                case Tribe.basic:
                    grid[x][y].isTakenBasic = true;
                    break;
                case Tribe.attacker:
                    grid[x][y].isTakenAttacker = true;
                    break;
                default:
                    break;
            }
        }

        private void FreeCell(ref List<List<ResourceCell>> grid, int _x, int _y)
        {
            switch (agentTribe)
            {
                case Tribe.basic:
                    grid[x][y].isTakenBasic = false;
                    break;
                case Tribe.attacker:
                    grid[x][y].isTakenAttacker = false;
                    break;
                default:
                    break;
            }
        }

        //only used for initialising agents on the grid
        public void SetAgentOnGrid(ref List<List<ResourceCell>> grid, int _x, int _y)
        {
            x = _x;
            y = _y;
            coordinates = "" + x + " " + y;

            cellObject.transform.position = grid[x][y].GetCellPosition();
            OccupyCell(ref grid, x, y);
        }

        public void MoveAgentOnGrid(ref List<List<ResourceCell>> grid, int _x, int _y)
        {
            //grid[x][y].isTaken = false;
            FreeCell(ref grid, x, y);

            x = _x;
            y = _y;
            coordinates = "" + x + " " + y;

            //grid[x][y].isTaken = true;
            OccupyCell(ref grid, x, y);
            cellObject.transform.position = grid[x][y].GetCellPosition();
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

        //look around and gather data about environment
        private List<NeighbouringCellData> getDataAboutEnvironment(ref List<List<ResourceCell>> grid, int width, int height)
        {
            List<NeighbouringCellData> nearestEnvironment = new List<NeighbouringCellData>();

            int vision = 1;
            //agent looks right
            while (vision < range && x + vision < width)
            {
                NeighbouringCellData neighbouringCell = new NeighbouringCellData(grid[x + vision][y], x + vision, y);
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

            return nearestEnvironment;
        }

        private void MoveAgentToResources(ref List<List<ResourceCell>> grid, int width, int height)
        {
            List<NeighbouringCellData> nearestEnvironment = getDataAboutEnvironment(ref grid, width, height);

            //remove taken cells
            for (int i = nearestEnvironment.Count-1; i >= 0; i--)
            {
                if(nearestEnvironment[i].gridCell.isTakenBasic || nearestEnvironment[i].gridCell.isTakenAttacker)
                {
                    nearestEnvironment.RemoveAt(i);
                }
            }

            //find max sugar cell
            int max = 0;
            for (int i = 0; i < nearestEnvironment.Count; i++)
            {
                if(max < nearestEnvironment[i].gridCell.GetSugar())
                {
                    max = nearestEnvironment[i].gridCell.GetSugar();
                }
            }

            for (int i = nearestEnvironment.Count - 1; i >= 0; i--)
            {
                if (nearestEnvironment[i].gridCell.GetSugar() != max)
                {
                    nearestEnvironment.RemoveAt(i);
                }
            }
                        
            System.Random rnd = new System.Random();
            int maxIndex = rnd.Next(nearestEnvironment.Count);

            //move agent to a new cell on the grid
            MoveAgentOnGrid(ref grid, nearestEnvironment[maxIndex]._x, nearestEnvironment[maxIndex]._y);
        }

        private bool PerformAttack(ref List<AgentCell> targetList, ref List<List<ResourceCell>> grid, int width, int height)
        {
            bool attackSucceeded = false;

            List<NeighbouringCellData> nearestEnvironment = getDataAboutEnvironment(ref grid, width, height);

            int tmp = nearestEnvironment.Count;
            //remove cells that aren't targets
            for (int i = nearestEnvironment.Count-1; i >= 0; i--)
            {
                if (!nearestEnvironment[i].gridCell.isTakenBasic || nearestEnvironment[i].gridCell.isTakenAttacker)
                {
                    nearestEnvironment.RemoveAt(i);
                }
            }

            //Debug.Log("Przed: "+tmp+", Po:"+ nearestEnvironment.Count);

            //randomly select target, if there's at least one
            if (nearestEnvironment.Count > 0)
            {
                System.Random rnd = new System.Random();
                int target = rnd.Next(nearestEnvironment.Count);
                string targetCoordintates = "" + nearestEnvironment[target]._x + " " + nearestEnvironment[target]._y;

                int targetIndex = targetList.FindIndex(a => a.coordinates == targetCoordintates);
                //Debug.Log("index: "+targetIndex+" zakres: "+ targetList.Count);

                if(targetIndex == -1) //other attacker was first
                {
                    attackSucceeded = false;
                } else
                {
                    if (attack > targetList[targetIndex].life)
                    {
                        targetList[targetIndex].life -= attack;

                        if (capacity > (backpack + targetList[targetIndex].backpack))
                        {
                            backpack += targetList[targetIndex].backpack;
                        }
                        else
                        {
                            backpack = capacity;
                        }

                        MoveAgentOnGrid(ref grid, targetList[targetIndex].x, targetList[targetIndex].y);
                        attackSucceeded = true;
                    }
                    else
                    {
                        life -= targetList[targetIndex].attack;
                        targetList[targetIndex].life -= attack;

                        attackSucceeded = false;
                    }
                }
            }           

            return attackSucceeded;
        }

        public void UpdateAgent(ref List<AgentCell> basicAgentList, ref List<List<ResourceCell>> grid, int width, int height)
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

                //agent's movement/interaction                
                switch (agentTribe)
                {
                    case Tribe.basic:
                        MoveAgentToResources(ref grid, width, height);
                        break;
                    case Tribe.attacker:
                        if(!PerformAttack(ref basicAgentList, ref grid, width, height))
                        {
                            MoveAgentToResources(ref grid, width, height);
                        }                        
                        break;
                    default:                        
                        break;
                }
            }            
        }
    }
}