using UnityEngine;

[CreateAssetMenu(fileName = "Pendulem Effect", menuName = "FancyText/Effects/Pendulem")]
public class FT_Pendulem_Effect : FancyTextEffect
{
    [Header("Parameters")]
    [SerializeField] TextEffectParameter[] parameters;
    public override TextEffectParameter[] Parameters { get { return parameters; } }

    public override void ApplyEffect(ref CharacterMesh charVerts, float time, float[] parameters)
    {
        float sin = Mathf.Sin(time);

        Vector3 origin = (charVerts.vertices[1] + charVerts.vertices[2]) / 2;

        for (int i = 0; i < 4; i++)
        {
            charVerts.vertices[i] = FancyTextMathHelper.RotateVector3In2DSpace(charVerts.origVerts[i], origin, sin * 60);
        }
    }
}
