using UnityEngine;

[CreateAssetMenu(fileName = "Shake Effect", menuName = "FancyText/Effects/Shake")]
public class FT_Shake_Effect : FancyTextEffect
{
    [Header("Parameters")]
    [SerializeField] TextEffectParameter[] parameters;
    public override TextEffectParameter[] Parameters { get { return parameters; } }
    [SerializeField] float shakeChance = .01f;

    const string shakeOffsetKey = "shakeOffset";

    public override void ApplyEffect(ref CharacterMesh charVerts, float time, float[] parameters)
    {
        float strength = parameters[0];

        if (Random.Range(0f, 1f) <= shakeChance) // if met shake chance
        {
            Vector3 offset = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * strength;
            charVerts.SaveVector3Data(shakeOffsetKey, new Vector3[] { offset });
            charVerts.Add(offset);
        }
        else
        {
            Vector3[] savedOffset = charVerts.GetVector3Data(shakeOffsetKey);
            if (savedOffset != null) { charVerts.Add(savedOffset[0]); }
        }
    }
}