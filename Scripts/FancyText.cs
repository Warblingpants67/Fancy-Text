using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using UnityEngine.Events;

public class FancyText : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textComponent;
    [Header("Events")]
    [SerializeField] public UnityEvent OnNewText;
    [SerializeField] public CharacterUnityEvent OnNewCharacterDisplayed;
    [SerializeField] public UnityEvent OnTextFinishedDisplaying;
    [Header("temp")]
    [SerializeField] FancyTextAppearEffect appearEffect;
    [SerializeField] bool useAppearEffect;
    [Header("Info")]
    [SerializeField] bool textFullyDisplayed;

    public bool TextFullyDisplayed { get { return textFullyDisplayed; } }

    FancyTextSettingsAsset defualtSettings;
    Mesh originalMesh;

    // Edit Arrays
    Vector3[] editedVertices;
    Color[] editedColors;

    // Meshes
    [SerializeField] CharacterMesh[] characterMeshes;

    // Settings
    float timeBetweenCharacters = .025f;
    float characterAppearTime = .1f;

    int nonSpaceVisibleCharacters = 0;

    // Appearing characters
    List<AppearingCharacter> appearingCharacters = new List<AppearingCharacter>();

    [Header("Text effect areas")]
    [SerializeField] List<CharacterEffectArea> updateCharacterEffectAreas;
    [SerializeField] List<CharacterEffectArea> fixedUpdateCharacterEffectAreas;

    // Edited Meshes
    List<int> updateEditedMeshes = new List<int>();
    List<int> fixedUpdateEditedMeshes = new List<int>();

    [SerializeField] List<FixedUpdateCharacterMeshChange> fixedUpdateMeshChanges = new List<FixedUpdateCharacterMeshChange>();

    // Text
    string parsedText;
    string unparsedText;
    string noSpacesParsedText;

    public string ParsedText { get { return parsedText; } }
    public string UnparsedText { get { return unparsedText; } }

    // Coroutines
    Coroutine displayTextCoroutine;

    public FancyTextSettingsAsset SettingsAsset { get { return defualtSettings; } }

    private void Start()
    {
        defualtSettings = (FancyTextSettingsAsset)Resources.Load("Default FancyText Settings Asset");
        SetNewText(textComponent.text);
    }
    
    private void Update()
    {
        ResetUpdatedCharacterMeshesToOriginal();
        updateEditedMeshes.Clear();

        ApplyCharacterAppearEffects();
        ApplyCharacterEffects(false);

        ApplyFixedUpdateMeshChanges();

        ApplyCharacterMeshEditsToEditArrays();
        ApplyEditArraysToMesh();
    }

    private void FixedUpdate()
    {
        ResetUpdatedCharacterMeshesToOriginal();
        fixedUpdateEditedMeshes.Clear();
        fixedUpdateMeshChanges.Clear();

        ApplyCharacterEffects(true);

        GetFixedUpdateMeshChanges();
    }

    void ResetUpdatedCharacterMeshesToOriginal()
    {
        List<int> indexList = fixedUpdateEditedMeshes.Union(updateEditedMeshes).ToList();
        for (int i = 0; i < indexList.Count; i++)
        {
            characterMeshes[indexList[i]].ResetVerticesToOriginalVertices();
            characterMeshes[indexList[i]].ResetColorsToOriginalColors();
        }
    }
    void ApplyCharacterAppearEffects()
    {
        for (int i = appearingCharacters.Count - 1; i >= 0 ; i--)
        {
            appearingCharacters[i].currentTime += Time.deltaTime;

            float p = (appearingCharacters[i].currentTime > appearingCharacters[i].appearTime ? appearingCharacters[i].appearTime : appearingCharacters[i].currentTime) / appearingCharacters[i].appearTime;
            appearEffect.ApplyAppearEffect(ref characterMeshes[appearingCharacters[i].meshIndex], p);

            if (p == 1) { appearingCharacters.RemoveAt(i); }
        }
    }
    void ApplyCharacterEffects(bool fixedUpdate)
    {
        List<CharacterEffectArea> characterEffectAreas = fixedUpdate ? ref fixedUpdateCharacterEffectAreas : ref updateCharacterEffectAreas;
        List<int> updatedCharacterMeshList = fixedUpdate ? ref fixedUpdateEditedMeshes : ref updateEditedMeshes;

        for (int i = 0; i < characterEffectAreas.Count; i++)
        {
            CharacterEffectArea effectArea = characterEffectAreas[i];

            if (effectArea.span.x < nonSpaceVisibleCharacters) // if effect is currently visible
            {
                int max = Mathf.Min(new int[] { effectArea.span.y + 1, nonSpaceVisibleCharacters });

                for (int j = effectArea.span.x; j < max; j++)
                {
                    effectArea.effect.ApplyEffect(ref characterMeshes[j], Time.time, effectArea.resolvedParameters);

                    if (!updatedCharacterMeshList.Contains(j)) { updatedCharacterMeshList.Add(j); }
                }
            }
        }
    }
    void ApplyCharacterMeshEditsToEditArrays()
    {
        for (int i = 0; i < nonSpaceVisibleCharacters; i++)
        {
            CharacterMesh charMesh = characterMeshes[i];

            editedVertices[charMesh.startIndex] = charMesh.vertices[0];
            editedVertices[charMesh.startIndex + 1] = charMesh.vertices[1];
            editedVertices[charMesh.startIndex + 2] = charMesh.vertices[2];
            editedVertices[charMesh.startIndex + 3] = charMesh.vertices[3];

            editedColors[charMesh.startIndex] = charMesh.colors[0];
            editedColors[charMesh.startIndex + 1] = charMesh.colors[1];
            editedColors[charMesh.startIndex + 2] = charMesh.colors[2];
            editedColors[charMesh.startIndex + 3] = charMesh.colors[3];
         }
    }
    void ApplyEditArraysToMesh()
    {
        originalMesh.vertices = editedVertices;
        originalMesh.colors = editedColors;
        textComponent.canvasRenderer.SetMesh(originalMesh);
    }
    void GetFixedUpdateMeshChanges()
    {
        for (int i = 0; i < fixedUpdateEditedMeshes.Count; i++)
        {
            int meshIndex = fixedUpdateEditedMeshes[i];
            fixedUpdateMeshChanges.Add(new FixedUpdateCharacterMeshChange(meshIndex, characterMeshes[meshIndex]));
        }
    }
    void ApplyFixedUpdateMeshChanges()
    {
        for (int i = 0; i < fixedUpdateMeshChanges.Count; i++)
        {
            int meshIndex = fixedUpdateMeshChanges[i].meshIndex;
            characterMeshes[meshIndex].ApplyFixedUpdateChange(fixedUpdateMeshChanges[i]);
        }
    }

    public void SetNewText(string newText)
    {
        unparsedText = newText;
        List<ParsedTag> parsedTags = FancyTextTagParser.ParseTags(newText, defualtSettings);
        textComponent.SetText(FancyTextTagParser.RemoveTags(newText, defualtSettings));
        textComponent.ForceMeshUpdate();
        parsedText = textComponent.GetParsedText();
        noSpacesParsedText = parsedText.Replace(" ", "");

        originalMesh = textComponent.mesh;

        // Init Edit Arrays
        editedVertices = new Vector3[textComponent.mesh.vertexCount];
        editedColors = new Color[editedVertices.Length];

        // Set Effect Areas
        CreateEffectAreas(parsedTags);

        OnNewText?.Invoke();

        CreateCharacterMeshes();
        StartDisplayingText();
    }

    void CreateEffectAreas(List<ParsedTag> parsedTags)
    {
        updateCharacterEffectAreas = new List<CharacterEffectArea>();
        fixedUpdateCharacterEffectAreas = new List<CharacterEffectArea>();

        for (int i = 0; i < parsedTags.Count; i++)
        {
            FancyTextEffect effect = defualtSettings.GetFancyTextEffect(parsedTags[i].EffectName);

            if (effect != null)
            {
                CharacterEffectArea newEffectArea = new CharacterEffectArea(effect, parsedTags[i].Parameters, parsedTags[i].ParsedTagStartIndex, parsedTags[i].ParsedTagEndIndex);

                if (effect.runInFixedUpdate) { fixedUpdateCharacterEffectAreas.Add(newEffectArea); }
                else { updateCharacterEffectAreas.Add(newEffectArea); }
            }
        }
    }
    void CreateCharacterMeshes()
    {
        characterMeshes = new CharacterMesh[noSpacesParsedText.Length];
        Vector3[] cachedVertices = textComponent.mesh.vertices;
        Color[] cachedColors = textComponent.mesh.colors;

        for (int i = 0; i < characterMeshes.Length; i++)
        {
            characterMeshes[i] = new CharacterMesh(i * 4, cachedVertices, cachedColors, noSpacesParsedText[i]);
        }
    }

    public void StartDisplayingText()
    {
        updateEditedMeshes = new List<int>();
        fixedUpdateEditedMeshes = new List<int>();
        appearingCharacters = new List<AppearingCharacter>();

        if (useAppearEffect)
        {
            textFullyDisplayed = false;

            if (displayTextCoroutine != null) { StopCoroutine(displayTextCoroutine); }
            displayTextCoroutine = StartCoroutine(DisplayText());
        }
        else
        {
            textFullyDisplayed = true;
            nonSpaceVisibleCharacters = noSpacesParsedText.Length;
            OnTextFinishedDisplaying?.Invoke();
        }
    }

    IEnumerator DisplayText()
    {
        appearingCharacters = new List<AppearingCharacter>();
        nonSpaceVisibleCharacters = 0;

        for (int i = 0; i < parsedText.Length; i++)
        {
            char currentChar = parsedText[i];

            if (currentChar != ' ')
            {
                appearingCharacters.Add(new AppearingCharacter(nonSpaceVisibleCharacters, characterAppearTime));
                nonSpaceVisibleCharacters++;
            }

            OnNewCharacterDisplayed?.Invoke(currentChar);
            yield return new WaitForSeconds(timeBetweenCharacters);
        }

        textFullyDisplayed = true;
        OnTextFinishedDisplaying?.Invoke();
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
        savedColorData = new Dictionary<string,  Color[]>();
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
    public  int meshIndex;
    public Vector3[] verticeChanges;
    public  Color[] colorChanges;

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
