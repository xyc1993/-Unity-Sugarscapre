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
            attacker,
            trader
        };

        private int lifeSugar; //current state of sugar, if 0 then agent dies, for simplicity life does not regenerate; sorry ants, plan your life better!
        private int lifeSpice; //you need both resources (all nutrients) to live!
        private int capacity; //how much resources in a backpack an agent can have in total
        private int sugarBackpack; //how much sugar an agent have at current timestep
        private int spiceBackpack; //how much spice an agent have at current timestep
        private int grab; //how much resources an agent grabs per turn (he can grab only one type of resource per turn)
        private int metabolism; //how much resources an agent eats per turn
        private int range; //range of vision and motion
        public bool dead;

        private int x, y; //position described in grid indexes
        private string coordinates;

        private Tribe agentTribe;
        private int attack;
        private int HP; //health points

        /*TO DO    
        private uint tradeRate;
        */

        public AgentCell(int _life, int _capacity, int _grab, int _metabolism, int _range, GameObject _cellObject, Tribe _agentTribe)
        {
            lifeSugar = _life;
            lifeSpice = _life;
            capacity = _capacity;
            grab = _grab;
            metabolism = _metabolism;
            range = _range;

            cellObject = _cellObject;
            cellPosition = new Vector3(0.0f, 0.0f, 0.0f);                        
            dead = false;
            sugarBackpack = 0;
            spiceBackpack = 0;

            agentTribe = _agentTribe;
            switch(agentTribe)
            {
                case Tribe.basic:
                    SetColor(1.0f, 1.0f, 0.0f);
                    attack = 2;
                    HP = 8;
                    break;
                case Tribe.attacker:
                    SetColor(1.0f, 0.0f, 0.0f);
                    attack = 3;
                    HP = 6;
                    break;
                case Tribe.trader:
                    SetColor(1.0f, 0.0f, 1.0f);
                    attack = 1;
                    HP = 5;
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
                case Tribe.trader:
                    grid[x][y].isTakenTrader = true;
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
                case Tribe.trader:
                    grid[x][y].isTakenTrader = false;
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

        void GoToMaxSugar(ref List<List<ResourceCell>> grid, int width, int height)
        {
            List<NeighbouringCellData> nearestEnvironment = getDataAboutEnvironment(ref grid, width, height);

            //remove taken cells
            for (int i = nearestEnvironment.Count - 1; i >= 0; i--)
            {
                if (nearestEnvironment[i].gridCell.isTakenBasic || nearestEnvironment[i].gridCell.isTakenAttacker || nearestEnvironment[i].gridCell.isTakenTrader)
                {
                    nearestEnvironment.RemoveAt(i);
                }
            }

            //find max sugar cell
            int max = 0;
            for (int i = 0; i < nearestEnvironment.Count; i++)
            {
                if (max < nearestEnvironment[i].gridCell.GetSugar())
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

        void GoToMaxSpice(ref List<List<ResourceCell>> grid, int width, int height)
        {
            List<NeighbouringCellData> nearestEnvironment = getDataAboutEnvironment(ref grid, width, height);

            //remove taken cells
            for (int i = nearestEnvironment.Count - 1; i >= 0; i--)
            {
                if (nearestEnvironment[i].gridCell.isTakenBasic || nearestEnvironment[i].gridCell.isTakenAttacker || nearestEnvironment[i].gridCell.isTakenTrader)
                {
                    nearestEnvironment.RemoveAt(i);
                }
            }

            //find max spice cell
            int max = 0;
            for (int i = 0; i < nearestEnvironment.Count; i++)
            {
                if (max < nearestEnvironment[i].gridCell.GetSpice())
                {
                    max = nearestEnvironment[i].gridCell.GetSpice();
                }
            }

            for (int i = nearestEnvironment.Count - 1; i >= 0; i--)
            {
                if (nearestEnvironment[i].gridCell.GetSpice() != max)
                {
                    nearestEnvironment.RemoveAt(i);
                }
            }

            System.Random rnd = new System.Random();
            int maxIndex = rnd.Next(nearestEnvironment.Count);

            //move agent to a new cell on the grid
            MoveAgentOnGrid(ref grid, nearestEnvironment[maxIndex]._x, nearestEnvironment[maxIndex]._y);
        }

        private void MoveAgentToResources(ref List<List<ResourceCell>> grid, int width, int height)
        {
            if (sugarBackpack > 0 && spiceBackpack > 0)
            {
                if (sugarBackpack < spiceBackpack)
                {
                    GoToMaxSugar(ref grid, width, height);
                }
                else
                {
                    GoToMaxSpice(ref grid, width, height);
                }
            }
            else if (sugarBackpack <= 0 && spiceBackpack > 0)
            {
                GoToMaxSugar(ref grid, width, height);
            }
            else if (sugarBackpack > 0 && spiceBackpack <= 0)
            {
                GoToMaxSpice(ref grid, width, height);
            }
            else
            {
                if (lifeSugar < lifeSpice)
                {
                    GoToMaxSugar(ref grid, width, height);
                }
                else
                {
                    GoToMaxSpice(ref grid, width, height);
                }
            }
        }

        private bool PerformAttack(ref List<AgentCell> basicList, ref List<AgentCell> tradersList, ref List<List<ResourceCell>> grid, int width, int height)
        {
            bool attackSucceeded = false;

            List<NeighbouringCellData> nearestEnvironment = getDataAboutEnvironment(ref grid, width, height);

            int tmp = nearestEnvironment.Count;
            //remove cells that aren't targets
            for (int i = nearestEnvironment.Count - 1; i >= 0; i--)
            {
                if(nearestEnvironment[i].gridCell.isTakenAttacker || (!nearestEnvironment[i].gridCell.isTakenBasic && !nearestEnvironment[i].gridCell.isTakenAttacker && !nearestEnvironment[i].gridCell.isTakenTrader))
                {
                    nearestEnvironment.RemoveAt(i);
                }
            }

            //randomly select target, if there's at least one
            if (nearestEnvironment.Count > 0)
            {
                System.Random rnd = new System.Random();
                int target = rnd.Next(nearestEnvironment.Count);
                string targetCoordintates = "" + nearestEnvironment[target]._x + " " + nearestEnvironment[target]._y;

                List<AgentCell> victimList;

                if (nearestEnvironment[target].gridCell.isTakenBasic)
                {
                    victimList = basicList;
                } else if (nearestEnvironment[target].gridCell.isTakenTrader)
                {
                    victimList = tradersList;
                } else
                {
                    victimList = basicList;
                }                

                int targetIndex = victimList.FindIndex(a => a.coordinates == targetCoordintates);

                if(targetIndex == -1) //other attacker was first
                {
                    attackSucceeded = false;
                } else
                {
                    if (attack > victimList[targetIndex].HP)
                    {
                        victimList[targetIndex].HP -= attack;

                        if (capacity > (sugarBackpack + victimList[targetIndex].sugarBackpack))
                        {
                            sugarBackpack += victimList[targetIndex].sugarBackpack;
                        }
                        else
                        {
                            sugarBackpack = capacity;
                        }

                        if (capacity > (spiceBackpack + victimList[targetIndex].spiceBackpack))
                        {
                            spiceBackpack += victimList[targetIndex].spiceBackpack;
                        }
                        else
                        {
                            spiceBackpack = capacity;
                        }

                        MoveAgentOnGrid(ref grid, victimList[targetIndex].x, victimList[targetIndex].y);
                        attackSucceeded = true;
                    }
                    else
                    {
                        HP -= victimList[targetIndex].attack;
                        victimList[targetIndex].HP -= attack;

                        attackSucceeded = false;
                    }
                }
            }           

            return attackSucceeded;
        }

        private bool PerformTrade(ref List<AgentCell> basicList, ref List<List<ResourceCell>> grid, int width, int height)
        {
            bool tradeSuccedeed = false;

            List<NeighbouringCellData> nearestEnvironment = getDataAboutEnvironment(ref grid, width, height);

            int tmp = nearestEnvironment.Count;
            //remove cells that aren't targets
            for (int i = nearestEnvironment.Count - 1; i >= 0; i--)
            {
                if (!nearestEnvironment[i].gridCell.isTakenBasic)
                {
                    nearestEnvironment.RemoveAt(i);
                }
            }

            //randomly select target, if there's at least one
            if (nearestEnvironment.Count > 0)
            {
                System.Random rnd = new System.Random();
                int target = rnd.Next(nearestEnvironment.Count);
                string targetCoordintates = "" + nearestEnvironment[target]._x + " " + nearestEnvironment[target]._y;
                
                int targetIndex = basicList.FindIndex(a => a.coordinates == targetCoordintates);

                if (targetIndex == -1) //other attacker was first
                {
                    tradeSuccedeed = false;
                }
                else
                {
                    if(spiceBackpack < sugarBackpack) //trader wants to buy spice
                    {
                        if(basicList[targetIndex].sugarBackpack < basicList[targetIndex].spiceBackpack) //good exchange rate
                        {
                            if(sugarBackpack >= 1 && basicList[targetIndex].spiceBackpack >= 3)
                            {
                                spiceBackpack += 3; sugarBackpack -= 1;
                                basicList[targetIndex].spiceBackpack -= 3; basicList[targetIndex].sugarBackpack += 1;
                                tradeSuccedeed = true;
                            } else
                            {
                                tradeSuccedeed = false;
                            }                            
                        } else //bad exchange rate
                        {
                            if (sugarBackpack >= 2 && basicList[targetIndex].spiceBackpack >= 1)
                            {
                                spiceBackpack += 1; sugarBackpack -= 2;
                                basicList[targetIndex].spiceBackpack -= 1; basicList[targetIndex].sugarBackpack += 2;
                                tradeSuccedeed = true;
                            } else
                            {
                                tradeSuccedeed = false;
                            }

                        }
                    } else //trader wants to buy sugar
                    {
                        if (basicList[targetIndex].sugarBackpack > basicList[targetIndex].spiceBackpack) //good exchange rate
                        {
                            if (spiceBackpack >= 1 && basicList[targetIndex].sugarBackpack >= 3)
                            {
                                sugarBackpack += 3; spiceBackpack -= 1;
                                basicList[targetIndex].sugarBackpack -= 3; basicList[targetIndex].spiceBackpack += 1;
                                tradeSuccedeed = true;
                            }
                            else
                            {
                                tradeSuccedeed = false;
                            }
                        }
                        else //bad exchange rate
                        {
                            if (spiceBackpack >= 2 && basicList[targetIndex].sugarBackpack >= 1)
                            {
                                sugarBackpack += 1; spiceBackpack -= 2;
                                basicList[targetIndex].sugarBackpack -= 1; basicList[targetIndex].spiceBackpack += 2;
                                tradeSuccedeed = true;
                            }
                            else
                            {
                                tradeSuccedeed = false;
                            }

                        }
                    }
                }
            }

            return tradeSuccedeed;
        }

        //eating gathered resources
        private void Eat()
        {
            if (sugarBackpack > 0)
            {
                sugarBackpack -= metabolism;
            }
            else
            {
                //if metabolism is higher than 1, backpack could be below zero so we add those negative points to life
                lifeSugar -= sugarBackpack;
                lifeSugar -= metabolism;
                sugarBackpack = 0;
            }

            if (spiceBackpack > 0)
            {
                spiceBackpack -= metabolism;
            }
            else
            {
                lifeSpice -= spiceBackpack;
                lifeSpice -= metabolism;
                spiceBackpack = 0;
            }
        }

        private void GatherSugar(ref List<List<ResourceCell>> grid)
        {
            int currentCellSugar = grid[x][y].GetSugar();
            if (currentCellSugar > grab)
            {
                if (capacity > (sugarBackpack + grab))
                {
                    grid[x][y].SetSugar(currentCellSugar - grab);
                    sugarBackpack += grab;
                }
                else
                {
                    grid[x][y].SetSugar(currentCellSugar - (capacity - sugarBackpack));
                    sugarBackpack = capacity;
                }

            }
            else
            {
                int tmpGrab = currentCellSugar;
                if (capacity > (sugarBackpack + tmpGrab))
                {
                    grid[x][y].SetSugar(currentCellSugar - tmpGrab);
                    sugarBackpack += tmpGrab;
                }
                else
                {
                    grid[x][y].SetSugar(currentCellSugar - (capacity - sugarBackpack));
                    sugarBackpack = capacity;
                }
            }
        }

        private void GatherSpice(ref List<List<ResourceCell>> grid)
        {
            int currentCellSpice = grid[x][y].GetSpice();
            if (currentCellSpice > grab)
            {
                if (capacity > (spiceBackpack + grab))
                {
                    grid[x][y].SetSpice(currentCellSpice - grab);
                    spiceBackpack += grab;
                }
                else
                {
                    grid[x][y].SetSpice(currentCellSpice - (capacity - spiceBackpack));
                    spiceBackpack = capacity;
                }

            }
            else
            {
                int tmpGrab = currentCellSpice;
                if (capacity > (spiceBackpack + tmpGrab))
                {
                    grid[x][y].SetSpice(currentCellSpice - tmpGrab);
                    spiceBackpack += tmpGrab;
                }
                else
                {
                    grid[x][y].SetSpice(currentCellSpice - (capacity - spiceBackpack));
                    spiceBackpack = capacity;
                }
            }
        }

        //getting new resources from ground
        private void GatherResources(ref List<List<ResourceCell>> grid)
        {
            if (sugarBackpack > 0 && spiceBackpack > 0)
            {
                if(sugarBackpack < spiceBackpack)
                {
                    GatherSugar(ref grid);
                } else
                {
                    GatherSpice(ref grid);
                }
            } else if (sugarBackpack <= 0 && spiceBackpack > 0)
            {
                GatherSugar(ref grid);
            } else if (sugarBackpack > 0 && spiceBackpack <= 0)
            {
                GatherSpice(ref grid);
            } else
            {
                if(lifeSugar < lifeSpice)
                {
                    GatherSugar(ref grid);
                } else
                {
                    GatherSpice(ref grid);
                }
            }
        }

        public void UpdateAgent(ref List<AgentCell> basicAgentList, ref List<AgentCell> tradersAgentList, ref List<List<ResourceCell>> grid, int width, int height)
        {
            //checking for agent's pulse
            if (lifeSugar < 0 || HP < 0)
            {
                dead = true;
            } else
            {                
                Eat();
                GatherResources(ref grid);

                //agent's movement/interaction                
                switch (agentTribe)
                {
                    case Tribe.basic:
                        MoveAgentToResources(ref grid, width, height);
                        break;
                    case Tribe.attacker:
                        if(!PerformAttack(ref basicAgentList, ref tradersAgentList, ref grid, width, height))
                        {
                            MoveAgentToResources(ref grid, width, height);
                        }                        
                        break;
                    case Tribe.trader:
                        if (!PerformTrade(ref basicAgentList, ref grid, width, height))
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