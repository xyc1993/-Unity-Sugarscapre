using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CellResource;
using CellAgent;

namespace GridControl
{
    public class GridController : MonoBehaviour
    {
        public GameObject cellPrefab;

        private List<List<ResourceCell>> grid = new List<List<ResourceCell>>();
        private List<AgentCell> basicAgents = new List<AgentCell>();
        private List<AgentCell> attackerAgents = new List<AgentCell>();
        private List<AgentCell> traderAgents = new List<AgentCell>();

        public static int width = 100;
        public static int height = 100;
        public static bool regrowResources = true;
        public static bool isPaused = true;

        //for the default size of the cell prefab, good square is 28x28, grid loaction at (-6.5, -4, 0) and distance between cells 0.303
        private const float delta = 0.303f;        
        private const int defaultWidth = 28, defaultHeight = 28;
        private float scale;            

        //scaling the grid and positioning it in the centre
        private void SetScalingAndPosition()
        {
            if (width > height)
            {
                scale = ((float)defaultWidth / (float)width);
                transform.position = new Vector3(transform.position.x, transform.position.y + (0.5f * (width - height) * delta) * scale, transform.position.z);
            }
            else
            {
                scale = ((float)defaultHeight / (float)height);
                transform.position = new Vector3(transform.position.x + (0.5f * (height - width) * delta) * scale, transform.position.y, transform.position.z);
            }
        }

        private void CreateGrid()
        {
            for (int i = 0; i < width; i++)
            {
                List<ResourceCell> gridColumn = new List<ResourceCell>();
                for (int j = 0; j < height; j++)
                {
                    float dx = i * scale * delta;
                    float dy = j * scale * delta;

                    Vector3 cellPosition = new Vector3(transform.position.x + dx, transform.position.y + dy, transform.position.z);
                    
                    float startingSugar = 0.1f + ResourceCell.maxResource * Mathf.Sin((float)i * Mathf.PI / (float)width) * Mathf.Cos((float)j * 0.5f * Mathf.PI / (float)width);
                    startingSugar = (startingSugar > 0.0f) ? startingSugar : 0.0f;

                    float startingSpice = 0.1f + ResourceCell.maxResource * Mathf.Sin((float)i * Mathf.PI / (float)width) * Mathf.Sin((float)j * 0.5f * Mathf.PI / (float)width);
                    startingSpice = (startingSpice > 0.0f) ? startingSpice : 0.0f;

                    ResourceCell singleCell = new ResourceCell((int)startingSugar, (int)startingSugar, (int)startingSpice, (int)startingSpice, 1, Instantiate(cellPrefab, cellPosition, transform.rotation), cellPosition);

                    singleCell.cellObject.transform.localScale = new Vector3(scale, scale, 1.0f);

                    gridColumn.Add(singleCell);
                }
                grid.Add(gridColumn);
            }
        }

        private void InitializeAgents(ref List<AgentCell> agentsTribe, AgentCell.Tribe tribe, float agentDensity)
        {
            int agentsNumber = (int)(agentDensity * (float)width * (float)height);
            for (int i = 0; i < agentsNumber; i++)
            {
                AgentCell singleAgent = new AgentCell(5, 10, 4, 2, 4, Instantiate(cellPrefab, Vector3.zero, transform.rotation), tribe);
                singleAgent.cellObject.transform.localScale = new Vector3(scale, scale, 1.0f);

                System.Random rnd = new System.Random();
                int x, y;
                do
                {
                    x = rnd.Next(width);
                    y = rnd.Next(height);
                } while (grid[x][y].isTakenBasic || grid[x][y].isTakenAttacker || grid[x][y].isTakenTrader);

                singleAgent.SetAgentOnGrid(ref grid, x, y);

                agentsTribe.Add(singleAgent);
            }
        }

        private void RegrowGridResources()
        {
            for (int i = 0; i < grid.Count; i++)
            {
                for (int j = 0; j < grid[i].Count; j++)
                {
                    grid[i][j].RegrowResources();
                }
            }
        }

        //basicAgentList is for possible interaction for more advanced tribes
        private void UpdateAgents(ref List<AgentCell> basicAgentList, ref List<AgentCell> tradersAgentList, ref List<AgentCell> agentsTribe)
        {
            for (int i = 0; i < agentsTribe.Count; i++)
            {
                agentsTribe[i].UpdateAgent(ref basicAgentList, ref tradersAgentList, ref grid, width, height);
            }

            for (int i = agentsTribe.Count-1; i >= 0; i--)
            {
                if (agentsTribe[i].dead)
                {
                    Destroy(agentsTribe[i].cellObject);
                    agentsTribe.RemoveAt(i);
                }
            }
        }

        // Use this for initialization
        void Start()
        {
            SetScalingAndPosition();
            CreateGrid();
            InitializeAgents(ref basicAgents, AgentCell.Tribe.basic, 0.02f);
            InitializeAgents(ref attackerAgents, AgentCell.Tribe.attacker, 0.01f);
            InitializeAgents(ref traderAgents, AgentCell.Tribe.trader, 0.01f);
        }

        // Update is called once per frame
        void Update()
        {
            if (!isPaused)
            {
                if (regrowResources)
                {
                    RegrowGridResources();
                }
                UpdateAgents(ref basicAgents, ref traderAgents, ref basicAgents);
                UpdateAgents(ref basicAgents, ref traderAgents, ref attackerAgents);
                UpdateAgents(ref basicAgents, ref traderAgents, ref traderAgents);
            }
        }
    }
}