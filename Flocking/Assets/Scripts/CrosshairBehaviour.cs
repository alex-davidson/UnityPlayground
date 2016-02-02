using UnityEngine;
using System;

/// <summary>
/// Render crosshair texture in the centre of the screen.
/// </summary>
public class CrosshairBehaviour : MonoBehaviour
{   
    public Texture2D CrosshairTexture;
    /// <summary>
    /// Preferred size of crosshair as a fraction of screen size.
    /// If zero, original crosshair texture size will be used.
    /// Based on screen height currently.
    /// </summary>
    public float ScaleToScreenFraction;

    Rect position;

	// Use this for initialization
	void Start ()
    {
        var screenSize = new Vector2(Screen.width, Screen.height);
        
        var size = new Vector2(CrosshairTexture.width, CrosshairTexture.height);
        size = ScaleCrosshair(screenSize, size, ScaleToScreenFraction);

        var offset = (screenSize - size) / 2;
	    position = new Rect(offset, size);
	}

    private static Vector2 ScaleCrosshair(Vector2 screenSize, Vector2 size, float screenPercentage)
    {
        if (!(screenPercentage > 0)) return size;

        // ASSUMPTION: Landscape-orientation, ie. height is smaller than width.
        var targetHeight = screenSize.y * Math.Min(screenPercentage, 1);
        
        var scale = TryRoundToInteger(targetHeight/size.y);
        
        return size * scale;
    }

    private static float TryRoundToInteger(float scale)
    {
        var rounded = (float) Math.Round(scale);
        var error = (rounded - scale)/scale;
        if (error < 0.2)
        {
            // Within about 20%, just fudge it to a nice round number for sharpness.
            scale = rounded;
        }
        return scale;
    }

    // Update is called once per frame
	void OnGUI ()
    {
        GUI.DrawTexture(position, CrosshairTexture, ScaleMode.StretchToFill);
	}
}
