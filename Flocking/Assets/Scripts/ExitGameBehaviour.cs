using UnityEngine;
using System.Collections;

/// <summary>
/// Quit the application when Escape is pressed.
/// </summary>
public class ExitGameBehaviour : MonoBehaviour
{   
	void Update ()
    {
	    if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
	}
}
