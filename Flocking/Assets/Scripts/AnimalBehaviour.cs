using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;
using Random = UnityEngine.Random;

public class AnimalBehaviour : MonoBehaviour
{
    public float MaximumSpeed = 20;
    public float MaximumAcceleration = 1000;
 
    public float HuntingVisionRadius;
    public float HuntingVisionAngleDegrees = 40;
    public float HuntingTargetSwitchChance = 0.4f;
    public float HuntingAwarenessSeconds = 0.8f;
    private float nextHuntingUpdate = 0;

    private Transform selfTransform;
    private Rigidbody selfBody;

    private Collider currentConsumable;


    void Start ()
    {
	    selfTransform = GetComponent<Transform>();
        selfBody = GetComponent<Rigidbody>();
        nextHuntingUpdate = Time.time;
	}
	
    void FixedUpdate ()
    {
        // My default behaviour is to gently come to a halt.
        selfBody.drag = 1;

        if(Time.fixedTime >= nextHuntingUpdate)
        {
            UpdateHunting();
            nextHuntingUpdate += HuntingAwarenessSeconds;
        }
        
        if(currentConsumable != null)
        {
            ApproachTargetPosition(currentConsumable.transform.position);
        }
	}

    private void UpdateHunting()
    {
        if (currentConsumable == null)
        {
            // If I have no current consumable target, find the closest one in range.
            currentConsumable = GetClosestConsumable();
            return;
        }
        var targetSwitchAttempt = Random.value;
        if(targetSwitchAttempt < HuntingTargetSwitchChance)
        {
            // If there's a closer consumable than my current target, maybe switch to it.
            var closestConsumable = GetClosestConsumable();
            if(closestConsumable == null || closestConsumable == currentConsumable) return;
            var bias = Utils.GetVisionCentrednessBias(selfBody.position, selfBody.velocity, closestConsumable.transform.position, HuntingVisionAngleDegrees);
            if(targetSwitchAttempt < bias * HuntingTargetSwitchChance)
            {
                currentConsumable = closestConsumable;
                return;
            }
        }
    }
    
    private Collider GetClosestConsumable()
    {
        return GetConsumablesInRange().Where(IsVisible).FirstOrDefault();
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
        selfBody.drag = 0;
    }
    
    private bool IsVisible(Collider target)
    {
        return selfTransform.position.CanSee(target);
    }

    /// <summary>
    /// Find 'Consumable' objects in hunting vision range, ordered closest to farthest.
    /// </summary>
    private IEnumerable<Collider> GetConsumablesInRange()
    {
	    var colliders = Physics.OverlapSphere(this.selfTransform.position, HuntingVisionRadius);
        return colliders.Where(c => c.CompareTag("Consumable"))
            .OrderBy(c => Vector3.Distance(selfTransform.position, c.transform.position));
    }
}
