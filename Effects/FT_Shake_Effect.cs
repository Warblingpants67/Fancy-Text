using UnityEngine;

[CreateAssetMenu(fileName = "Shake Effect", menuName = "FancyText/Effects/Shake")]
public class FT_Shake_Effect : FancyTextEffect
{
    [Header("Parameters")]
    [SerializeField] TextEffectParameter[] parameters;
    public override TextEffectParameter[] Parameters { get { return parameters; } }
    [SerializeField] float shakeChance = .01f;

    public override void ApplyEffect(ref CharacterMesh charVerts, float time, float[] parameters)
    {
        float strength = parameters[0];

        if (Random.Range(0f, 1f) <= shakeChance) // if met shake chance
        {
            Vector3 offset = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * strength;

            if (charVerts.appearing)
            {
                charVerts.Add(offset);
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    charVerts.vertices[i] = charVerts.origVerts[i] + offset;
                }
            }
        }
    }
}
