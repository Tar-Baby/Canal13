using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public class TVShowCharacter
{
    [Header("Character Info")]
    public string characterName;
    public string role; // "Host", "Co-Host", "Guest", etc.
    
    [Header("Visual Elements")]
    public Sprite characterSprite;
    public Sprite[] emotionalStates; // [0] Normal, [1] Happy, [2] Confused/Nervous, [3] Excited
    public Color nameTagColor = Color.white;
    
    [Header("Animation")]
    public Vector3 defaultPosition;
    public AnimationCurve entranceAnimation;
    public float animationDuration = 1f;
    
    [Header("Audio")]
    public AudioClip[] voiceClips;
    public AudioClip characterTheme;
}

public class TVShowCharacterManager : MonoBehaviour
{
    [Header("Character Setup")]
    [SerializeField] private TVShowCharacter[] showCharacters;
    
    [Header("Scene References")]
    [SerializeField] private Transform[] characterPositions; // Left, Center, Right positions
    [SerializeField] private GameObject characterPrefab;
    [SerializeField] private Canvas showCanvas;
    
    [Header("Name Tags")]
    [SerializeField] private GameObject nameTagPrefab;
    [SerializeField] private Transform nameTagContainer;
    
    [Header("Character Highlight Effects")]
    [SerializeField] private Color speakingHighlightColor = Color.yellow;
    [SerializeField] private float highlightIntensity = 1.2f;
    [SerializeField] private float highlightDuration = 0.3f;
    
    private Dictionary<string, GameObject> spawnedCharacters = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> spawnedNameTags = new Dictionary<string, GameObject>();
    private Dictionary<string, SpriteRenderer> characterRenderers = new Dictionary<string, SpriteRenderer>();
    private string currentSpeaker = "";
    
    private void Start()
    {
        // Initialize characters based on name_program story
        SetupShowCharacters();
    }
    
    private void SetupShowCharacters()
    {
        // Setup the three main characters: Lucia, Carmen, Leila
        if (showCharacters.Length >= 3)
        {
            // Lucia - Host (left position)
            SpawnCharacter(showCharacters[0], 0);
            
            // Carmen - Co-Host (center position)  
            SpawnCharacter(showCharacters[1], 1);
            
            // Leila - Guest (right position)
            SpawnCharacter(showCharacters[2], 2);
        }
        
        Debug.Log("Show characters initialized: Lucia, Carmen, Leila");
    }
    
    public void SpawnCharacter(TVShowCharacter character, int positionIndex)
    {
        if (positionIndex >= characterPositions.Length) 
        {
            Debug.LogWarning($"Position index {positionIndex} out of range!");
            return;
        }
        
        // Create character object
        GameObject characterObj = Instantiate(characterPrefab, characterPositions[positionIndex]);
        characterObj.name = $"Character_{character.characterName}";
        
        // Setup sprite renderer
        SpriteRenderer spriteRenderer = characterObj.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = characterObj.AddComponent<SpriteRenderer>();
        }
        
        spriteRenderer.sprite = character.characterSprite;
        spriteRenderer.sortingLayerName = "Characters";
        spriteRenderer.sortingOrder = positionIndex; // Left to right layering
        
        // Create name tag
        CreateNameTag(character, characterObj.transform);
        
        // Store references
        spawnedCharacters[character.characterName] = characterObj;
        characterRenderers[character.characterName] = spriteRenderer;
        
        // Set initial position and play entrance animation
        characterObj.transform.position = character.defaultPosition;
        StartCoroutine(PlayEntranceAnimation(characterObj, character));
        
        Debug.Log($"Character spawned: {character.characterName} at position {positionIndex}");
    }
    
    private void CreateNameTag(TVShowCharacter character, Transform parentTransform)
    {
        if (nameTagPrefab == null || nameTagContainer == null) return;
        
        GameObject nameTagObj = Instantiate(nameTagPrefab, nameTagContainer);
        nameTagObj.name = $"NameTag_{character.characterName}";
        
        // Position name tag below character
        RectTransform nameTagRect = nameTagObj.GetComponent<RectTransform>();
        if (nameTagRect != null)
        {
            // Convert world position to screen position
            Vector3 worldPos = parentTransform.position;
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
            
            // Convert screen position to canvas position
            RectTransform canvasRect = showCanvas.GetComponent<RectTransform>();
            Vector2 canvasPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, screenPos, showCanvas.worldCamera, out canvasPos);
            
            // Position below character
            nameTagRect.anchoredPosition = canvasPos + Vector2.down * 80f;
        }
        
        // Setup text and color
        TMPro.TextMeshProUGUI nameText = nameTagObj.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (nameText != null)
        {
            nameText.text = character.characterName;
            nameText.color = character.nameTagColor;
            nameText.fontSize = 24f;
            nameText.fontStyle = TMPro.FontStyles.Bold;
        }
        
        // Add background panel
        UnityEngine.UI.Image bgImage = nameTagObj.GetComponent<UnityEngine.UI.Image>();
        if (bgImage != null)
        {
            bgImage.color = new Color(0, 0, 0, 0.7f); // Semi-transparent black
        }
        
        spawnedNameTags[character.characterName] = nameTagObj;
    }
    
    private IEnumerator PlayEntranceAnimation(GameObject characterObj, TVShowCharacter character)
    {
        Vector3 startPos = character.defaultPosition + Vector3.down * 5f; // Start below
        Vector3 endPos = character.defaultPosition;
        
        float elapsedTime = 0;
        characterObj.transform.position = startPos;
        
        // Make sure the character starts invisible then fades in
        SpriteRenderer renderer = characterObj.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            Color startColor = renderer.color;
            startColor.a = 0f;
            renderer.color = startColor;
        }
        
        while (elapsedTime < character.animationDuration)
        {
            float t = elapsedTime / character.animationDuration;
            
            // Use animation curve if available, otherwise linear
            float curveValue = character.entranceAnimation != null ? 
                character.entranceAnimation.Evaluate(t) : t;
            
            // Animate position
            characterObj.transform.position = Vector3.Lerp(startPos, endPos, curveValue);
            
            // Animate alpha
            if (renderer != null)
            {
                Color color = renderer.color;
                color.a = Mathf.Lerp(0f, 1f, curveValue);
                renderer.color = color;
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Ensure final position and visibility
        characterObj.transform.position = endPos;
        if (renderer != null)
        {
            Color finalColor = renderer.color;
            finalColor.a = 1f;
            renderer.color = finalColor;
        }
    }
    
    public void ChangeCharacterExpression(string characterName, int expressionIndex)
    {
        if (!spawnedCharacters.ContainsKey(characterName))
        {
            Debug.LogWarning($"Character {characterName} not found!");
            return;
        }
        
        TVShowCharacter character = GetCharacterByName(characterName);
        if (character != null && expressionIndex >= 0 && expressionIndex < character.emotionalStates.Length)
        {
            SpriteRenderer spriteRenderer = characterRenderers[characterName];
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = character.emotionalStates[expressionIndex];
                
                // Add a small bounce animation for expression change
                StartCoroutine(ExpressionChangeAnimation(spawnedCharacters[characterName]));
            }
            
            Debug.Log($"Changed {characterName} expression to index {expressionIndex}");
        }
        else
        {
            Debug.LogWarning($"Invalid expression index {expressionIndex} for character {characterName}");
        }
    }
    
    private IEnumerator ExpressionChangeAnimation(GameObject characterObj)
    {
        Vector3 originalScale = characterObj.transform.localScale;
        Vector3 bounceScale = originalScale * 1.05f;
        
        float duration = 0.2f;
        float elapsedTime = 0f;
        
        // Scale up
        while (elapsedTime < duration / 2f)
        {
            float t = elapsedTime / (duration / 2f);
            characterObj.transform.localScale = Vector3.Lerp(originalScale, bounceScale, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        elapsedTime = 0f;
        
        // Scale back down
        while (elapsedTime < duration / 2f)
        {
            float t = elapsedTime / (duration / 2f);
            characterObj.transform.localScale = Vector3.Lerp(bounceScale, originalScale, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        characterObj.transform.localScale = originalScale;
    }
    
    public void PlayCharacterVoice(string characterName, int clipIndex = 0)
    {
        TVShowCharacter character = GetCharacterByName(characterName);
        if (character != null && character.voiceClips.Length > clipIndex)
        {
            AudioSource.PlayClipAtPoint(
                character.voiceClips[clipIndex], 
                Camera.main.transform.position,
                0.7f // Volume
            );
            
            Debug.Log($"Playing voice clip for {characterName}");
        }
    }
    
    private TVShowCharacter GetCharacterByName(string name)
    {
        foreach (TVShowCharacter character in showCharacters)
        {
            if (character.characterName.Equals(name, System.StringComparison.OrdinalIgnoreCase))
                return character;
        }
        
        Debug.LogWarning($"Character {name} not found in character array!");
        return null;
    }
    
    public void SetActiveCharacters(string[] characterNames)
    {
        // Hide all characters and name tags first
        foreach (var kvp in spawnedCharacters)
        {
            kvp.Value.SetActive(false);
        }
        
        foreach (var kvp in spawnedNameTags)
        {
            kvp.Value.SetActive(false);
        }
        
        // Show only specified characters
        foreach (string characterName in characterNames)
        {
            if (spawnedCharacters.ContainsKey(characterName))
            {
                spawnedCharacters[characterName].SetActive(true);
                
                if (spawnedNameTags.ContainsKey(characterName))
                {
                    spawnedNameTags[characterName].SetActive(true);
                }
            }
        }
        
        Debug.Log($"Active characters set: {string.Join(", ", characterNames)}");
    }
    
    public void OnDialogCharacterSpeak(string characterName)
    {
        // Remove highlight from previous speaker
        if (!string.IsNullOrEmpty(currentSpeaker) && currentSpeaker != characterName)
        {
            RemoveCharacterHighlight(currentSpeaker);
        }
        
        // Add highlight to current speaker
        AddCharacterHighlight(characterName);
        currentSpeaker = characterName;
        
        Debug.Log($"Character speaking: {characterName}");
    }
    
    private void AddCharacterHighlight(string characterName)
    {
        if (!spawnedCharacters.ContainsKey(characterName)) return;
        
        GameObject character = spawnedCharacters[characterName];
        
        // Add glow effect or scale animation
        StartCoroutine(SpeakingHighlightAnimation(character, true));
        
        // Make name tag more prominent
        if (spawnedNameTags.ContainsKey(characterName))
        {
            TMPro.TextMeshProUGUI nameText = spawnedNameTags[characterName].GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.color = speakingHighlightColor;
                nameText.fontSize = 28f; // Larger when speaking
            }
        }
    }
    
    private void RemoveCharacterHighlight(string characterName)
    {
        if (!spawnedCharacters.ContainsKey(characterName)) return;
        
        GameObject character = spawnedCharacters[characterName];
        
        // Remove glow effect
        StartCoroutine(SpeakingHighlightAnimation(character, false));
        
        // Restore name tag to normal
        if (spawnedNameTags.ContainsKey(characterName))
        {
            TVShowCharacter charData = GetCharacterByName(characterName);
            TMPro.TextMeshProUGUI nameText = spawnedNameTags[characterName].GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (nameText != null && charData != null)
            {
                nameText.color = charData.nameTagColor;
                nameText.fontSize = 24f; // Normal size
            }
        }
    }
    
    private IEnumerator SpeakingHighlightAnimation(GameObject character, bool highlight)
    {
        SpriteRenderer renderer = character.GetComponent<SpriteRenderer>();
        if (renderer == null) yield break;
        
        Color originalColor = renderer.color;
        Color targetColor = highlight ? 
            originalColor * highlightIntensity : 
            originalColor / highlightIntensity;
        
        Vector3 originalScale = character.transform.localScale;
        Vector3 targetScale = highlight ? 
            originalScale * 1.05f : 
            originalScale / 1.05f;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < highlightDuration)
        {
            float t = elapsedTime / highlightDuration;
            
            renderer.color = Color.Lerp(originalColor, targetColor, t);
            character.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        renderer.color = targetColor;
        character.transform.localScale = targetScale;
    }
    
    #region Character Expression Mapping for name_program
    
    // Helper methods for specific expressions based on name_program story
    public void LuciaExcited()
    {
        ChangeCharacterExpression("Lucia", 3); // Excited expression
        PlayCharacterVoice("Lucia", 0);
    }
    
    public void CarmenHappy()
    {
        ChangeCharacterExpression("Carmen", 1); // Happy expression
    }
    
    public void LeilaMysterious()
    {
        ChangeCharacterExpression("Leila", 2); // Mysterious expression
    }
    
    public void AllCharactersReactToGoodName()
    {
        ChangeCharacterExpression("Lucia", 3); // Excited
        ChangeCharacterExpression("Carmen", 1); // Happy
        ChangeCharacterExpression("Leila", 1); // Neutral/Approving
    }
    
    public void AllCharactersReactToBadChoice()
    {
        ChangeCharacterExpression("Lucia", 2); // Confused
        ChangeCharacterExpression("Carmen", 2); // Nervous
        ChangeCharacterExpression("Leila", 2); // Mysterious/Disapproving
    }
    
    #endregion
    
    #region Public Getters
    
    public bool IsCharacterActive(string characterName)
    {
        return spawnedCharacters.ContainsKey(characterName) && 
               spawnedCharacters[characterName].activeInHierarchy;
    }
    
    public Vector3 GetCharacterPosition(string characterName)
    {
        if (spawnedCharacters.ContainsKey(characterName))
        {
            return spawnedCharacters[characterName].transform.position;
        }
        return Vector3.zero;
    }
    
    public string GetCurrentSpeaker()
    {
        return currentSpeaker;
    }
    
    #endregion
}