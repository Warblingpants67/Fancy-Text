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

        Dictionary<string, bool> recognizedTags = new Dictionary<string, bool>();

        Dictionary<string, FancyTextAppearEffect> textAppearEffectsDict = new Dictionary<string, FancyTextAppearEffect>();
        Dictionary<string, FancyTextEffect> textEffectsDict = new Dictionary<string, FancyTextEffect>();

        public FancyTextAppearEffect GetFancyTextAppearEffect(string name)
        {
            string lowercaseName = name.ToLower();
            FancyTextAppearEffect desiredEffect = null;
            if (textAppearEffectsDict.TryGetValue(lowercaseName, out desiredEffect)) { return desiredEffect; }
            else
            {
                for (int i = 0; i < textEffects.Length; i++)
                {
                    if (textAppearEffects[i].name.ToLower() == lowercaseName)
                    {
                        textAppearEffectsDict.Add(lowercaseName, textAppearEffects[i]);
                        return textAppearEffects[i];
                    }
                }
            }

            textAppearEffectsDict.Add(lowercaseName, null);
            return desiredEffect;
        }

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

            textEffectsDict.Add(lowercaseName, null);
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

                for (int i = 0; i < FancyTextHelper.OtherTags.Length; i++)
                {
                    if (lower == FancyTextHelper.OtherTags[i].ToLower())
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
