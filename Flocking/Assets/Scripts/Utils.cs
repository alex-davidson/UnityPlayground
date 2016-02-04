using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.Scripts
{
    public static class Utils
    {
        public static float MeasurePrefabRadius(GameObject prefab)
        {
            if (prefab == null) return 0;
            var radius = MeasureObjectRadius(prefab);
            if (radius > 0) return radius; // Not actually a prefab?

            // Can apparently only get the prefab's bounds if we instantiate it:
            var actual = (GameObject)Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);
            radius = MeasureObjectRadius(actual);
            Object.Destroy(actual);
            return radius;
        }

        public static float MeasureObjectRadius(GameObject obj)
        {
            if (obj == null) return 0;

            var bounds = obj.GetComponent<Collider>().bounds;
            return bounds.extents.magnitude;
        }

        /// <summary>
        /// Get a value in the range 0 - 1 indicating how close to the centre of the body's vision
        /// the target is, assuming that its direction of motion is the centre of its vision.
        /// If the body is not moving, it is assumed to be looking directly at the target.
        /// </summary>
        public static float GetVisionCentrednessBias(Vector3 selfPosition, Vector3 selfDirection, Vector3 targetPosition, float visionAngleDegrees)
        {
            if(selfDirection.magnitude < 0.0001f) return 1;

            // 1 = Directly in front.
            var bias = Vector3.Dot(selfDirection.normalized,
                    (targetPosition - selfPosition).normalized);
        
            // If visionBoundary is X, we want a result from 0 - 1
            var visionBoundary = Mathf.Sin(Mathf.Deg2Rad * visionAngleDegrees);
            if(bias < visionBoundary) return 0;

            return (bias - visionBoundary) / (1 - visionBoundary);
        }

        /// <summary>
        /// Returns true if a ray from the eye position strikes the target collider without
        /// hitting anything else first.
        /// </summary>
        /// <remarks>
        /// Rather crude. Does not consider the extents of the target.
        /// </remarks>
        public static bool CanSee(this Vector3 selfEyes, Collider target)
        {
            RaycastHit hit;
            Physics.Linecast(selfEyes, target.transform.position, out hit);
            return hit.collider == target;
        }
    }
}
