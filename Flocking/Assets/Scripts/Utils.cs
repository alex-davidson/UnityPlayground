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
        /// <summary>
        /// A value small enough to be zero in most physics-related circumstances.
        /// </summary>
        public const float Float_Delta = 0.001f;

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
            if(selfDirection.magnitude < Float_Delta) return 1;

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

        public static IEnumerable<Collider> OrderByDistanceFrom(this IEnumerable<Collider> colliders, Vector3 centrePosition)
        {
             return colliders.OrderBy(c => Vector3.Distance(centrePosition, c.transform.position));
        }

        public static IEnumerable<Collider> GetTaggedObjectsInRange(this Vector3 centrePosition, string tag, float range)
        {
            return Physics.OverlapSphere(centrePosition, range).Where(c => c.CompareTag(tag));
        }
    }
}
