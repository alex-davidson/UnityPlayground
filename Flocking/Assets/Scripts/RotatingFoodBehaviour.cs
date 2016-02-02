using UnityEngine;
using System.Collections;

public class RotatingFoodBehaviour : MonoBehaviour {

    Transform selfTransform;

	// Use this for initialization
	void Start()
    {
	    selfTransform = GetComponent<Transform>();
	}
	
	// Update is called once per frame
	void FixedUpdate()
    {
        var rotation = new Vector3(30, 30, 30);
	    this.selfTransform.Rotate(rotation * Time.deltaTime, Space.Self);
	}
}
