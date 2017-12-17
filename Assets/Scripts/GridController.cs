using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using CellResource;
using CellAgent;

namespace GridControl
{
    public class GridController : MonoBehaviour
    {

        public GameObject cellPrefab;
        private ResourceCell gridCell;

        public Toggle regrowToggle;

        private List<List<ResourceCell>> grid = new List<List<ResourceCell>>();
        private List<AgentCell> defenders = new List<AgentCell>();

        public static int width, height;
        private int agentsNumber;

        //for the default size of the cell, good square is 28x28, grid loaction
        //28*0.303 = 8,484
        private const float delta = 0.303f;
        //default square positioned at (-6.5, -4, 0)
        private const int defaultWidth = 28, defaultHeight = 28;
        private float scale;

        private const int maxSugar = 20;
        public bool regrowResources;
        public bool isPaused;

        public void ToggleRegrowResources()
        {
            regrowResources = !regrowResources;
        }

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

                    float startingResources = 0.1f + maxSugar * Mathf.Sin((float)i * Mathf.PI / (float)width) * Mathf.Cos((float)j * 0.5f * Mathf.PI / (float)width);
                    if (startingResources < 0.0f)
                    {
                        startingResources = 0.0f;
                    }

                    ResourceCell singleCell = new ResourceCell((int)startingResources, (int)startingResources, 1, Instantiate(cellPrefab, cellPosition, transform.rotation), cellPosition);

                    singleCell.cellObject.transform.localScale = new Vector3(scale, scale, 1.0f);

                    gridColumn.Add(singleCell);
                }
                grid.Add(gridColumn);
            }
        }

        private void InitializeAgents()
        {
            agentsNumber = (int)(0.027f * (float)width * (float)height);
            for (int i = 0; i < agentsNumber; i++)
            {
                Color defendersColor = new Color(1.0f, 1.0f, 0.0f);

                AgentCell singleAgent = new AgentCell(5, 10, 4, 2, 4, Instantiate(cellPrefab, Vector3.zero, transform.rotation), defendersColor);
                singleAgent.cellObject.transform.localScale = new Vector3(scale, scale, 1.0f);

                System.Random rnd = new System.Random();
                int x, y;
                do
                {
                    x = rnd.Next(width);
                    y = rnd.Next(height);
                } while (grid[x][y].isTaken);

                singleAgent.SetAgentOnGrid(ref grid, x, y);

                defenders.Add(singleAgent);
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

        private void UpdateAgents()
        {
            for (int i = 0; i < defenders.Count; i++)
            {
                defenders[i].UpdateAgent(ref grid, width, height);
            }

            for (int i = 0; i < defenders.Count; i++)
            {
                if (defenders[i].dead)
                {
                    Destroy(defenders[i].cellObject);
                    defenders.RemoveAt(i);
                }
            }
        }

        // Use this for initialization
        void Start()
        {
            SetScalingAndPosition();
            CreateGrid();
            InitializeAgents();
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
                UpdateAgents();
            }
        }

        public void StartPauseSImulation()
        {
            isPaused = !isPaused;
        }

        public void QuitToMainMenu()
        {
            SceneManager.LoadScene("MainMenu");
        }
    }

}