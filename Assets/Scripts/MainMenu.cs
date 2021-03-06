﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using GridControl;

public class MainMenu : MonoBehaviour
{
    public Text helpText;
    public InputField gridWidth, gridHeight;

    private void SetInputFieldValue(InputField inputField, ref int gridValue)
    {
        if (inputField.text == "")
        {
            gridValue = 100;
        }
        else
        {
            int value = int.Parse(inputField.text);
            if (value < 20)
            {
                gridValue = 20;
            }
            else if (value > 150)
            {
                gridValue = 150;
            }
            else
            {
                gridValue = value;
            }
        }
    }

    public void StartSimulation()
    {
        //setting width
        SetInputFieldValue(gridWidth, ref GridController.width);
        //setting height
        SetInputFieldValue(gridHeight, ref GridController.height);

        SceneManager.LoadScene("Simulation");
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

    public void QuitApplication()
    {        
        Application.Quit(); //for some reason this generates crash on quit

        /*this is a hack that works well
        if (!Application.isEditor)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }*/
    }
}