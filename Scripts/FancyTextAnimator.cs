using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace FancyText
{
    public class FancyTextAnimator : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI textComponent;
        [SerializeField] FancyTextAnimationSettings animationSettings;
        [Header("Events")]
        [SerializeField] public UnityEvent OnNewText;
        [SerializeField] public CharacterUnityEvent OnNewCharacterDisplayed;
        [SerializeField] public UnityEvent OnTextFinishedDisplaying;
        [Header("Info")]
        [SerializeField] bool textFullyDisplayed;
        [SerializeField] CharacterMesh[] characterMeshes;
        [SerializeField] List<CharacterEffectArea<ResolvedEffect>> updateCharacterEffectAreas;
        [SerializeField] List<CharacterEffectArea<ResolvedEffect>> fixedUpdateCharacterEffectAreas;
        [SerializeField] List<FixedUpdateCharacterMeshChange> fixedUpdateMeshChanges = new List<FixedUpdateCharacterMeshChange>();
        [SerializeField] List<CharacterEffectArea<FancyTextAppearEffect>> appearEffectChanges;
        [SerializeField] List<TextAppearPause> textAppearPauses;

        FancyTextSettingsAsset defualtSettings;
        Mesh originalMesh;
        Vector3[] editedVertices;
        Color[] editedColors;
        int nonSpaceVisibleCharacters = 0;
        Coroutine displayTextCoroutine;

        List<AppearingCharacter> appearingCharacters = new List<AppearingCharacter>();
        List<int> updateEditedMeshes = new List<int>();
        List<int> fixedUpdateEditedMeshes = new List<int>();

        // Text
        string parsedText;
        string unparsedText;
        string noSpacesParsedText;

        public string ParsedText { get { return parsedText; } }
        public string UnparsedText { get { return unparsedText; } }
        public FancyTextSettingsAsset SettingsAsset { get { return defualtSettings; } }
        public bool TextFullyDisplayed { get { return textFullyDisplayed; } }

        private void Start()
        {
            defualtSettings = (FancyTextSettingsAsset)Resources.Load("Default FancyText Settings Asset");

            HandleNullVariables();
            SetNewText(textComponent.text);
        }
        void HandleNullVariables()
        {
            if (textComponent == null)
            {
                textComponent = GetComponent<TextMeshProUGUI>();
                if (textComponent == null) { Debug.LogError("FancyTextAnimator has no assigned text mesh pro object!"); }
            }
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
            for (int i = appearingCharacters.Count - 1; i >= 0; i--)
            {
                appearingCharacters[i].currentTime += Time.deltaTime;

                float p = (appearingCharacters[i].currentTime > appearingCharacters[i].appearTime ? appearingCharacters[i].appearTime : appearingCharacters[i].currentTime) / appearingCharacters[i].appearTime;
                appearingCharacters[i].appearEffect.ApplyAppearEffect(ref characterMeshes[appearingCharacters[i].meshIndex], p);

                if (p == 1) { appearingCharacters.RemoveAt(i); }
            }
        }
        void ApplyCharacterEffects(bool fixedUpdate)
        {
            List<CharacterEffectArea<ResolvedEffect>> characterEffectAreas = fixedUpdate ? ref fixedUpdateCharacterEffectAreas : ref updateCharacterEffectAreas;
            List<int> updatedCharacterMeshList = fixedUpdate ? ref fixedUpdateEditedMeshes : ref updateEditedMeshes;

            for (int i = 0; i < characterEffectAreas.Count; i++)
            {
                CharacterEffectArea<ResolvedEffect> effectArea = characterEffectAreas[i];

                if (effectArea.span.x < nonSpaceVisibleCharacters) // if effect is currently visible
                {
                    int max = Mathf.Min(new int[] { effectArea.span.y + 1, nonSpaceVisibleCharacters });

                    for (int j = effectArea.span.x; j < max; j++)
                    {
                        effectArea.effect.effect.ApplyEffect(ref characterMeshes[j], Time.time, effectArea.effect.resolvedParameters);

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
            EasyStopwatch wholeSW = new EasyStopwatch("set new text"); 

            unparsedText = newText;
            List<ParsedTag> parsedTags = FancyTextTagParser.ParseTags(newText, defualtSettings);
            textComponent.SetText(FancyTextTagParser.RemoveTags(newText, defualtSettings));
            textComponent.ForceMeshUpdate();
            parsedText = textComponent.GetParsedText();
            noSpacesParsedText = parsedText.Replace(" ", "");

            originalMesh = textComponent.mesh;

            // Initialize Edit Arrays
            editedVertices = new Vector3[textComponent.mesh.vertexCount];
            editedColors = new Color[editedVertices.Length];

            EasyStopwatch createAreasSW = new EasyStopwatch("create parsed text effects");
            ParsedTextEffects parsedEffects = FancyTextHelper.GetParsedTextEffects(parsedTags, defualtSettings);
            updateCharacterEffectAreas = parsedEffects.updateTextEffects;
            fixedUpdateCharacterEffectAreas = parsedEffects.fixedUpdateTextEffects;
            appearEffectChanges = parsedEffects.textAppearEffects;
            textAppearPauses = parsedEffects.textPauses;
            createAreasSW.StopAndLog();

            OnNewText?.Invoke();

            characterMeshes = FancyTextHelper.CreateCharacterMeshes(noSpacesParsedText, textComponent);
            StartDisplayingText();

            wholeSW.StopAndLog();
            Debug.Log("------------------------");
        }

        public void StartDisplayingText()
        {
            updateEditedMeshes = new List<int>();
            fixedUpdateEditedMeshes = new List<int>();
            appearingCharacters = new List<AppearingCharacter>();

            if (animationSettings.useAppearEffect)
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
                    appearingCharacters.Add(new AppearingCharacter(nonSpaceVisibleCharacters, GetAssignedAppearEffect(nonSpaceVisibleCharacters), animationSettings.characterAppearTime));
                    nonSpaceVisibleCharacters++;
                }

                OnNewCharacterDisplayed?.Invoke(currentChar);
                yield return new WaitForSeconds(animationSettings.timeBetweenCharacters + GetAdditionalCharacterDelay(i));
            }

            textFullyDisplayed = true;
            OnTextFinishedDisplaying?.Invoke();
        }

        float GetAdditionalCharacterDelay(int index)
        {
            char character = parsedText[index];
            float addTime = 0;

            addTime += animationSettings.GetCharacterDelay(character);

            return addTime;
        }

        FancyTextAppearEffect GetAssignedAppearEffect(int index)
        {
            return FancyTextHelper.GetEffectFromAreaList(ref appearEffectChanges, index) ?? animationSettings.appearEffect;
        }
    }
}