// GamepadIconsData.cs - Base class
// Jangan buat asset dari class ini langsung.
// Gunakan: Assets > Create > BoxSiege > Xbox Controller Icons
//      atau: Assets > Create > BoxSiege > PS Controller Icons

using UnityEngine;

public abstract class GamepadIconsData : ScriptableObject
{
    public abstract Sprite GetIcon(string bindingPath);
}