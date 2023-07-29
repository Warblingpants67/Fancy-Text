using UnityEngine;

[CreateAssetMenu(fileName = "Stand Effect", menuName = "FancyText/AppearEffects/Stand")]
public class FT_Stand_AppearEffect : FancyTextAppearEffect
{
    public override void ApplyAppearEffect(ref CharacterMesh charVerts, float percent)
    {

        charVerts.vertices[1].y = Mathf.Lerp(charVerts.origVerts[0].y, charVerts.origVerts[1].y, percent);
        charVerts.vertices[2].y = Mathf.Lerp(charVerts.origVerts[3].y, charVerts.origVerts[2].y, percent);

        charVerts.vertices[0] = charVerts.origVerts[0];
        charVerts.vertices[3] = charVerts.origVerts[3];
    }
}
