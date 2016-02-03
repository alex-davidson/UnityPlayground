using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AnimalBehaviour : MonoBehaviour
{
    public float HuntingVisionRadius;
    public float MaximumSpeed;
    public float MaximumAcceleration;

    private Transform selfTransform;
    private Rigidbody selfBody;

    void Start ()
    {
	    selfTransform = GetComponent<Transform>();
        selfBody = GetComponent<Rigidbody>();
	}
	
	void FixedUpdate ()
    {
	    var closestConsumable = GetConsumablesInRange().Where(IsVisible).FirstOrDefault();
        if(closestConsumable == null)
        {
            selfBody.drag = 1;
        }
        else
        {
            selfBody.drag = 0;
            ApproachTargetPosition(closestConsumable.transform.position);
        }
	}

    private void ApproachTargetPosition(Vector3 position)
    {
        var drag = MaximumAcceleration / MaximumSpeed;
        
        var translation = position - selfBody.position;
        var drive = translation / MaximumSpeed;

        // Cap acceleration magnitude.
        var acceleration = MaximumAcceleration * drive / Math.Max(1, drive.magnitude);

        var force = acceleration - (selfBody.velocity * drag);

        selfBody.AddForce(force * Time.fixedDeltaTime);
    }
    private void TryToHalt()
    {
        const float damping = 0.8f;
        selfBody.AddTorque(-selfBody.angularVelocity * Time.fixedDeltaTime * damping);
        selfBody.AddForce(-selfBody.velocity * Time.fixedDeltaTime * damping);
    }

    private bool IsVisible(Collider target)
    {
        RaycastHit hit;
        Physics.Linecast(selfTransform.position, target.transform.position, out hit);
        return hit.collider == target;
    }

    /// <summary>
    /// Find 'Consumable' objects in hunting vision range, ordered closest to farthest.
    /// </summary>
    /// <returns></returns>
    private IEnumerable<Collider> GetConsumablesInRange()
    {
	    var colliders = Physics.OverlapSphere(this.selfTransform.position, HuntingVisionRadius);
        return colliders.Where(c => c.CompareTag("Consumable"))
            .OrderBy(c => Vector3.Distance(selfTransform.position, c.transform.position));
    }
}
