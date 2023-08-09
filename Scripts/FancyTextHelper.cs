using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;

namespace FancyText
{
    public static class FancyTextHelper
    {
        public static readonly string[] OtherTags = new string[] { "pause", };

        public static ParsedTextEffects GetParsedTextEffects(List<ParsedTag> parsedTags, FancyTextSettingsAsset settingsAsset)
        {
            List<CharacterEffectArea<ResolvedEffect>> updateArea = new List<CharacterEffectArea<ResolvedEffect>>();
            List<CharacterEffectArea<ResolvedEffect>> fixedUpdateArea = new List<CharacterEffectArea<ResolvedEffect>>();
            List<CharacterEffectArea<FancyTextAppearEffect>> appearArea = new List<CharacterEffectArea<FancyTextAppearEffect>>();
            List<TextAppearPause> pauses = new List<TextAppearPause>();

            for (int i = 0; i < parsedTags.Count; i++)
            {
                ParsedTag tag = parsedTags[i];

                if (tag.EffectName == OtherTags[0]) // is pause
                {
                    pauses.Add(new TextAppearPause(tag.ParsedTagStartIndex, tag.Parameters));
                }
                else
                {
                    FancyTextEffect effect = settingsAsset.GetFancyTextEffect(tag.EffectName);

                    if (effect != null) // is effect
                    {
                        CharacterEffectArea<ResolvedEffect> newEffectArea = new CharacterEffectArea<ResolvedEffect>(new ResolvedEffect(effect, parsedTags[i].Parameters), parsedTags[i].ParsedTagStartIndex, parsedTags[i].ParsedTagEndIndex);

                        if (effect.runInFixedUpdate) { fixedUpdateArea.Add(newEffectArea); }
                        else { updateArea.Add(newEffectArea); }
                    }
                    else // is appear effect
                    {
                        FancyTextAppearEffect appearEffect = settingsAsset.GetFancyTextAppearEffect(tag.EffectName);
                        appearArea.Add(new CharacterEffectArea<FancyTextAppearEffect>(appearEffect, tag.ParsedTagStartIndex, tag.ParsedTagEndIndex));
                    }
                }
            }

            return new ParsedTextEffects(updateArea, fixedUpdateArea, appearArea, pauses);
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

        public static T GetEffectFromAreaList<T>(ref List<CharacterEffectArea<T>> list, int index)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].WithinSpan(index)) { return list[i].effect; }
            }

            return default(T);
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
        public readonly FancyTextAppearEffect appearEffect;
        public readonly float appearTime;
        public float currentTime;

        public AppearingCharacter(int meshIndex, FancyTextAppearEffect appearEffect, float appearTime)
        {
            this.meshIndex = meshIndex;
            this.appearEffect = appearEffect;
            this.appearTime = appearTime;
            currentTime = 0;
        }
    }

    [System.Serializable]
    public struct CharacterEffectArea<T>
    {
        public T effect;
        public Vector2Int span;

        public CharacterEffectArea(T effect, int start, int end)
        {
            this.effect = effect;
            span = new Vector2Int(start, end);
        }

        public bool WithinSpan(int index)
        {
            return span.x <= index && span.y >= index;
        }
    }

    [System.Serializable]
    public struct ResolvedEffect
    {
        public FancyTextEffect effect;
        public TextEffectParameter[] givenParameters;
        public float[] resolvedParameters;

        public ResolvedEffect(FancyTextEffect effect, TextEffectParameter[] parameters)
        {
            this.effect = effect;
            givenParameters = parameters;
            resolvedParameters = FancyTextEffect.ResolveParameters(effect.Parameters, givenParameters);
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

    public class ParsedTextEffects
    {
        public List<CharacterEffectArea<ResolvedEffect>> updateTextEffects;
        public List<CharacterEffectArea<ResolvedEffect>> fixedUpdateTextEffects;
        public List<CharacterEffectArea<FancyTextAppearEffect>> textAppearEffects;
        public List<TextAppearPause> textPauses;

        public ParsedTextEffects(List<CharacterEffectArea<ResolvedEffect>> updateTextEffects, List<CharacterEffectArea<ResolvedEffect>> fixedUpdateTextEffects, List<CharacterEffectArea<FancyTextAppearEffect>> textAppearEffects, List<TextAppearPause> textPauses)
        {
            this.updateTextEffects = updateTextEffects;
            this.fixedUpdateTextEffects = fixedUpdateTextEffects;
            this.textAppearEffects = textAppearEffects;
            this.textPauses = textPauses;
        }
    }

    public struct TextAppearPause
    {
        public readonly int index;
        public readonly float time;

        public TextAppearPause(int index, TextEffectParameter[] parameters)
        {
            this.index = index;
            if (parameters[0].name.ToLower() == "pause") { time = parameters[0].value; }
            else { time = 1; }
        }
    }

    public class EasyStopwatch
    {
        Stopwatch stopwatch;
        string task;

        public EasyStopwatch(string task)
        {
            this.task = task;

            stopwatch = new Stopwatch();
            stopwatch.Start();
        }

        public void StopAndLog()
        {
            stopwatch.Stop();

            UnityEngine.Debug.Log($"Time to {task}: {stopwatch.ElapsedMilliseconds}ms");
        }
    }
}