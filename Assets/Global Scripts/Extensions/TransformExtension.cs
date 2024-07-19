using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GlobalScripts.Extensions
{
    public static class TransformExtension
    {
        public static bool IsCloseTo(this Transform transform, Transform target, float minDistance)
        {
            return Vector3.SqrMagnitude(target.position - transform.position) <= minDistance * minDistance;
        }

        public static bool IsCloseTo(this Transform transform, Vector3 position, float minDistance)
        {
            return Vector3.SqrMagnitude(position - transform.position) <= minDistance * minDistance;
        }

        public static void SetTRS(this Transform transform, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            transform.SetPositionAndRotation(position, rotation);
            transform.localScale = scale;
        }

        public static void SetTRS(this Transform transform, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            Quaternion quaternion = Quaternion.Euler(rotation);
            transform.SetPositionAndRotation(position, quaternion);
            transform.localScale = scale;
        }

        public static void SetTRSP(this Transform transform, Vector3 position, Quaternion rotation, Vector3 scale, Transform parent = null)
        {
            transform.SetPositionAndRotation(position, rotation);
            transform.localScale = scale;
            transform.SetParent(parent);
        }

        public static void SetTRSP(this Transform transform, Vector3 position, Vector3 rotation, Vector3 scale, Transform parent = null)
        {
            Quaternion quaternion = Quaternion.Euler(rotation);
            transform.SetPositionAndRotation(position, quaternion);
            transform.localScale = scale;
            transform.SetParent(parent);
        }
    }
}
