using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterData", menuName = "Character/CharacterData")]
public class CharacterData : ScriptableObject
{
    public Sprite aliveSprite; // Sprite saat karakter hidup
    public Sprite deathSprite; // Sprite saat karakter mati
    public string characterName; // Nama karakter (opsional untuk UI)
}