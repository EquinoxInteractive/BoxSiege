// GamepadIconsDataXbox.cs
// Asset untuk Xbox / XInput controller icons.
// Unity menu: Assets > Create > BoxSiege > Xbox Controller Icons

using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "BoxSiege/Xbox Controller Icons", fileName = "XboxControllerIcons")]
public class GamepadIconsDataXbox : GamepadIconsData
{
    [Header("=== Face Buttons ===")]
    public Sprite ButtonA;          // buttonSouth
    public Sprite ButtonB;          // buttonEast
    public Sprite ButtonX;          // buttonWest
    public Sprite ButtonY;          // buttonNorth

    [Header("=== Shoulder and Trigger ===")]
    public Sprite LB;               // leftShoulder
    public Sprite RB;               // rightShoulder
    public Sprite LT;               // leftTrigger
    public Sprite RT;               // rightTrigger

    [Header("=== Stick Buttons ===")]
    public Sprite LS;               // leftStickButton
    public Sprite RS;               // rightStickButton

    [Header("=== System Buttons ===")]
    public Sprite MenuStart;        // start
    public Sprite ViewBack;         // select

    [Header("=== D-Pad ===")]
    public Sprite DPadUp;
    public Sprite DPadDown;
    public Sprite DPadLeft;
    public Sprite DPadRight;

    [Header("=== Left Stick Directions ===")]
    public Sprite LeftStickUp;
    public Sprite LeftStickDown;
    public Sprite LeftStickLeft;
    public Sprite LeftStickRight;
    public Sprite LeftStick;

    [Header("=== Right Stick Directions ===")]
    public Sprite RightStickUp;
    public Sprite RightStickDown;
    public Sprite RightStickLeft;
    public Sprite RightStickRight;
    public Sprite RightStick;

    [Header("=== Stick Press ===")]
    public Sprite LeftStickPress;
    public Sprite RightStickPress;

    [Header("=== Custom Extra ===")]
    [Tooltip("Tambahkan entry kustom jika ada binding yang tidak tercakup")]
    public List<CustomEntry> customEntries = new List<CustomEntry>();

    [Serializable]
    public class CustomEntry
    {
        [Tooltip("Nama control sesuai binding path. Contoh: buttonSouth, leftStick/up")]
        public string controlName;
        public Sprite icon;
    }

    private Dictionary<string, Sprite> m_Cache;

    private void BuildCache()
    {
        m_Cache = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase)
        {
            { "buttonSouth",       ButtonA         },
            { "buttonEast",        ButtonB         },
            { "buttonWest",        ButtonX         },
            { "buttonNorth",       ButtonY         },
            { "leftShoulder",      LB              },
            { "rightShoulder",     RB              },
            { "leftTrigger",       LT              },
            { "rightTrigger",      RT              },
            { "leftStickButton",   LS              },
            { "rightStickButton",  RS              },
            { "start",             MenuStart       },
            { "select",            ViewBack        },
            { "dpad/up",           DPadUp          },
            { "dpad/down",         DPadDown        },
            { "dpad/left",         DPadLeft        },
            { "dpad/right",        DPadRight       },
            { "leftStick/up",      LeftStickUp     },
            { "leftStick/down",    LeftStickDown   },
            { "leftStick/left",    LeftStickLeft   },
            { "leftStick/right",   LeftStickRight  },
            { "leftStick",         LeftStick       },
            { "rightStick/up",     RightStickUp    },
            { "rightStick/down",   RightStickDown  },
            { "rightStick/left",   RightStickLeft  },
            { "rightStick/right",  RightStickRight },
            { "rightStick",        RightStick      },

            { "leftStickPress",    LeftStickPress  },
            { "rightStickPress",   RightStickPress },
        };

        if (customEntries != null)
            foreach (var e in customEntries)
                if (!string.IsNullOrEmpty(e.controlName) && e.icon != null)
                    m_Cache[e.controlName] = e.icon;
    }

    public override Sprite GetIcon(string bindingPath)
    {
        if (string.IsNullOrEmpty(bindingPath)) return null;
        if (m_Cache == null) BuildCache();

        string ctrl = bindingPath;
        int closeAngle = bindingPath.IndexOf('>');
        if (closeAngle >= 0 && closeAngle + 1 < bindingPath.Length)
            ctrl = bindingPath.Substring(closeAngle + 1).TrimStart('/');

        Sprite result;
        if (m_Cache.TryGetValue(ctrl, out result) && result != null) return result;
        if (m_Cache.TryGetValue(bindingPath, out result) && result != null) return result;
        return null;
    }

    private void OnValidate() { m_Cache = null; }
}