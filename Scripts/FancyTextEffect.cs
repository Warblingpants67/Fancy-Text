using UnityEngine;

namespace FancyText
{
    public abstract class FancyTextEffect : ScriptableObject
    {
        public abstract TextEffectParameter[] Parameters { get; }
        [Header("Effect Settings", order = 999)]
        [Tooltip("The name you will use in tags to use this effect")]
        [SerializeField] public string name;
        [Tooltip("Whether or not the effect should run in the FixedUpdate loop, or normal Update loop. FixedUpdate is usually chosen when you need something to happen a set amount of times per second.")]
        [SerializeField] public bool runInFixedUpdate;

        public virtual void ApplyEffect(ref CharacterMesh charVerts, float time, float[] parameters)
        {
            // Base effect is nothing
        }

        public virtual Vector3[] GetOffset(float time, float[] parameters)
        {
            return null;
        }

        public static float[] ResolveParameters(TextEffectParameter[] defaults, TextEffectParameter[] given)
        {
            float[] parameters = new float[defaults.Length];

            if (given != null)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    string lowerName = defaults[i].name.ToLower();

                    int givenIndex = -1;

                    for (int j = 0; j < given.Length && givenIndex == -1; j++)
                    {
                        if (given[j].name.ToLower() == lowerName)
                        {
                            givenIndex = j;
                        }
                    }

                    if (givenIndex == -1) { parameters[i] = defaults[i].value; }
                    else { parameters[i] = given[givenIndex].value; }
                }
            }
            else
            {
                for (int i = 0; i < parameters.Length; i++) { parameters[i] = defaults[i].value; }
            }

            return parameters;
        }
    }
}