using UnityEngine;
using FancyText;

[CreateAssetMenu(fileName = "Wave Effect", menuName = "FancyText/Effects/Wave")]
public class FT_Wave_Effect : FancyTextEffect
{
    [Header("Parameters")]
    [SerializeField] TextEffectParameter[] parameters;
    public override TextEffectParameter[] Parameters { get { return parameters; } }

    public override void ApplyEffect(ref CharacterMesh charVerts, float time, float[] parameters)
    {
        float speed = parameters[0];
        float strength = parameters[1];
        float stretch = parameters[2];

        Vector3 offset = new Vector3(0, Mathf.Sin((time * speed + charVerts.startIndex / 4) / stretch) * strength, 0);
        charVerts.Add(offset);
    }
}
