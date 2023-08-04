using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace FancyText
{
    public static class FancyTextHelper
    {
        public static List<CharacterEffectArea>[] CreateEffectAreas(List<ParsedTag> parsedTags, FancyTextSettingsAsset settingsAsset)
        {
            List<CharacterEffectArea> updateArea = new List<CharacterEffectArea>();
            List<CharacterEffectArea> fixedUpdateArea = new List<CharacterEffectArea>();

            for (int i = 0; i < parsedTags.Count; i++)
            {
                FancyTextEffect effect = settingsAsset.GetFancyTextEffect(parsedTags[i].EffectName);

                CharacterEffectArea newEffectArea = new CharacterEffectArea(effect, parsedTags[i].Parameters, parsedTags[i].ParsedTagStartIndex, parsedTags[i].ParsedTagEndIndex);

                if (effect.runInFixedUpdate) { fixedUpdateArea.Add(newEffectArea); }
                else { updateArea.Add(newEffectArea); }
            }

            return new List<CharacterEffectArea>[] { updateArea, fixedUpdateArea };
        }

        public static CharacterMesh[] CreateCharacterMeshes(string noSpacesParsedText, TextMeshProUGUI textComponent)
        {
            CharacterMesh[] characterMeshes = new CharacterMesh[noSpacesParsedText.Length];
            Vector3[] cachedVertices = textComponent.mesh.vertices;
            Color[] cachedColors = textComponent.mesh.colors;

            for (int i = 0; i < characterMeshes.Length; i++)
            {
                characterMeshes[i] = new CharacterMesh(i * 4, cachedVertices, cachedColors, noSpacesParsedText[i]);
            }

            return characterMeshes;
        }
    }

    [System.Serializable]
    public struct CharacterMesh
    {
        public readonly char character;
        [SerializeField] string name; // for easier looking in arrays in editor (for now, custom editor later?)
        public int startIndex;

        // Original data
        public readonly Vector3[] origVerts;
        public readonly Color[] origColors;

        // Editable arrays
        public Vector3[] vertices;
        public Color[] colors;

        // Saved data
        Dictionary<string, Vector3[]> savedVector3Data;
        Dictionary<string, Color[]> savedColorData;

        public Vector3 OriginalVerticeAveragePos { get { return (origVerts[0] + origVerts[1] + origVerts[2] + origVerts[3]) / 4; } }

        public CharacterMesh(int startIndex, Vector3[] allVertices, Color[] allColors, char character)
        {
            this.character = character;
            name = character.ToString();
            this.startIndex = startIndex;

            origVerts = new Vector3[4];
            origVerts[0] = allVertices[startIndex];
            origVerts[1] = allVertices[startIndex + 1];
            origVerts[2] = allVertices[startIndex + 2];
            origVerts[3] = allVertices[startIndex + 3];

            vertices = (Vector3[])origVerts.Clone();

            origColors = new Color[4];
            origColors[0] = allColors[startIndex];
            origColors[1] = allColors[startIndex + 1];
            origColors[2] = allColors[startIndex + 2];
            origColors[3] = allColors[startIndex + 3];

            colors = (Color[])origColors.Clone();

            savedVector3Data = new Dictionary<string, Vector3[]>();
            savedColorData = new Dictionary<string, Color[]>();
        }

        public void ResetVerticesToOriginalVertices() { vertices = (Vector3[])origVerts.Clone(); }
        public void ResetColorsToOriginalColors() { colors = (Color[])origColors.Clone(); }

        public void Add(Vector3 a)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = vertices[i] + a;
            }
        }
        public void Add(Vector3[] a)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = vertices[i] + a[i];
            }
        }

        public void ApplyFixedUpdateChange(FixedUpdateCharacterMeshChange mc)
        {
            for (int i = 0; i < 4; i++)
            {
                vertices[i] += mc.verticeChanges[i];
                colors[i] += mc.colorChanges[i];
            }
        }

        // Saved data
        public void SaveVector3Data(string key, Vector3[] data)
        {
            if (savedVector3Data.ContainsKey(key))
            {
                savedVector3Data[key] = data;
            }
            else { savedVector3Data.Add(key, data); }
        }
        public Vector3[] GetVector3Data(string key)
        {
            Vector3[] data = null;
            if (savedVector3Data.TryGetValue(key, out data))
            {
                return data;
            }

            return data;
        }

        public void SaveColorData(string key, Color[] data)
        {
            if (savedColorData.ContainsKey(key))
            {
                savedColorData[key] = data;
            }
            else { savedColorData.Add(key, data); }
        }
        public Color[] GetColorData(string key)
        {
            if (savedColorData.ContainsKey(key))
            {
                return savedColorData[key];
            }

            return null;
        }

        // Overrides
        public override string ToString()
        {
            string output = "[";

            for (int i = 0; i < vertices.Length; i++)
            {
                output += vertices[i];
            }

            output += "]";
            return output;
        }
    }

    [System.Serializable]
    public struct FixedUpdateCharacterMeshChange
    {
        public int meshIndex;
        public Vector3[] verticeChanges;
        public Color[] colorChanges;

        public FixedUpdateCharacterMeshChange(int index, CharacterMesh characterMesh)
        {
            meshIndex = index;

            verticeChanges = new Vector3[4];
            colorChanges = new Color[4];

            for (int i = 0; i < 4; i++)
            {
                verticeChanges[i] = characterMesh.vertices[i] - characterMesh.origVerts[i];
                colorChanges[i] = characterMesh.colors[i] - characterMesh.origColors[i];
            }
        }
    }

    public class AppearingCharacter
    {
        public readonly int meshIndex;
        public readonly float appearTime;
        public float currentTime;

        public AppearingCharacter(int meshIndex, float appearTime)
        {
            this.meshIndex = meshIndex; ;
            this.appearTime = appearTime;
            currentTime = 0;
        }
    }

    [System.Serializable]
    public struct CharacterEffectArea
    {
        public FancyTextEffect effect;
        public TextEffectParameter[] givenParameters;
        public float[] resolvedParameters;
        public Vector2Int span;

        public CharacterEffectArea(FancyTextEffect effect, TextEffectParameter[] parameters, int start, int end)
        {
            this.effect = effect;
            givenParameters = parameters;
            resolvedParameters = FancyTextEffect.ResolveParameters(effect.Parameters, givenParameters);
            span = new Vector2Int(start, end);
        }
    }

    [System.Serializable]
    public class TextEffectParameter
    {
        public string name;
        public float value;

        public TextEffectParameter(string name, float value)
        {
            this.name = name;
            this.value = value;
        }
    }

    [System.Serializable] public class CharacterUnityEvent : UnityEngine.Events.UnityEvent<char> { }

    [System.Serializable]
    public class FancyTextAnimationSettings
    {
        [Header("Appear settings")]
        public bool useAppearEffect;
        public FancyTextAppearEffect appearEffect;
        public float characterAppearTime = .12f;
        public float timeBetweenCharacters = .028f;
        [SerializeField] AdditionalTimeAfterCharacter[] additionalCharacterDelays;

        bool createdDelayDict = false;
        Dictionary<char, float> characterDelaysDict;
        void CreateCharacterDelaysDict()
        {
            characterDelaysDict = new Dictionary<char, float>();
            for (int i = 0; i < additionalCharacterDelays.Length; i++)
            {
                characterDelaysDict.Add(additionalCharacterDelays[i].character, additionalCharacterDelays[i].additionalTime);
            }
            createdDelayDict = true;
        }
        public float GetCharacterDelay(char character)
        {
            if (!createdDelayDict) { CreateCharacterDelaysDict(); }

            float delay;
            if (characterDelaysDict.TryGetValue(character, out delay)) { return delay; }
            return 0;
        }

        [System.Serializable]
        class AdditionalTimeAfterCharacter
        {
            public char character;
            public float additionalTime;
        }
    }

}