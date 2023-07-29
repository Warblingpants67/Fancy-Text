using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEditor;

public class FancyText : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textComponent;
    [Header("State")]
    [SerializeField] bool textFullyDisplayed;
    [Header("Events")]
    [SerializeField] CharacterUnityEvent OnNewCharacterDisplayed;
    [Header("temp")]
    [SerializeField] FancyTextAppearEffect appearEffect;

    FancyTextSettingsAsset defualtSettings;

    string currentText;
    Mesh originalMesh;

    // Edit Arrays
    Vector3[] editedVertices;
    Color[] editedColors;

    // Meshes
    CharacterMesh[] characterMeshes;

    // Settings
    float timeBetweenCharacters = .025f;
    float characterAppearTime = .09f;

    int nonSpaceVisibleCharacters = 0;

    // Appearing characters
    List<AppearingCharacter> appearingCharacters = new List<AppearingCharacter>();

    [Header("Text effect areas")]
    [SerializeField] List<CharacterEffectArea> updateCharacterEffectAreas;
    [SerializeField] List<CharacterEffectArea> fixedUpdateCharacterEffectAreas;

    private void Start()
    {
        defualtSettings = (FancyTextSettingsAsset)AssetDatabase.LoadAssetAtPath("Assets/Fancy-Text/Default FancyText Settings Asset.asset", typeof(FancyTextSettingsAsset));
        SetNewText(textComponent.text);
    }
    
    private void Update()
    {
        ApplyCharacterAppearEffects();
        ApplyCharacterEffects(false);
        ApplyCharacterMeshEditsToEditArrays();
        ApplyEditArraysToMesh();
    }

    private void FixedUpdate()
    {
        ApplyCharacterEffects(true);
    }

    void ApplyCharacterAppearEffects()
    {
        for (int i = appearingCharacters.Count - 1; i >= 0 ; i--)
        {
            int index = appearingCharacters[i].meshIndex;
            appearingCharacters[i].currentTime += Time.deltaTime;

            float p = (appearingCharacters[i].currentTime > appearingCharacters[i].appearTime ? appearingCharacters[i].appearTime : appearingCharacters[i].currentTime) / appearingCharacters[i].appearTime;
            appearEffect.ApplyAppearEffect(ref characterMeshes[index], p);

            if (!appearingCharacters[i].displayed)
            {
                textComponent.maxVisibleCharacters++;
                appearingCharacters[i].displayed = true;
            }
            else if (p == 1)
            {
                characterMeshes[index].appearing = false;
                appearingCharacters.RemoveAt(i);
            }
        }
    }
    void ApplyCharacterEffects(bool fixedUpdate)
    {
        List<CharacterEffectArea> listToUse = fixedUpdate ? ref fixedUpdateCharacterEffectAreas : ref updateCharacterEffectAreas;

        for (int i = 0; i < listToUse.Count; i++)
        {
            CharacterEffectArea effectArea = listToUse[i];

            if (effectArea.span.x < nonSpaceVisibleCharacters) // if effect is currently visible
            {
                int max = Mathf.Min(new int[] { effectArea.span.y + 1, nonSpaceVisibleCharacters });

                for (int j = effectArea.span.x; j < max; j++)
                {
                    effectArea.effect.ApplyEffect(ref characterMeshes[j], Time.time, effectArea.resolvedParameters);
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

    public void SetNewText(string newText)
    {
        textComponent.SetText(newText);
        textComponent.ForceMeshUpdate(); // mesh must be updated before you can get parsed text... TMPro why
        List<ParsedTag> parsedTags = FancyTextTagParser.ParseTags(textComponent.GetParsedText());
        textComponent.SetText(FancyTextTagParser.RemoveTags(newText, defualtSettings));
        textComponent.ForceMeshUpdate();
        currentText = textComponent.GetParsedText();

        originalMesh = textComponent.mesh;

        // Init Edit Arrays
        editedVertices = new Vector3[textComponent.mesh.vertexCount];
        editedColors = new Color[editedVertices.Length];

        // Set Effect Areas
        CreateEffectAreas(parsedTags);

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
        characterMeshes = new CharacterMesh[textComponent.mesh.vertexCount / 4];

        for (int i = 0; i < characterMeshes.Length; i++)
        {
            characterMeshes[i] = new CharacterMesh(i * 4, textComponent.mesh.vertices, textComponent.mesh.colors);
        }
    }

    public void StartDisplayingText()
    {
        textComponent.maxVisibleCharacters = 0;
        textFullyDisplayed = false;

        StartCoroutine(DisplayText());

        textFullyDisplayed = true;
    }

    IEnumerator DisplayText()
    {
        nonSpaceVisibleCharacters = 0;
        for (int i = 0; i < currentText.Length; i++)
        {
            char currentChar = currentText[i];

            if (currentChar != ' ')
            {
                characterMeshes[nonSpaceVisibleCharacters].appearing = true;
                appearingCharacters.Add(new AppearingCharacter(nonSpaceVisibleCharacters, characterAppearTime));
                nonSpaceVisibleCharacters++;
            }

            OnNewCharacterDisplayed?.Invoke(currentChar);
            yield return new WaitForSeconds(timeBetweenCharacters);
        }
    }
}

public struct CharacterMesh
{
    public int startIndex;
    public bool appearing;

    // Editable arrays
    public Vector3[] vertices;
    public Color[] colors;

    // Original Data
    public readonly Vector3[] origVerts;
    public readonly Color[] origColors;

    public Vector3 OriginalVerticeAveragePos { get { return (origVerts[0] + origVerts[1] + origVerts[2] + origVerts[3]) / 4; } }

    public CharacterMesh(int startIndex, Vector3[] allVertices, Color[] allColors)
    {
        this.startIndex = startIndex;
        appearing = false;

        vertices = new Vector3[4];
        vertices[0] = allVertices[startIndex];
        vertices[1] = allVertices[startIndex + 1];
        vertices[2] = allVertices[startIndex + 2];
        vertices[3] = allVertices[startIndex + 3];

        origVerts = new Vector3[4];
        origVerts = (Vector3[])vertices.Clone();

        colors = new Color[4];
        colors[0] = allColors[startIndex];
        colors[1] = allColors[startIndex + 1];
        colors[2] = allColors[startIndex + 2];
        colors[3] = allColors[startIndex + 3];

        origColors = new Color[4];
        origColors = (Color[])colors.Clone();
    }

    public void Add(Vector3 a)
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = vertices[i] + a;
        }
    }

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

public class AppearingCharacter
{
    public readonly int meshIndex;
    public readonly float appearTime;
    public float currentTime;
    public bool displayed;

    public AppearingCharacter(int meshIndex, float appearTime)
    {
        this.meshIndex = meshIndex; ;
        this.appearTime = appearTime;
        currentTime = 0;
        displayed = false;
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
        this.givenParameters = parameters;
        this.resolvedParameters = FancyTextEffect.ResolveParameters(effect.Parameters, givenParameters);
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
