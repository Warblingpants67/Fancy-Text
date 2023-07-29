using System.Collections;
using UnityEngine;

public abstract class FancyTextAppearEffect : ScriptableObject
{
    public virtual void ApplyAppearEffect(ref CharacterMesh charVerts, float percent)
    {
        // Base effect is nothing, to simply appear
    }
}