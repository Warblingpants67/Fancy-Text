using System.Collections.Generic;
using UnityEngine;

namespace FancyText
{
    [CreateAssetMenu(fileName = "FancyText Settings Asset", menuName = "FancyText/Settings Asset")]
    public class FancyTextSettingsAsset : ScriptableObject
    {
        [SerializeField] FancyTextAppearEffect[] textAppearEffects;
        [SerializeField] FancyTextEffect[] textEffects;

        public FancyTextAppearEffect[] TextAppearEffects { get { return textAppearEffects; } }
        public FancyTextEffect[] TextEffects { get { return textEffects; } }

        string[] OtherTags = new string[] { "Pause", };
        Dictionary<string, bool> recognizedTags = new Dictionary<string, bool>();

        Dictionary<string, FancyTextEffect> textEffectsDict = new Dictionary<string, FancyTextEffect>();

        public FancyTextEffect GetFancyTextEffect(string name)
        {
            string lowercaseName = name.ToLower();
            FancyTextEffect desiredEffect = null;
            if (textEffectsDict.TryGetValue(lowercaseName, out desiredEffect)) { return desiredEffect; }
            else
            {
                for (int i = 0; i < textEffects.Length; i++)
                {
                    if (textEffects[i].name.ToLower() == lowercaseName)
                    {
                        textEffectsDict.Add(lowercaseName, textEffects[i]);
                        return textEffects[i];
                    }
                }
            }

            Debug.LogError("Could not find text effect with name \"" + name + "\"");
            return desiredEffect;
        }

        public bool IsRecognizedTag(string tagName)
        {
            bool recognized = false;
            string lower = tagName.ToLower();

            if (recognizedTags.TryGetValue(lower, out recognized)) { return recognized; }
            else
            {
                for (int i = 0; i < textAppearEffects.Length; i++)
                {
                    if (lower == textAppearEffects[i].name.ToLower())
                    {
                        recognizedTags.Add(lower, true);
                        return true;
                    }
                }

                for (int i = 0; i < textEffects.Length; i++)
                {
                    if (lower == textEffects[i].name.ToLower())
                    {
                        recognizedTags.Add(lower, true);
                        return true;
                    }
                }

                for (int i = 0; i < OtherTags.Length; i++)
                {
                    if (lower == OtherTags[i].ToLower())
                    {
                        recognizedTags.Add(lower, true);
                        return true;
                    }
                }
            }

            recognizedTags.Add(lower, false);
            return recognized;
        }
    }
}
