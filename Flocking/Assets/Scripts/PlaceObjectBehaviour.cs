﻿using UnityEngine;
using System.Collections;

public class PlaceObjectBehaviour : MonoBehaviour
{
    public GameObject Prefab;
    private Transform selfTransform;
    private float rayRadius;

    void Start()
    {
        selfTransform = GetComponent<Transform>();

        rayRadius = GetPrefabRadius(Prefab);
    }

    private static float GetPrefabRadius(GameObject prefab)
    {
        // Can apparently only get the prefab's bounds if we instantiate it:
        var actual = (GameObject) Instantiate(prefab, Vector3.zero, Quaternion.identity);
        var radius = GetObjectRadius(actual);
        Destroy(actual);
        return radius;
    }

    private static float GetObjectRadius(GameObject prefab)
    {
        if(prefab == null) return 0;
        
        var bounds = prefab.GetComponent<Collider>().bounds;
        return bounds.extents.magnitude;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // The intent is to spawn the object on our projected ray, rather than above its intersection with
            // the terrain. This is potentially expensive to calculate exactly:
            // * Ray passes close to terrain,
            // * travels through enough space to spawn,
            // * intersects again.
            // * How much work to find farthest point on ray before last intersection where sufficient space
            //   exists to spawn the object?
            // Instead we project the object along our ray and spawn it close to where its bounding sphere would
            // contact the terrain.
            var ray = new Ray(selfTransform.position, selfTransform.forward);
            RaycastHit hit;
            if(Physics.SphereCast(ray, rayRadius, out hit))
            {
                // Hit point is the intersection of the bounding sphere with the terrain. We want to spawn it
                // above the surface, ie. radius along the normal.
                CreateAt(hit.point + (hit.normal * rayRadius));
            }
        }
    }

    public void CreateAt(Vector3 worldLocation)
    {
        Instantiate(Prefab, worldLocation, Quaternion.identity);
    }
}
