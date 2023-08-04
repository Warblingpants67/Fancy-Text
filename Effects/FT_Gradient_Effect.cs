using UnityEngine;
using FancyText;

[CreateAssetMenu(fileName = "Gradient Effect", menuName = "FancyText/Effects/Gradient")]
public class FT_Gradient_Effect : FancyTextEffect
{
    [Header("Parameters")]
    [SerializeField] TextEffectParameter[] parameters;
    public override TextEffectParameter[] Parameters { get { return parameters; } }
    [SerializeField] Gradient gradient;

    public override void ApplyEffect(ref CharacterMesh charVerts, float time, float[] parameters)
    {
        float speed = parameters[0];
        float length = parameters[1];

        float pBC = 1f / gradient.colorKeys.Length / length;
        float percent = (charVerts.startIndex / 4 * pBC + (time * speed)) % 1;
        Color lColor = gradient.Evaluate(percent);
        Color rColor = gradient.Evaluate((percent + pBC) % 1);

        charVerts.colors[0] = lColor;
        charVerts.colors[1] = lColor;
        charVerts.colors[2] = rColor;
        charVerts.colors[3] = rColor;
    }
}
