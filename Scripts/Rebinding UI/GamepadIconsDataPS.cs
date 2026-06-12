// GamepadIconsDataPS.cs
// Asset untuk PlayStation controller icons.
// Mendukung DualShock 4, DualSense, dan PS3/PC Wired GamePad (generic HID).
// Unity menu: Assets > Create > BoxSiege > PS Controller Icons
//
// CARA PENGGUNAAN untuk PS3/PC Wired GamePad:
//   Saat assign di keybind, Unity akan mendeteksi nama generic (button2, button3, dst).
//   Assign sprite PS ke field "Generic HID" sesuai dengan tombol yang kamu uji.
//   Contoh dari pengujian kamu:
//     button2 = Circle     button3 = Cross (X)
//     button4 = Square     Trigger = Triangle

using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "BoxSiege/PS Controller Icons", fileName = "PSControllerIcons")]
public class GamepadIconsDataPS : GamepadIconsData
{
    [Header("=== DualShock 4 / DualSense (properly recognized) ===")]
    [Tooltip("buttonSouth - Cross (X)")]
    public Sprite Cross;
    [Tooltip("buttonEast - Circle")]
    public Sprite Circle;
    [Tooltip("buttonWest - Square")]
    public Sprite Square;
    [Tooltip("buttonNorth - Triangle")]
    public Sprite Triangle;

    [Header("=== Shoulder and Trigger (DualShock) ===")]
    [Tooltip("leftShoulder - L1")]
    public Sprite L1;
    [Tooltip("rightShoulder - R1")]
    public Sprite R1;
    [Tooltip("leftTrigger - L2")]
    public Sprite L2;
    [Tooltip("rightTrigger - R2")]
    public Sprite R2;

    [Header("=== Stick Buttons (DualShock) ===")]
    [Tooltip("leftStickButton - L3")]
    public Sprite L3;
    [Tooltip("rightStickButton - R3")]
    public Sprite R3;

    [Header("=== System Buttons (DualShock) ===")]
    [Tooltip("start - Options / Start")]
    public Sprite Options;
    [Tooltip("select - Share / Touchpad / Select")]
    public Sprite Share;

    [Header("=== D-Pad (DualShock) ===")]
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

    [Header("=== Generic HID - PS3/PC Wired GamePad ===")]
    [Tooltip("Assign sprite sesuai tombol yang terdeteksi saat assign keybind.\n" +
             "Dari pengujian: button2=Circle, button3=Cross, button4=Square, Trigger=Triangle")]
    public Sprite HID_Button1;
    public Sprite HID_Button2;
    public Sprite HID_Button3;
    public Sprite HID_Button4;
    public Sprite HID_Button5;
    public Sprite HID_Button6;
    public Sprite HID_Button7;
    public Sprite HID_Button8;
    public Sprite HID_Button9;
    public Sprite HID_Button10;
    public Sprite HID_Button11;
    public Sprite HID_Button12;
    [Tooltip("trigger - biasanya analog trigger atau tombol tertentu pada controller generic")]
    public Sprite HID_Trigger;
    [Tooltip("stick/up - Left Stick Up generic")]
    public Sprite HID_StickUp;
    [Tooltip("stick/down - Left Stick Down generic")]
    public Sprite HID_StickDown;
    [Tooltip("stick/left - Left Stick Left generic")]
    public Sprite HID_StickLeft;
    [Tooltip("stick/right - Left Stick Right generic")]
    public Sprite HID_StickRight;
    public Sprite HID_Stick;
    [Tooltip("stick2/up - Right Stick Up generic")]
    public Sprite HID_Stick2Up;
    [Tooltip("stick2/down - Right Stick Down generic")]
    public Sprite HID_Stick2Down;
    [Tooltip("stick2/left - Right Stick Left generic")]
    public Sprite HID_Stick2Left;
    [Tooltip("stick2/right - Right Stick Right generic")]
    public Sprite HID_Stick2Right;
    public Sprite HID_Stick2;

    [Header("=== Custom Extra ===")]
    [Tooltip("Tambahkan entry kustom jika ada binding yang tidak tercakup")]
    public List<CustomEntry> customEntries = new List<CustomEntry>();

    [Serializable]
    public class CustomEntry
    {
        [Tooltip("Nama control sesuai binding path. Contoh: button5, stick/up")]
        public string controlName;
        public Sprite icon;
    }

    private Dictionary<string, Sprite> m_Cache;

    private void BuildCache()
    {
        m_Cache = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase)
        {
            // DualShock / DualSense named controls
            { "buttonSouth",       Cross           },
            { "buttonEast",        Circle          },
            { "buttonWest",        Square          },
            { "buttonNorth",       Triangle        },
            { "leftShoulder",      L1              },
            { "rightShoulder",     R1              },
            { "leftTrigger",       L2              },
            { "rightTrigger",      R2              },
            { "leftStickButton",   L3              },
            { "rightStickButton",  R3              },
            { "start",             Options         },
            { "select",            Share           },
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

            // Generic HID (PS3/PC Wired GamePad)
            { "button1",           HID_Button1     },
            { "button2",           HID_Button2     },
            { "button3",           HID_Button3     },
            { "button4",           HID_Button4     },
            { "button5",           HID_Button5     },
            { "button6",           HID_Button6     },
            { "button7",           HID_Button7     },
            { "button8",           HID_Button8     },
            { "button9",           HID_Button9     },
            { "button10",          HID_Button10    },
            { "button11",          HID_Button11    },
            { "button12",          HID_Button12    },
            { "trigger",           HID_Trigger     },
            { "stick/up",          HID_StickUp     },
            { "stick/down",        HID_StickDown   },
            { "stick/left",        HID_StickLeft   },
            { "stick/right",       HID_StickRight  },
            { "stick",             HID_Stick       },
            { "stick2/up",         HID_Stick2Up    },
            { "stick2/down",       HID_Stick2Down  },
            { "stick2/left",       HID_Stick2Left  },
            { "stick2/right",      HID_Stick2Right },
            { "stick2",            HID_Stick2      },
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