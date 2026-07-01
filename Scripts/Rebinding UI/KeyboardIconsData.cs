// KeyboardIconsData.cs
// Asset untuk Keyboard & Mouse icons.
// Unity menu: Assets > Create > BoxSiege > Keyboard Icons

using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "BoxSiege/Keyboard Icons", fileName = "KeyboardIcons")]
public class KeyboardIconsData : GamepadIconsData
{
    // =====================================================================
    // KEYBOARD - Letter Keys
    // =====================================================================
    [Header("=== Letter Keys ===")]
    public Sprite Key_A;
    public Sprite Key_B;
    public Sprite Key_C;
    public Sprite Key_D;
    public Sprite Key_E;
    public Sprite Key_F;
    public Sprite Key_G;
    public Sprite Key_H;
    public Sprite Key_I;
    public Sprite Key_J;
    public Sprite Key_K;
    public Sprite Key_L;
    public Sprite Key_M;
    public Sprite Key_N;
    public Sprite Key_O;
    public Sprite Key_P;
    public Sprite Key_Q;
    public Sprite Key_R;
    public Sprite Key_S;
    public Sprite Key_T;
    public Sprite Key_U;
    public Sprite Key_V;
    public Sprite Key_W;
    public Sprite Key_X;
    public Sprite Key_Y;
    public Sprite Key_Z;

    // =====================================================================
    // KEYBOARD - Number Keys (top row)
    // =====================================================================
    [Header("=== Number Keys (Top Row) ===")]
    public Sprite Key_0;
    public Sprite Key_1;
    public Sprite Key_2;
    public Sprite Key_3;
    public Sprite Key_4;
    public Sprite Key_5;
    public Sprite Key_6;
    public Sprite Key_7;
    public Sprite Key_8;
    public Sprite Key_9;

    // =====================================================================
    // KEYBOARD - Function Keys
    // =====================================================================
    [Header("=== Function Keys ===")]
    public Sprite Key_F1;
    public Sprite Key_F2;
    public Sprite Key_F3;
    public Sprite Key_F4;
    public Sprite Key_F5;
    public Sprite Key_F6;
    public Sprite Key_F7;
    public Sprite Key_F8;
    public Sprite Key_F9;
    public Sprite Key_F10;
    public Sprite Key_F11;
    public Sprite Key_F12;

    // =====================================================================
    // KEYBOARD - Arrow Keys
    // =====================================================================
    [Header("=== Arrow Keys ===")]
    public Sprite Key_UpArrow;
    public Sprite Key_DownArrow;
    public Sprite Key_LeftArrow;
    public Sprite Key_RightArrow;

    // =====================================================================
    // KEYBOARD - Modifier Keys
    // =====================================================================
    [Header("=== Modifier Keys ===")]
    public Sprite Key_LeftShift;
    public Sprite Key_RightShift;
    public Sprite Key_LeftCtrl;
    public Sprite Key_RightCtrl;
    public Sprite Key_LeftAlt;
    public Sprite Key_RightAlt;
    public Sprite Key_LeftCommand;  // Mac: Cmd / Windows: Win key
    public Sprite Key_RightCommand;

    // =====================================================================
    // KEYBOARD - Special Keys
    // =====================================================================
    [Header("=== Special Keys ===")]
    public Sprite Key_Space;
    public Sprite Key_Enter;
    public Sprite Key_Backspace;
    public Sprite Key_Delete;
    public Sprite Key_Tab;
    public Sprite Key_Escape;
    public Sprite Key_CapsLock;

    // =====================================================================
    // KEYBOARD - Navigation Keys
    // =====================================================================
    [Header("=== Navigation Keys ===")]
    public Sprite Key_Insert;
    public Sprite Key_Home;
    public Sprite Key_End;
    public Sprite Key_PageUp;
    public Sprite Key_PageDown;

    // =====================================================================
    // KEYBOARD - Numpad
    // =====================================================================
    [Header("=== Numpad ===")]
    public Sprite Key_Numpad0;
    public Sprite Key_Numpad1;
    public Sprite Key_Numpad2;
    public Sprite Key_Numpad3;
    public Sprite Key_Numpad4;
    public Sprite Key_Numpad5;
    public Sprite Key_Numpad6;
    public Sprite Key_Numpad7;
    public Sprite Key_Numpad8;
    public Sprite Key_Numpad9;
    public Sprite Key_NumpadPlus;
    public Sprite Key_NumpadMinus;
    public Sprite Key_NumpadMultiply;
    public Sprite Key_NumpadDivide;
    public Sprite Key_NumpadPeriod;
    public Sprite Key_NumpadEnter;
    public Sprite Key_NumLock;

    // =====================================================================
    // KEYBOARD - Symbol / Punctuation
    // =====================================================================
    [Header("=== Symbol / Punctuation Keys ===")]
    public Sprite Key_Minus;            // -
    public Sprite Key_Equals;           // =
    public Sprite Key_LeftBracket;      // [
    public Sprite Key_RightBracket;     // ]
    public Sprite Key_Backslash;        // backslash
    public Sprite Key_Semicolon;        // ;
    public Sprite Key_Quote;            // '
    public Sprite Key_Comma;            // ,
    public Sprite Key_Period;           // .
    public Sprite Key_Slash;            // /
    public Sprite Key_Backquote;        // ` (tilde)

    // =====================================================================
    // MOUSE - Buttons
    // =====================================================================
    [Header("=== Mouse Buttons ===")]
    public Sprite Mouse_LeftButton;
    public Sprite Mouse_RightButton;
    public Sprite Mouse_MiddleButton;
    public Sprite Mouse_Button4;        // Extra mouse button (forward/back)
    public Sprite Mouse_Button5;

    // =====================================================================
    // MOUSE - Movement
    // =====================================================================
    [Header("=== Mouse Movement ===")]
    public Sprite Mouse_Move;
    public Sprite Mouse_MoveUp;
    public Sprite Mouse_MoveDown;
    public Sprite Mouse_MoveLeft;
    public Sprite Mouse_MoveRight;
    public Sprite Mouse_ScrollWheel;
    public Sprite Mouse_ScrollUp;
    public Sprite Mouse_ScrollDown;

    [Header("=== Mouse Additional Buttons ===")]
    public Sprite Back;    // alias untuk Button4
    public Sprite Forward; // alias untuk Button5


    // =====================================================================
    // CUSTOM EXTRA
    // =====================================================================
    [Header("=== Custom Extra ===")]
    [Tooltip("Tambahkan entry kustom jika ada binding yang tidak tercakup.\n" +
             "Contoh controlName: 'a', 'space', 'leftButton', 'scroll/up'")]
    public List<CustomEntry> customEntries = new List<CustomEntry>();

    [Serializable]
    public class CustomEntry
    {
        [Tooltip("Nama control sesuai binding path Unity Input System.\n" +
                 "Contoh: 'a', 'space', 'enter', 'leftButton', 'scroll/up'")]
        public string controlName;
        public Sprite icon;
    }

    // =====================================================================
    // Cache & GetIcon
    // =====================================================================

    private Dictionary<string, Sprite> m_Cache;

    private void BuildCache()
    {
        m_Cache = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase)
        {
            // --- Letters ---
            { "a", Key_A }, { "b", Key_B }, { "c", Key_C }, { "d", Key_D },
            { "e", Key_E }, { "f", Key_F }, { "g", Key_G }, { "h", Key_H },
            { "i", Key_I }, { "j", Key_J }, { "k", Key_K }, { "l", Key_L },
            { "m", Key_M }, { "n", Key_N }, { "o", Key_O }, { "p", Key_P },
            { "q", Key_Q }, { "r", Key_R }, { "s", Key_S }, { "t", Key_T },
            { "u", Key_U }, { "v", Key_V }, { "w", Key_W }, { "x", Key_X },
            { "y", Key_Y }, { "z", Key_Z },

            // --- Top Number Row ---
            { "0", Key_0 }, { "1", Key_1 }, { "2", Key_2 }, { "3", Key_3 },
            { "4", Key_4 }, { "5", Key_5 }, { "6", Key_6 }, { "7", Key_7 },
            { "8", Key_8 }, { "9", Key_9 },

            // --- Function Keys ---
            { "f1",  Key_F1  }, { "f2",  Key_F2  }, { "f3",  Key_F3  },
            { "f4",  Key_F4  }, { "f5",  Key_F5  }, { "f6",  Key_F6  },
            { "f7",  Key_F7  }, { "f8",  Key_F8  }, { "f9",  Key_F9  },
            { "f10", Key_F10 }, { "f11", Key_F11 }, { "f12", Key_F12 },

            // --- Arrow Keys ---
            { "upArrow",    Key_UpArrow    },
            { "downArrow",  Key_DownArrow  },
            { "leftArrow",  Key_LeftArrow  },
            { "rightArrow", Key_RightArrow },

            // --- Modifier Keys ---
            { "leftShift",   Key_LeftShift   },
            { "rightShift",  Key_RightShift  },
            { "leftCtrl",    Key_LeftCtrl    },
            { "rightCtrl",   Key_RightCtrl   },
            { "leftAlt",     Key_LeftAlt     },
            { "rightAlt",    Key_RightAlt    },
            { "leftCommand", Key_LeftCommand  },
            { "rightCommand",Key_RightCommand },
            { "leftMeta",    Key_LeftCommand  },  // alias
            { "rightMeta",   Key_RightCommand },  // alias
            { "leftWindows", Key_LeftCommand  },  // alias
            { "rightWindows",Key_RightCommand },  // alias

            // --- Special Keys ---
            { "space",     Key_Space     },
            { "enter",     Key_Enter     },
            { "backspace", Key_Backspace },
            { "delete",    Key_Delete    },
            { "tab",       Key_Tab       },
            { "escape",    Key_Escape    },
            { "capsLock",  Key_CapsLock  },

            // --- Navigation ---
            { "insert",   Key_Insert   },
            { "home",     Key_Home     },
            { "end",      Key_End      },
            { "pageUp",   Key_PageUp   },
            { "pageDown", Key_PageDown },

            // --- Numpad ---
            { "numpad0",        Key_Numpad0        },
            { "numpad1",        Key_Numpad1        },
            { "numpad2",        Key_Numpad2        },
            { "numpad3",        Key_Numpad3        },
            { "numpad4",        Key_Numpad4        },
            { "numpad5",        Key_Numpad5        },
            { "numpad6",        Key_Numpad6        },
            { "numpad7",        Key_Numpad7        },
            { "numpad8",        Key_Numpad8        },
            { "numpad9",        Key_Numpad9        },
            { "numpadPlus",     Key_NumpadPlus     },
            { "numpadMinus",    Key_NumpadMinus    },
            { "numpadMultiply", Key_NumpadMultiply },
            { "numpadDivide",   Key_NumpadDivide   },
            { "numpadPeriod",   Key_NumpadPeriod   },
            { "numpadEnter",    Key_NumpadEnter    },
            { "numLock",        Key_NumLock        },

            // --- Symbol / Punctuation ---
            { "minus",        Key_Minus        },
            { "equals",       Key_Equals       },
            { "leftBracket",  Key_LeftBracket  },
            { "rightBracket", Key_RightBracket },
            { "backslash",    Key_Backslash    },
            { "semicolon",    Key_Semicolon    },
            { "quote",        Key_Quote        },
            { "comma",        Key_Comma        },
            { "period",       Key_Period       },
            { "slash",        Key_Slash        },
            { "backquote",    Key_Backquote    },

            // --- Mouse Buttons ---
            { "leftButton",   Mouse_LeftButton   },
            { "rightButton",  Mouse_RightButton  },
            { "middleButton", Mouse_MiddleButton },
            { "button4",      Mouse_Button4      },
            { "button5",      Mouse_Button5      },

            // --- Mouse Movement ---
            { "delta",          Mouse_Move        },
            { "delta/up",       Mouse_MoveUp      },
            { "delta/down",     Mouse_MoveDown    },
            { "delta/left",     Mouse_MoveLeft    },
            { "delta/right",    Mouse_MoveRight   },
            { "scroll",         Mouse_ScrollWheel },
            { "scroll/up",      Mouse_ScrollUp    },
            { "scroll/down",    Mouse_ScrollDown  },
            { "scroll/y",       Mouse_ScrollWheel },

            { "back",           Mouse_Button4 },
            { "forward",        Mouse_Button5 },
        };

        // Custom entries (override atau tambah)
        if (customEntries != null)
            foreach (var e in customEntries)
                if (!string.IsNullOrEmpty(e.controlName) && e.icon != null)
                    m_Cache[e.controlName] = e.icon;
    }

    public override Sprite GetIcon(string bindingPath)
    {
        if (string.IsNullOrEmpty(bindingPath)) return null;
        if (m_Cache == null) BuildCache();

        // Extract control name dari path seperti "<Keyboard>/a" -> "a"
        // atau "<Mouse>/leftButton" -> "leftButton"
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