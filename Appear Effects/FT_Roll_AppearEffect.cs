using UnityEngine;
using FancyText;

[CreateAssetMenu(fileName = "Roll Appear Effect", menuName = "FancyText/AppearEffects/Roll")]
public class FT_Roll_AppearEffect : FancyTextAppearEffect
{
    public override void ApplyAppearEffect(ref CharacterMesh charVerts, float percent)
    {
        Vector3 origin = charVerts.OriginalVerticeAveragePos;
        float degrees = (1 - percent) * 360f;

        for (int i = 0; i < 4; i++)
        {
            charVerts.vertices[i] = FancyTextMathHelper.RotateVector3In2DSpace(charVerts.origVerts[i], origin, degrees);
        }
    }
}
