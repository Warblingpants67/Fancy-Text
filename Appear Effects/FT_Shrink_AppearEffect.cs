using UnityEngine;
using FancyText;

[CreateAssetMenu(fileName = "Shrink Effect", menuName = "FancyText/AppearEffects/Shrink")]
public class FT_Shrink_AppearEffect : FancyTextAppearEffect
{
    [SerializeField] float additionalPercentSize = .3f;

    public override void ApplyAppearEffect(ref CharacterMesh charVerts, float percent)
    {
        Vector3 avgPos = charVerts.OriginalVerticeAveragePos;

        for (int i = 0; i < 4; i++)
        {
            charVerts.vertices[i] = Vector3.LerpUnclamped(avgPos, charVerts.origVerts[i], 1 + ((1 - percent) * additionalPercentSize));
        }
    }
}
