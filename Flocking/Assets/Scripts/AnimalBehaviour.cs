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
    public float SocialAwarenessSeconds = 0.4f; // Frequency of 'cluster' updates, ie. which peers are considered to be representative of the flock.
    public int SocialGroupPopulation = 6;
    public float SocialGroupRadius = 16;
    public float PersonalSpace = 4;
    
    private Collider[] currentSocialGroup;
    private Collider[] closestSocialCandidates;
    private float nextSocialUpdate = 0;

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
        if(currentSocialGroup.Length > 0)
        {
            // Flock.
            drive += GetDriveToMaintainPersonalSpaceInFlock(currentSocialGroup);
        }
        else if(closestSocialCandidates.Length > 0)
        {
            // Approach potential flocks.
            drive += GetDriveToApproachFlockCandidates(closestSocialCandidates);
        }


        // Convert drive into action.

        if (drive.magnitude < Utils.Float_Delta)
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
    
    private Vector3 GetDriveToApproachFlockCandidates(Collider[] candidates)
    {
        var total = Vector3.zero;
        foreach (var candidate in candidates)
        {
            var difference = candidate.transform.position - selfTransform.position;
            var distance = difference.magnitude - PersonalSpace;
            if(distance <= 0) continue;

            var attraction = distance / (SocialVisionRadius - PersonalSpace);
            attraction = Mathf.Sqrt(attraction);
            total += attraction * difference.normalized;
        }
        return total;
    }

    private Vector3 GetDriveToMaintainPersonalSpaceInFlock(Collider[] peers)
    {
        var total = Vector3.zero;
        foreach (var peer in peers)
        {
            var inverseDifference = selfTransform.position - peer.transform.position;
            var invasionDistance = PersonalSpace - inverseDifference.magnitude;
            if(invasionDistance <= 0) continue;

            var repulsion = invasionDistance / PersonalSpace;
            repulsion = repulsion * repulsion * 3;
            total += repulsion * inverseDifference.normalized;
        }
        return total;
    }

    #endregion
}
