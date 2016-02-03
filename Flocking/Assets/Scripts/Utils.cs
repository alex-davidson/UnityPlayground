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
            if(prefab == null) return 0;
            var radius = MeasureObjectRadius(prefab);
            if(radius > 0) return radius; // Not actually a prefab?

            // Can apparently only get the prefab's bounds if we instantiate it:
            var actual = (GameObject) Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);
            radius = MeasureObjectRadius(actual);
            Object.Destroy(actual);
            return radius;
        }

        public static float MeasureObjectRadius(GameObject obj)
        {
            if(obj == null) return 0;
        
            var bounds = obj.GetComponent<Collider>().bounds;
            return bounds.extents.magnitude;
        }

    }
}
