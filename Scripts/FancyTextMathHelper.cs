using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FancyText
{
    public class FancyTextMathHelper : MonoBehaviour
    {
        public static Vector3 RotateVector3In2DSpace(Vector3 original, Vector3 origin, float degrees)
        {
            Vector3 relativePos = original - origin;

            float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
            float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

            return new Vector3(relativePos.x * cos - relativePos.y * sin, relativePos.x * sin + relativePos.y * cos, relativePos.z) + origin;
        }
    }
}
