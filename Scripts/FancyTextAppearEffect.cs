using UnityEngine;

namespace FancyText
{
    public abstract class FancyTextAppearEffect : ScriptableObject
    {
        [Tooltip("The name you will use in tags to use this effect")]
        [SerializeField] public string name;

        public virtual void ApplyAppearEffect(ref CharacterMesh charVerts, float percent)
        {
            // Base effect is nothing, to simply appear
        }
    }
}