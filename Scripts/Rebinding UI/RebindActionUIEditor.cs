#if UNITY_EDITOR
using System.Linq;
using UnityEditor;

namespace UnityEngine.InputSystem.Samples.RebindUI
{
    [CustomEditor(typeof(RebindActionUI))]
    public class RebindActionUIEditor : UnityEditor.Editor
    {
        protected void OnEnable()
        {
            m_ActionProperty               = serializedObject.FindProperty("m_Action");
            m_BindingIdProperty            = serializedObject.FindProperty("m_BindingId");
            m_ActionLabelProperty          = serializedObject.FindProperty("m_ActionLabel");
            m_BindingTextProperty          = serializedObject.FindProperty("m_BindingText");
            m_RebindOverlayProperty        = serializedObject.FindProperty("m_RebindOverlay");
            m_RebindTextProperty           = serializedObject.FindProperty("m_RebindText");
            m_UpdateBindingUIEventProperty = serializedObject.FindProperty("m_UpdateBindingUIEvent");
            m_RebindStartEventProperty     = serializedObject.FindProperty("m_RebindStartEvent");
            m_RebindStopEventProperty      = serializedObject.FindProperty("m_RebindStopEvent");
            m_DisplayStringOptionsProperty = serializedObject.FindProperty("m_DisplayStringOptions");

            // Icon Support fields
            m_IconsDataXboxProperty     = serializedObject.FindProperty("m_IconsDataXbox");
            m_IconsDataPSProperty       = serializedObject.FindProperty("m_IconsDataPS");
            m_IconsDataKeyboardProperty = serializedObject.FindProperty("m_IconsDataKeyboard");
            m_BindingIconProperty       = serializedObject.FindProperty("m_BindingIcon");
            m_SuffixTextProperty        = serializedObject.FindProperty("m_SuffixText");

            RefreshBindingOptions();
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            // Binding section
            EditorGUILayout.LabelField(m_BindingLabel, Styles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(m_ActionProperty);

                var newSelectedBinding = EditorGUILayout.Popup(m_BindingLabel, m_SelectedBindingOption, m_BindingOptions);
                if (newSelectedBinding != m_SelectedBindingOption)
                {
                    var bindingId = m_BindingOptionValues[newSelectedBinding];
                    m_BindingIdProperty.stringValue = bindingId;
                    m_SelectedBindingOption = newSelectedBinding;
                }

                var optionsOld = (InputBinding.DisplayStringOptions)m_DisplayStringOptionsProperty.intValue;
                var optionsNew = (InputBinding.DisplayStringOptions)EditorGUILayout.EnumFlagsField(m_DisplayOptionsLabel, optionsOld);
                if (optionsOld != optionsNew)
                    m_DisplayStringOptionsProperty.intValue = (int)optionsNew;
            }

            // UI section
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(m_UILabel, Styles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(m_ActionLabelProperty);
                EditorGUILayout.PropertyField(m_BindingTextProperty);
                EditorGUILayout.PropertyField(m_RebindOverlayProperty);
                EditorGUILayout.PropertyField(m_RebindTextProperty);
            }

            // Icon Support section
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(m_IconSupportLabel, Styles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(m_IconsDataXboxProperty,
                    new GUIContent("Icons Data Xbox", "Icons untuk Xbox / XInput controller"));
                EditorGUILayout.PropertyField(m_IconsDataPSProperty,
                    new GUIContent("Icons Data PS", "Icons untuk PlayStation (DualShock / DualSense)"));
                EditorGUILayout.PropertyField(m_IconsDataKeyboardProperty,
                    new GUIContent("Icons Data Keyboard", "Icons untuk Keyboard & Mouse"));
                EditorGUILayout.PropertyField(m_BindingIconProperty,
                    new GUIContent("Binding Icon", "Image component untuk menampilkan icon tombol"));
                EditorGUILayout.PropertyField(m_SuffixTextProperty,
                    new GUIContent("Suffix Text", "Text opsional untuk suffix (0)/(1)"));
            }

            // Events section
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(m_EventsLabel, Styles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(m_RebindStartEventProperty);
                EditorGUILayout.PropertyField(m_RebindStopEventProperty);
                EditorGUILayout.PropertyField(m_UpdateBindingUIEventProperty);
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                RefreshBindingOptions();
            }
        }

        protected void RefreshBindingOptions()
        {
            var actionReference = (InputActionReference)m_ActionProperty.objectReferenceValue;
            var action = actionReference?.action;

            if (action == null)
            {
                m_BindingOptions = new GUIContent[0];
                m_BindingOptionValues = new string[0];
                m_SelectedBindingOption = -1;
                return;
            }

            var bindings = action.bindings;
            var bindingCount = bindings.Count;

            m_BindingOptions = new GUIContent[bindingCount];
            m_BindingOptionValues = new string[bindingCount];
            m_SelectedBindingOption = -1;

            var currentBindingId = m_BindingIdProperty.stringValue;
            for (var i = 0; i < bindingCount; ++i)
            {
                var binding = bindings[i];
                var bindingId = binding.id.ToString();
                var haveBindingGroups = !string.IsNullOrEmpty(binding.groups);

                var displayOptions =
                    InputBinding.DisplayStringOptions.DontUseShortDisplayNames |
                    InputBinding.DisplayStringOptions.IgnoreBindingOverrides;
                if (!haveBindingGroups)
                    displayOptions |= InputBinding.DisplayStringOptions.DontOmitDevice;

                var displayString = action.GetBindingDisplayString(i, displayOptions);

                if (binding.isPartOfComposite)
                    displayString = ObjectNames.NicifyVariableName(binding.name) + ": " + displayString;

                displayString = displayString.Replace('/', '\\');

                if (haveBindingGroups)
                {
                    var asset = action.actionMap?.asset;
                    if (asset != null)
                    {
                        var controlSchemes = string.Join(", ",
                            binding.groups.Split(InputBinding.Separator)
                                .Select(x => asset.controlSchemes.FirstOrDefault(c => c.bindingGroup == x).name));
                        displayString = displayString + " (" + controlSchemes + ")";
                    }
                }

                m_BindingOptions[i] = new GUIContent(displayString);
                m_BindingOptionValues[i] = bindingId;

                if (currentBindingId == bindingId)
                    m_SelectedBindingOption = i;
            }
        }

        // Original properties
        private SerializedProperty m_ActionProperty;
        private SerializedProperty m_BindingIdProperty;
        private SerializedProperty m_ActionLabelProperty;
        private SerializedProperty m_BindingTextProperty;
        private SerializedProperty m_RebindOverlayProperty;
        private SerializedProperty m_RebindTextProperty;
        private SerializedProperty m_RebindStartEventProperty;
        private SerializedProperty m_RebindStopEventProperty;
        private SerializedProperty m_UpdateBindingUIEventProperty;
        private SerializedProperty m_DisplayStringOptionsProperty;

        // Icon Support properties
        private SerializedProperty m_IconsDataXboxProperty;
        private SerializedProperty m_IconsDataPSProperty;
        private SerializedProperty m_IconsDataKeyboardProperty;
        private SerializedProperty m_BindingIconProperty;
        private SerializedProperty m_SuffixTextProperty;

        private GUIContent m_BindingLabel      = new GUIContent("Binding");
        private GUIContent m_DisplayOptionsLabel = new GUIContent("Display Options");
        private GUIContent m_UILabel            = new GUIContent("UI");
        private GUIContent m_IconSupportLabel   = new GUIContent("Icon Support");
        private GUIContent m_EventsLabel        = new GUIContent("Events");
        private GUIContent[] m_BindingOptions;
        private string[] m_BindingOptionValues;
        private int m_SelectedBindingOption;

        private static class Styles
        {
            public static GUIStyle boldLabel = new GUIStyle("MiniBoldLabel");
        }
    }
}
#endif