using UnityEngine;
using FancyText;

[CreateAssetMenu(fileName = "Fade Drop In Effect", menuName = "FancyText/AppearEffects/Fade Drop In")]
public class FT_FadeDropIn_AppearEffect : FancyTextAppearEffect
{
    [SerializeField] float dropHeight = .01f;

    public override void ApplyAppearEffect(ref CharacterMesh charVerts, float percent)
    {
        float additionalY = dropHeight * (1 - percent);

        for (int i = 0; i < 4; i++)
        {
            charVerts.vertices[i].y = charVerts.origVerts[i].y + additionalY;
            charVerts.colors[i].a = percent;
        }
    }
}