using UnityEngine;

[CreateAssetMenu(fileName = "Grow Effect", menuName = "FancyText/AppearEffects/Grow")]
public class FT_Grow_AppearEffect : FancyTextAppearEffect
{
    public override void ApplyAppearEffect(ref CharacterMesh charVerts, float percent)
    {
        Vector3 avgPos = charVerts.OriginalVerticeAveragePos;

        for (int i = 0; i < 4; i++)
        {
            charVerts.vertices[i] = Vector3.Lerp(avgPos, charVerts.origVerts[i], percent);
        }
    }
}
