using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using GridControl;

public class GridUI : MonoBehaviour {

    public void ToggleRegrowResources()
    {
        GridController.regrowResources = !GridController.regrowResources;
    }

    public void StartPauseSimulation()
    {
        GridController.isPaused = !GridController.isPaused;
    }

    public void ResetSimulation()
    {
        GridController.regrowResources = true;
        GridController.isPaused = true;

        SceneManager.LoadScene("Simulation");
    }

    public void QuitToMainMenu()
    {
        GridController.regrowResources = true;
        GridController.isPaused = true;

        SceneManager.LoadScene("MainMenu");
    }
}