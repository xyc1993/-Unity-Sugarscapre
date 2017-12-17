using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using GridControl;

public class MainMenu : MonoBehaviour {

    public string simulation;
    public Text helpText;
    public InputField gridWidth, gridHeight;

    public void StartSimulation()
    {
        //setting width
        if(gridWidth.text == "")
        {
            GridController.width = 100;
        } else
        {
            int value = int.Parse(gridWidth.text);
            if (value < 20)
            {
                GridController.width = 20;
            } else if (value > 150)
            {
                GridController.width = 150;
            } else
            {
                GridController.width = value;
            }
        }

        //setting height
        if (gridHeight.text == "")
        {
            GridController.height = 100;
        }
        else
        {
            int value = int.Parse(gridHeight.text);
            if (value < 20)
            {
                GridController.height = 20;
            }
            else if (value > 150)
            {
                GridController.height = 150;
            }
            else
            {
                GridController.height = value;
            }
        }

        SceneManager.LoadScene(simulation);
    }

    public void ShowHideHelp()
    {
        if (helpText.IsActive())
        {
            helpText.gameObject.SetActive(false);
        } else
        {
            helpText.gameObject.SetActive(true);
        }
    }

    public void QuiteApplication()
    {
        Application.Quit();
    }
}
