using UnityEngine;
using System.Collections;

/// <summary>
/// Look around with the mouse.  Move in the XZ plane with WASD.
/// Camera remains aligned with the XZ plane, ie. 'up' is always along the world's positive Y axis.
/// </summary>
public class PlayerCameraBehaviour : MonoBehaviour {

    private Transform viewMatrix;

    private const float BaseRotationSensitivity = 70;
    private const float BaseTranslationSensitivity = 30;

	// Use this for initialization
	void Start () {
	    viewMatrix = this.GetComponent<Transform>();
	}
	
    public float MotionSensitivity;

    public float MouseXSensitivity;
    public float MouseYSensitivity;
    public bool MouseYInvert;
    
    private float currentYaw;
    
	// Update is called once per frame
	void FixedUpdate () {
	    var interpolatedYaw = ApplyRotationAndGetInterpolatedYaw();
        ApplyTranslation(interpolatedYaw);
	}

    private void ApplyTranslation(float interpolatedYaw)
    {
        var delta = Time.deltaTime * BaseTranslationSensitivity * MotionSensitivity;
        var globalXZAlignedForwardBackwardAxis = Quaternion.Euler(0, interpolatedYaw, 0) * Vector3.forward;

        var leftRight = Input.GetAxis("Horizontal") * delta;
        var forwardBackward = Input.GetAxis("Vertical") * delta;
        
        var localLeftRight = new Vector3(leftRight, 0);
        var globalForwardBackward =  globalXZAlignedForwardBackwardAxis * forwardBackward;

        viewMatrix.Translate(localLeftRight, Space.Self); 
        viewMatrix.Translate(globalForwardBackward, Space.World); 
    }

    private float ApplyRotationAndGetInterpolatedYaw()
    {
        var delta = Time.deltaTime * BaseRotationSensitivity;

        var yaw = Input.GetAxis("Mouse X") * delta * MouseXSensitivity;
        var pitch = Input.GetAxis("Mouse Y") * delta * -MouseYSensitivity;
        if(MouseYInvert) pitch = -pitch;

        var localPitch = new Vector3(pitch, 0);
        var globalYaw = new Vector3(0, yaw);
        
        viewMatrix.Rotate(localPitch, Space.Self);
        viewMatrix.Rotate(globalYaw, Space.World);

        currentYaw += yaw;
        // To deal with variable framerate, take the midpoint of our yaw.
        return currentYaw - yaw/2;
    }
}
