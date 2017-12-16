using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridController : MonoBehaviour {

    public GameObject cell;
    
    public uint width, height;
    
    //for the default size of the cell, good square is 28x28, grid loaction
    //28*0.303 = 8,484
    const float delta = 0.303f;
    //default square positioned at (-6.5, -4, 0)
    const uint defaultWidth = 28, defaultHeight = 28;

    private void CreateGrid()
    {
        float scale = 1.0f;
        if(width > height)
        {
            scale = ((float)defaultWidth / (float)width);
            transform.position = new Vector3(transform.position.x, transform.position.y + (0.5f * (width - height) * delta) * scale, transform.position.z);
        } else
        {
            scale = ((float)defaultHeight / (float)height);
            transform.position = new Vector3(transform.position.x + (0.5f * (height - width) * delta) * scale, transform.position.y, transform.position.z);
        }

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                float tmp1 = i * scale * delta;
                float tmp2 = j * scale * delta;

                cell.transform.localScale = new Vector3(scale, scale, 1.0f);
                
                if (i == 2 && j == 3) {
                    cell.GetComponent<SpriteRenderer>().color = new Color(0.8f, 0.1f, 0.2f);
                } else
                {
                    cell.GetComponent<SpriteRenderer>().color = new Color(0.2f, 0.8f, 0.2f);
                }
                
                Vector3 cellPosition = new Vector3(transform.position.x + tmp1, transform.position.y + tmp2, transform.position.z);
                Instantiate(cell, cellPosition, transform.rotation);                
            }            
        }
    }

	// Use this for initialization
	void Start () {
        CreateGrid();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
