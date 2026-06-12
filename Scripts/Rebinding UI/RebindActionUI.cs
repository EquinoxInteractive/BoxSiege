// RebindActionUI.cs - Fixed Final with Icon Support

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

namespace UnityEngine.InputSystem.Samples.RebindUI
{
    public class RebindActionUI : MonoBehaviour
    {
        // --- Properties ---

        public InputActionReference actionReference
        {
            get => m_Action;
            set { m_Action = value; UpdateActionLabel(); UpdateBindingDisplay(); }
        }

        public string bindingId
        {
            get => m_BindingId;
            set { m_BindingId = value; UpdateBindingDisplay(); }
        }

        public InputBinding.DisplayStringOptions displayStringOptions
        {
            get => m_DisplayStringOptions;
            set { m_DisplayStringOptions = value; UpdateBindingDisplay(); }
        }

        public Text actionLabel
        {
            get => m_ActionLabel;
            set { m_ActionLabel = value; UpdateActionLabel(); }
        }

        public Text bindingText
        {
            get => m_BindingText;
            set { m_BindingText = value; UpdateBindingDisplay(); }
        }

        public Text rebindPrompt
        {
            get => m_RebindText;
            set => m_RebindText = value;
        }

        public GameObject rebindOverlay
        {
            get => m_RebindOverlay;
            set => m_RebindOverlay = value;
        }

        public UpdateBindingUIEvent updateBindingUIEvent
        {
            get
            {
                if (m_UpdateBindingUIEvent == null)
                    m_UpdateBindingUIEvent = new UpdateBindingUIEvent();
                return m_UpdateBindingUIEvent;
            }
        }

        public InteractiveRebindEvent startRebindEvent
        {
            get
            {
                if (m_RebindStartEvent == null)
                    m_RebindStartEvent = new InteractiveRebindEvent();
                return m_RebindStartEvent;
            }
        }

        public InteractiveRebindEvent stopRebindEvent
        {
            get
            {
                if (m_RebindStopEvent == null)
                    m_RebindStopEvent = new InteractiveRebindEvent();
                return m_RebindStopEvent;
            }
        }

        public InputActionRebindingExtensions.RebindingOperation ongoingRebind => m_RebindOperation;

        // --- ResolveActionAndBinding ---

        public bool ResolveActionAndBinding(out InputAction action, out int bindingIndex)
        {
            bindingIndex = -1;
            action = m_Action?.action;
            if (action == null || string.IsNullOrEmpty(m_BindingId))
                return false;

            var bindingId = new Guid(m_BindingId);
            bindingIndex = action.bindings.IndexOf(x => x.id == bindingId);
            if (bindingIndex == -1)
            {
                Debug.LogError("Cannot find binding with ID '" + bindingId + "' on '" + action + "'", this);
                return false;
            }
            return true;
        }

        // --- GetIconsDataForPath: pilih icon set berdasarkan layout controller ---

        // --- GetIconsDataForPath ---
        // Deteksi controller dari assigned device (lebih akurat dari path)
        // karena controller generik seperti PS3/PC Wired tidak punya "DualShock" di path.

        private GamepadIconsData GetIconsDataForPath(string bindingPath)
        {
            // PRIORITAS 1: deteksi dari assigned gamepad (paling akurat)
            string assetName = m_Action?.action?.actionMap?.asset?.name ?? string.Empty;
            Gamepad assignedGamepad = ControllerAssignmentManager.GetAssignedGamepad(assetName);

            if (assignedGamepad != null)
            {
                // Cek layout name
                string layout = assignedGamepad.layout ?? string.Empty;

                // Cek nama device + product description
                string deviceName = assignedGamepad.name ?? string.Empty;
                string product    = string.Empty;
                try { product = assignedGamepad.description.product ?? string.Empty; }
                catch { }

                string combined = (layout + " " + deviceName + " " + product).ToLower();

                bool isPS = combined.Contains("dualshock") ||
                            combined.Contains("dualsense") ||
                            combined.Contains("playstation") ||
                            combined.Contains("ps3") ||
                            combined.Contains("ps4") ||
                            combined.Contains("ps5") ||
                            combined.Contains("sixaxis");

                bool isXbox = combined.Contains("xinput") ||
                              combined.Contains("xbox") ||
                              combined.Contains("360");

                if (isPS   && m_IconsDataPS   != null) return m_IconsDataPS;
                if (isXbox && m_IconsDataXbox != null) return m_IconsDataXbox;

                // Controller terdeteksi tapi bukan PS/Xbox (generic HID)
                // Fallback: pakai yang tersedia
                return m_IconsDataPS ?? m_IconsDataXbox;
            }

            // PRIORITAS 2: deteksi dari binding path (fallback jika belum assign)
            if (!string.IsNullOrEmpty(bindingPath))
            {
                bool isPathPS = bindingPath.Contains("DualShock") ||
                                bindingPath.Contains("DualSense") ||
                                bindingPath.Contains("PS4") ||
                                bindingPath.Contains("PS5");

                bool isPathXbox = bindingPath.Contains("XInput") ||
                                  bindingPath.Contains("Xbox");

                if (isPathPS   && m_IconsDataPS   != null) return m_IconsDataPS;
                if (isPathXbox && m_IconsDataXbox != null) return m_IconsDataXbox;
            }

            return m_IconsDataXbox ?? m_IconsDataPS;
        }

        // --- UpdateBindingDisplay with Icon Support ---

        public void UpdateBindingDisplay()
        {
            var displayString    = string.Empty;
            var deviceLayoutName = default(string);
            var controlPath      = default(string);

            var action = m_Action?.action;
            if (action != null)
            {
                var bindingIndex = action.bindings.IndexOf(x => x.id.ToString() == m_BindingId);
                if (bindingIndex != -1)
                {
                    displayString = action.GetBindingDisplayString(
                        bindingIndex,
                        out deviceLayoutName,
                        out controlPath,
                        displayStringOptions);

                    string effectivePath     = action.bindings[bindingIndex].effectivePath ?? string.Empty;
                    bool   isKeyboardOrMouse = effectivePath.StartsWith("<Keyboard>") ||
                                               effectivePath.StartsWith("<Mouse>");
                    string suffix = string.Empty;

                    if (!isKeyboardOrMouse)
                    {
                        string assetName = m_Action?.action?.actionMap?.asset?.name ?? string.Empty;
                        suffix = ControllerAssignmentManager.GetDeviceIndexSuffix(assetName);
                    }

                    // Icon mode: pilih icon set berdasarkan layout controller
                    var iconsData = GetIconsDataForPath(effectivePath);
                    if (!isKeyboardOrMouse && iconsData != null && m_BindingIcon != null)
                    {
                        Sprite icon = iconsData.GetIcon(effectivePath);
                        if (icon != null)
                        {
                            m_BindingIcon.sprite  = icon;
                            m_BindingIcon.enabled = true;

                            // Sembunyikan binding text agar tidak tertumpuk dengan icon
                            if (m_BindingText != null)
                                m_BindingText.text = string.Empty;

                            // Suffix (0)/(1) hanya tampil di SuffixText
                            if (m_SuffixText != null)
                                m_SuffixText.text = suffix;

                            updateBindingUIEvent?.Invoke(this, displayString + suffix, deviceLayoutName, controlPath);
                            return;
                        }
                    }

                    // Text mode: keyboard atau icon tidak tersedia
                    if (m_BindingIcon != null)
                        m_BindingIcon.enabled = false;

                    if (!string.IsNullOrEmpty(suffix))
                        displayString += suffix;
                }
            }

            if (m_BindingText != null)
                m_BindingText.text = displayString;

            if (m_SuffixText != null)
                m_SuffixText.text = string.Empty;

            updateBindingUIEvent?.Invoke(this, displayString, deviceLayoutName, controlPath);
        }

        // --- ResetToDefault ---

        public void ResetToDefault()
        {
            if (!ResolveActionAndBinding(out var action, out var bindingIndex))
                return;

            if (action.bindings[bindingIndex].isComposite)
            {
                for (var i = bindingIndex + 1;
                     i < action.bindings.Count && action.bindings[i].isPartOfComposite;
                     ++i)
                    action.RemoveBindingOverride(i);
            }
            else
            {
                action.RemoveBindingOverride(bindingIndex);
            }

            SaveBindingsToPlayerPrefs();
            UpdateBindingDisplay();
        }

        // --- StartInteractiveRebind ---

        public void StartInteractiveRebind()
        {
            if (m_Action == null || m_Action.action == null)
            {
                Debug.LogWarning("[RebindActionUI] No action assigned.", this);
                return;
            }

            m_Action.action.Disable();

            if (!ResolveActionAndBinding(out var action, out var bindingIndex))
            {
                m_Action.action.Enable();
                return;
            }

            if (action.bindings[bindingIndex].isComposite)
            {
                var firstPartIndex = bindingIndex + 1;
                if (firstPartIndex < action.bindings.Count &&
                    action.bindings[firstPartIndex].isPartOfComposite)
                    PerformInteractiveRebind(action, firstPartIndex, allCompositeParts: true);
            }
            else
            {
                PerformInteractiveRebind(action, bindingIndex);
            }
        }

        // --- PerformInteractiveRebind ---

        private void PerformInteractiveRebind(InputAction action, int bindingIndex,
                                               bool allCompositeParts = false)
        {
            m_RebindOperation?.Cancel();

            void CleanUp()
            {
                m_RebindOperation?.Dispose();
                m_RebindOperation = null;
                if (m_Action != null && m_Action.action != null)
                    m_Action.action.Enable();
            }

            string expectedType = action.bindings[bindingIndex].isPartOfComposite ? "Axis" : null;

            var rebindOp = action.PerformInteractiveRebinding(bindingIndex)
                .WithCancelingThrough("<Keyboard>/escape")
                .WithMagnitudeHavingToBeGreaterThan(0.5f);

            if (!string.IsNullOrEmpty(expectedType))
                rebindOp = rebindOp.WithExpectedControlType(expectedType);

            rebindOp
                .OnCancel(operation =>
                {
                    stopRebindEvent?.Invoke(this, operation);
                    m_RebindOverlay?.SetActive(false);
                    UpdateBindingDisplay();
                    CleanUp();
                })
                .OnComplete(operation =>
                {
                    m_RebindOverlay?.SetActive(false);
                    stopRebindEvent?.Invoke(this, operation);

                    FixAnalogDirection(action, bindingIndex, operation);

                    var selectedDevice = operation.selectedControl?.device;
                    if (selectedDevice is Gamepad gamepad)
                    {
                        string assetName = action.actionMap?.asset?.name ?? string.Empty;
                        if (!string.IsNullOrEmpty(assetName) &&
                            ControllerAssignmentManager.Instance != null)
                        {
                            ControllerAssignmentManager.Instance.AssignGamepadToPlayer(
                                assetName, gamepad);
                        }
                    }

                    SaveBindingsToPlayerPrefs();
                    UpdateBindingDisplay();
                    CleanUp();

                    if (allCompositeParts)
                    {
                        var nextBindingIndex = bindingIndex + 1;
                        if (nextBindingIndex < action.bindings.Count &&
                            action.bindings[nextBindingIndex].isPartOfComposite)
                            PerformInteractiveRebind(action, nextBindingIndex, true);
                    }
                });

            m_RebindOperation = rebindOp;

            // Sembunyikan icon dan reset teks saat rebinding dimulai
            if (m_BindingIcon != null)
                m_BindingIcon.enabled = false;
            if (m_SuffixText != null)
                m_SuffixText.text = string.Empty;

            var partName = action.bindings[bindingIndex].isPartOfComposite
                ? "Binding '" + action.bindings[bindingIndex].name + "'. "
                : "";
            m_RebindOverlay?.SetActive(true);
            if (m_RebindText != null)
            {
                m_RebindText.text = !string.IsNullOrEmpty(expectedType)
                    ? partName + "Tekan input " + expectedType + "..."
                    : partName + "Tekan tombol apa saja...";
            }

            if (m_RebindOverlay == null && m_RebindText == null && m_BindingText != null)
                m_BindingText.text = "<Tekan tombol...>";

            startRebindEvent?.Invoke(this, m_RebindOperation);
            m_RebindOperation.Start();
        }

        // --- FixAnalogDirection ---

        private static void FixAnalogDirection(InputAction action, int bindingIndex,
                                               InputActionRebindingExtensions.RebindingOperation op)
        {
            var ctrl = op.selectedControl;
            if (ctrl == null) return;

            string currentPath = action.bindings[bindingIndex].effectivePath;
            if (string.IsNullOrEmpty(currentPath)) return;

            string part = action.bindings[bindingIndex].isPartOfComposite
                ? action.bindings[bindingIndex].name.ToLower()
                : string.Empty;

            string fixedPath = FixAnalogDirectionInPath(ctrl, currentPath, part);

            if (fixedPath != currentPath)
            {
                action.ApplyBindingOverride(bindingIndex, fixedPath);
                Debug.Log("[RebindActionUI] Analog path: '" + currentPath + "' -> '" + fixedPath + "'");
            }
        }

        private static string FixAnalogDirectionInPath(InputControl ctrl, string path, string part)
        {
            if (string.IsNullOrEmpty(part))
            {
                if (ctrl is StickControl sc)
                {
                    string s = GetDirectionSuffix(sc.ReadValue());
                    return path.EndsWith(s, StringComparison.OrdinalIgnoreCase) ? path : path + s;
                }
                if (ctrl is DpadControl dc)
                {
                    string s = GetDirectionSuffix(dc.ReadValue());
                    return path.EndsWith(s, StringComparison.OrdinalIgnoreCase) ? path : path + s;
                }
                return path;
            }

            if (ctrl is StickControl || ctrl is DpadControl)
            {
                string ds = CompositePartToSuffix(part);
                if (!string.IsNullOrEmpty(ds) &&
                    !path.EndsWith(ds, StringComparison.OrdinalIgnoreCase))
                    return path + ds;
                return path;
            }

            if (ctrl is AxisControl)
            {
                if (path.EndsWith("/x", StringComparison.OrdinalIgnoreCase))
                {
                    string b = path.Substring(0, path.Length - 2);
                    if (part == "left")  return b + "/left";
                    if (part == "right") return b + "/right";
                }
                else if (path.EndsWith("/y", StringComparison.OrdinalIgnoreCase))
                {
                    string b = path.Substring(0, path.Length - 2);
                    if (part == "up")   return b + "/up";
                    if (part == "down") return b + "/down";
                }
            }

            return path;
        }

        private static string GetDirectionSuffix(Vector2 v)
        {
            if (v.magnitude < 0.2f) return "/down";
            if (Mathf.Abs(v.y) >= Mathf.Abs(v.x)) return v.y > 0 ? "/up" : "/down";
            return v.x > 0 ? "/right" : "/left";
        }

        private static string CompositePartToSuffix(string part)
        {
            switch (part)
            {
                case "left":  return "/left";
                case "right": return "/right";
                case "up":    return "/up";
                case "down":  return "/down";
                default:      return string.Empty;
            }
        }

        // --- OnEnable / OnDisable / OnActionChange ---

        protected void OnEnable()
        {
            if (s_RebindActionUIs == null)
                s_RebindActionUIs = new List<RebindActionUI>();
            s_RebindActionUIs.Add(this);
            if (s_RebindActionUIs.Count == 1)
                InputSystem.onActionChange += OnActionChange;
        }

        protected void OnDisable()
        {
            m_RebindOperation?.Dispose();
            m_RebindOperation = null;
            s_RebindActionUIs.Remove(this);
            if (s_RebindActionUIs.Count == 0)
            {
                s_RebindActionUIs = null;
                InputSystem.onActionChange -= OnActionChange;
            }
        }

        private static void OnActionChange(object obj, InputActionChange change)
        {
            if (change != InputActionChange.BoundControlsChanged) return;

            var action      = obj as InputAction;
            var actionMap   = action?.actionMap ?? obj as InputActionMap;
            var actionAsset = actionMap?.asset ?? obj as InputActionAsset;

            for (var i = 0; i < s_RebindActionUIs?.Count; ++i)
            {
                var comp   = s_RebindActionUIs[i];
                var refAct = comp.actionReference?.action;
                if (refAct == null) continue;

                if (refAct == action ||
                    refAct.actionMap == actionMap ||
                    refAct.actionMap?.asset == actionAsset)
                    comp.UpdateBindingDisplay();
            }
        }

        // --- Save / Load Bindings ---

        public void SaveBindingsToPlayerPrefs()
        {
            if (m_Action?.action?.actionMap == null)
            {
                Debug.LogWarning("[RebindActionUI] Cannot save: null reference.", this);
                return;
            }

            var actionMap = m_Action.action.actionMap;
            var assetName = actionMap.asset != null ? actionMap.asset.name : "unnamed";
            string key    = "InputBindings_" + assetName + "_" + actionMap.name;

            try
            {
                var json = actionMap.SaveBindingOverridesAsJson();
                if (!string.IsNullOrEmpty(json))
                {
                    PlayerPrefs.SetString(key, json);
                    PlayerPrefs.Save();
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[RebindActionUI] Save failed: " + e.Message, this);
            }
        }

        public void LoadBindingsFromPlayerPrefs()
        {
            if (m_Action?.action?.actionMap == null) return;

            var actionMap = m_Action.action.actionMap;
            var assetName = actionMap.asset != null ? actionMap.asset.name : "unnamed";
            string key    = "InputBindings_" + assetName + "_" + actionMap.name;

            if (!PlayerPrefs.HasKey(key)) return;
            string saved = PlayerPrefs.GetString(key);
            if (string.IsNullOrEmpty(saved)) return;

            try
            {
                actionMap.RemoveAllBindingOverrides();
                actionMap.LoadBindingOverridesFromJson(saved);
                UpdateBindingDisplay();
            }
            catch (Exception e)
            {
                Debug.LogError("[RebindActionUI] Load failed: " + e.Message, this);
            }
        }

        public static void LoadAllBindingsFromPlayerPrefs()
        {
            var assets    = Resources.FindObjectsOfTypeAll<InputActionAsset>();
            var processed = new HashSet<InputActionAsset>();

            foreach (var asset in assets)
            {
                if (asset == null || processed.Contains(asset)) continue;
                processed.Add(asset);

                foreach (var actionMap in asset.actionMaps)
                {
                    string key = "InputBindings_" + asset.name + "_" + actionMap.name;
                    if (!PlayerPrefs.HasKey(key)) continue;
                    string saved = PlayerPrefs.GetString(key);
                    if (string.IsNullOrEmpty(saved)) continue;

                    try
                    {
                        actionMap.RemoveAllBindingOverrides();
                        actionMap.LoadBindingOverridesFromJson(saved);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("[RebindActionUI] Load failed: " + e.Message);
                    }
                }
            }

            if (s_RebindActionUIs != null)
                foreach (var ui in s_RebindActionUIs)
                    if (ui != null) ui.UpdateBindingDisplay();
        }

        public static void SaveAllBindings()
        {
            var assets    = Resources.FindObjectsOfTypeAll<InputActionAsset>();
            var processed = new HashSet<InputActionAsset>();

            foreach (var asset in assets)
            {
                if (asset == null || processed.Contains(asset)) continue;
                processed.Add(asset);

                foreach (var actionMap in asset.actionMaps)
                {
                    string key = "InputBindings_" + asset.name + "_" + actionMap.name;
                    try
                    {
                        var json = actionMap.SaveBindingOverridesAsJson();
                        if (!string.IsNullOrEmpty(json))
                        {
                            PlayerPrefs.SetString(key, json);
                            PlayerPrefs.Save();
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("[RebindActionUI] Save failed: " + e.Message);
                    }
                }
            }
        }

        public static void ApplySavedBindings() => LoadAllBindingsFromPlayerPrefs();

        // --- Awake / Initialize ---

        private void Awake()
        {
            LoadBindingsFromPlayerPrefs();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            Application.quitting += OnApplicationQuit;
        }

        private static void OnApplicationQuit()
        {
            SaveAllBindings();
            Application.quitting -= OnApplicationQuit;
        }

#if UNITY_EDITOR
        protected void OnValidate()
        {
            UpdateActionLabel();
            UpdateBindingDisplay();
        }
#endif

        private void UpdateActionLabel()
        {
            if (m_ActionLabel != null)
            {
                var action = m_Action?.action;
                m_ActionLabel.text = action != null ? action.name : string.Empty;
            }
        }

        // --- Serialized Fields ---

        [SerializeField] private InputActionReference m_Action;
        [SerializeField] private string m_BindingId;
        [SerializeField] private InputBinding.DisplayStringOptions m_DisplayStringOptions;

        [Header("UI")]
        [SerializeField] private Text m_ActionLabel;
        [SerializeField] private Text m_BindingText;
        [SerializeField] private GameObject m_RebindOverlay;
        [SerializeField] private Text m_RebindText;

        [Header("Icon Support")]
        [Tooltip("Icons untuk Xbox controller (XInput). Kosongkan jika tidak pakai.")]
        [SerializeField] private GamepadIconsData m_IconsDataXbox;
        [Tooltip("Icons untuk PlayStation controller (DualShock/DualSense). Kosongkan jika tidak pakai.")]
        [SerializeField] private GamepadIconsData m_IconsDataPS;
        [Tooltip("Image component untuk icon tombol gamepad")]
        [SerializeField] private UnityEngine.UI.Image m_BindingIcon;
        [Tooltip("Text opsional untuk suffix (0)/(1) terpisah dari binding text")]
        [SerializeField] private Text m_SuffixText;

        [Header("Events")]
        [SerializeField] private UpdateBindingUIEvent m_UpdateBindingUIEvent;
        [SerializeField] private InteractiveRebindEvent m_RebindStartEvent;
        [SerializeField] private InteractiveRebindEvent m_RebindStopEvent;

        private InputActionRebindingExtensions.RebindingOperation m_RebindOperation;
        private static List<RebindActionUI> s_RebindActionUIs;

        // --- Event Types ---

        [Serializable]
        public class UpdateBindingUIEvent
            : UnityEvent<RebindActionUI, string, string, string> { }

        [Serializable]
        public class InteractiveRebindEvent
            : UnityEvent<RebindActionUI, InputActionRebindingExtensions.RebindingOperation> { }
    }
}