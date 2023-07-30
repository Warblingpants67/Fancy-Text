using UnityEngine;

[CreateAssetMenu(fileName = "Pendulum Effect", menuName = "FancyText/Effects/Pendulum")]
public class FT_Pendulum_Effect : FancyTextEffect
{
    [Header("Parameters")]
    [SerializeField] TextEffectParameter[] parameters;
    public override TextEffectParameter[] Parameters { get { return parameters; } }

    public override void ApplyEffect(ref CharacterMesh charVerts, float time, float[] parameters)
    {
        float speed = parameters[0];
        float degrees = parameters[1];

        float sin = Mathf.Sin(time * speed);

        Vector3 origin = (charVerts.vertices[1] + charVerts.vertices[2]) / 2;

        for (int i = 0; i < 4; i++)
        {
            charVerts.vertices[i] = FancyTextMathHelper.RotateVector3In2DSpace(charVerts.vertices[i], origin, sin * degrees);
        }
    }
}
