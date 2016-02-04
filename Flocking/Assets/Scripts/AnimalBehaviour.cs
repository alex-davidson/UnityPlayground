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
    public float HuntingAwarenessSeconds = 0.8f; // Frequency of 'target' updates, ie. which consumable to chase.

    private float nextHuntingUpdate = 0;
    private Collider currentConsumable;

    public float SocialVisionRadius;
    public float SocialAwarenessSeconds = 0.8f; // Frequency of 'cluster' updates, ie. which peers are considered to be representative of the flock.
    public int SocialGroupPopulation = 6;
    public float SocialGroupRadius = 20;

    private Collider[] currentSocialGroup;
    private Collider[] closestSocialCandidates;
    private float nextSocialUpdate = 0;
    
    private const float Float_Delta = 0.0001f;


    private Transform selfTransform;
    private Rigidbody selfBody;
    

    void Start ()
    {
	    selfTransform = GetComponent<Transform>();
        selfBody = GetComponent<Rigidbody>();
        nextHuntingUpdate = Time.time;
	}

    void FixedUpdate()
    {
        // Update my incentives:
        //  * the food I'm chasing,
        //  * the members of my flock,
        //  * any flocks nearby I might join.

        if (Time.fixedTime >= nextHuntingUpdate)
        {
            UpdateHunting();
            nextHuntingUpdate += HuntingAwarenessSeconds;
        }
        if (Time.fixedTime >= nextSocialUpdate)
        {
            UpdateSocial();
            nextSocialUpdate += SocialAwarenessSeconds;
        }


        // Sum my drives towards each incentive.
        // These are weighted vectors of arbitrary magnitude.

        Vector3 drive = Vector3.zero;
        if (currentConsumable != null)
        {
            drive += GetDriveToApproachPosition(currentConsumable.transform.position);
        }



        // Convert drive into action.

        if (drive.magnitude < Float_Delta)
        {
            // My default behaviour is to gently come to a halt.
            selfBody.drag = 1;
        }
        else
        {
            selfBody.drag = 0;
            ApplyDrive(drive);
        }
    }

    private Vector3 GetDriveToApproachPosition(Vector3 position)
    {
        var translation = position - selfBody.position;
        return translation / MaximumSpeed;
    }

    private void ApplyDrive(Vector3 drive)
    {
        var drag = MaximumAcceleration / MaximumSpeed;

        // Cap acceleration magnitude.
        var acceleration = MaximumAcceleration * drive / Math.Max(1, drive.magnitude);

        var force = acceleration - (selfBody.velocity * drag);

        selfBody.AddForce(force * Time.fixedDeltaTime);
    }

    private bool IsVisible(Collider target)
    {
        return selfTransform.position.CanSee(target);
    }

    #region Hunting

    private void UpdateHunting()
    {
        if (currentConsumable == null)
        {
            // If I have no current consumable target, find the closest one in range.
            currentConsumable = GetClosestConsumableInVisionRange();
            return;
        }
        var targetSwitchAttempt = Random.value;
        if(targetSwitchAttempt < HuntingTargetSwitchChance)
        {
            // If there's a closer consumable than my current target, maybe switch to it.
            var closestConsumable = GetClosestConsumableInVisionRange();
            if(closestConsumable == null || closestConsumable == currentConsumable) return;
            var bias = Utils.GetVisionCentrednessBias(selfBody.position, selfBody.velocity, closestConsumable.transform.position, HuntingVisionAngleDegrees);
            if(targetSwitchAttempt < bias * HuntingTargetSwitchChance)
            {
                currentConsumable = closestConsumable;
                return;
            }
        }
    }
    
    private Collider GetClosestConsumableInVisionRange()
    {
        return selfTransform.position.GetTaggedObjectsInRange("Consumable", HuntingVisionRadius)
            .OrderByDistanceFrom(selfTransform.position)
            .Where(IsVisible)
            .FirstOrDefault();
    }

    #endregion

    #region Flocking

    private void UpdateSocial()
    {
        closestSocialCandidates = GetPeersInVisionRange().Take(SocialGroupPopulation).ToArray();
        currentSocialGroup = closestSocialCandidates
            .Where(s => Vector3.Distance(selfTransform.position, s.transform.position) <= SocialGroupRadius)
            .ToArray();
    }

    private IEnumerable<Collider> GetPeersInVisionRange()
    {
        return selfTransform.position.GetTaggedObjectsInRange("Consumer", SocialVisionRadius)
            .OrderByDistanceFrom(selfTransform.position)
            .Where(IsVisible);
    }
    
    #endregion
}
