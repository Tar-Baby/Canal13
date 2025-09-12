using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterSpriteDatabase", menuName = "Scriptables/Character Sprite Database")]
public class CharacterSpriteDatabase : ScriptableObject
{
    [System.Serializable]
    public class ExpressionEntry
    {
        public string expressionName; // e.g., "default", "happy", "sad"
        public Sprite sprite;
    }

    [System.Serializable]
    public class CharacterEntry
    {
        public string characterName; // Name used in Ink
        public List<ExpressionEntry> expressions = new List<ExpressionEntry>();

        public Sprite GetExpressionSprite(string expression)
        {
            // Default to "default" if specific expression not found or empty
            if (string.IsNullOrEmpty(expression)) expression = "default";

            foreach (var entry in expressions)
            {
                if (entry.expressionName.Equals(expression,
                        System.StringComparison.OrdinalIgnoreCase))
                {
                    return entry.sprite;
                }
            }
            // Fallback to "default" if the requested expression isn't found
            Debug.LogWarning(
                $"Expression '{expression}' not found for character '{characterName}'. Falling back to 'default'.");
            return GetExpressionSprite("default");
        }
    }

    public List<CharacterEntry> characters = new List<CharacterEntry>();

    public CharacterEntry GetCharacterEntry(string name)
    {
        foreach (var entry in characters)
        {
            if (entry.characterName.Equals(name,
                    System.StringComparison.OrdinalIgnoreCase))
            {
                return entry;
            }
        }
        return null;
    }
}
