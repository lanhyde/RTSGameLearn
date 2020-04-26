using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class PresetWindow : EditorWindow {
    [MenuItem ("Window/UIElements/PresetWindow")]
    public static void ShowExample () {
        PresetWindow wnd = GetWindow<PresetWindow> ();
        wnd.titleContent = new GUIContent ("PresetWindow");
    }

    private PresetObject presetManager;
    private SerializedObject presetManagerSerialized;
    private Preset selectedPreset;
    private ObjectField presetObjectField;
    public void OnEnable () {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset> ("Assets/Tests/RW/Editor/PresetWindow.uxml");
        VisualElement labelFromUXML = visualTree.CloneTree ();
        root.Add (labelFromUXML);

        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet> ("Assets/Tests/RW/Editor/PresetWindow.uss");

        var preMadeStyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet> ("Assets/Tests/RW/Editor/PresetTemplate.uss");
        root.styleSheets.Add(styleSheet);
        root.styleSheets.Add(preMadeStyleSheet);

        presetObjectField = root.Q<ObjectField>("ObjectField");
        presetObjectField.objectType = typeof(PresetObject);

        presetObjectField.RegisterCallback<ChangeEvent<UnityEngine.Object>>(e => {
            if(presetObjectField.value != null)
            {
                presetManager = (PresetObject) presetObjectField.value;
                presetManagerSerialized = new SerializedObject(presetManager);
            }
            PopulatePresetList();
        });
        PopulatePresetList();
        SetupControls();
    }
 
    private void OnGUI()
    {
        rootVisualElement.Q<VisualElement>("Container").style.height = new StyleLength(position.height);
    }

    private void SetupControls()
    {
        Button newButton = rootVisualElement.Q<Button>("NewButton");
        Button clearButton = rootVisualElement.Q<Button>("ClearButton");
        Button deleteButton = rootVisualElement.Q<Button>("DeleteButton");

        newButton.clickable.clicked += () => {
            if(presetManager != null)
            {
                Preset newPreset = new Preset();
                presetManager.presets.Add(newPreset);

                EditorUtility.SetDirty(presetManager);
                PopulatePresetList();
                BindControls();
            }
        };

        clearButton.clickable.clicked += () => {
            if(presetManager != null && selectedPreset != null)
            {
                selectedPreset.color = Color.black;
                selectedPreset.animationSpeed = 1;
                selectedPreset.objectName = "Unnamed Preset";
                selectedPreset.isAnimating = true;
                selectedPreset.rotation = Vector3.zero;
                selectedPreset.size = Vector3.one;
            }
        };

        deleteButton.clickable.clicked += () => {
            if(presetManager != null && selectedPreset != null)
            {
                presetManager.presets.Remove(selectedPreset);
                PopulatePresetList();
                BindControls();
            }
        };
    }

    private void PopulatePresetList()
    {
        ListView list = rootVisualElement.Q<ListView>("ListView");
        list.Clear();

        Dictionary<Button, int> buttonDictionary = new Dictionary<Button, int>();
        if(presetManager == null)
        {
            return;
        }

        for(int i = 0; i < presetManager.presets.Count; ++i)
        {
            VisualElement listContainer = new VisualElement();
            listContainer.name = "ListContainer";

            Label listLabel = new Label(presetManager.presets[i].objectName);

            Button listButton = new Button();
            listButton.text = "L";

            listLabel.AddToClassList("ListLabel");
            listButton.AddToClassList("ListButton");

            listContainer.Add(listLabel);
            listContainer.Add(listButton);

            list.Insert(list.childCount, listContainer);

            if(!buttonDictionary.ContainsKey(listButton))
            {
                buttonDictionary.Add(listButton, i);
            }

            listButton.clickable.clicked += () => {
                if(presetObjectField.value != null)
                {
                    LoadPreset(buttonDictionary[listButton]);
                }
            };

            if(selectedPreset == presetManager.presets[buttonDictionary[listButton]])
            {
                listButton.style.backgroundColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f, 1f));
            }
        }
    }

    private void LoadPreset(int elementID)
    {
        if(presetManager != null)
        {
            selectedPreset = presetManager.presets[elementID];
            presetManager.currentlyEditing = selectedPreset;

            PopulatePresetList();

            if(selectedPreset != null)
            {
                BindControls();
            }
        }
        else
        {
            PopulatePresetList();
        }
    }

    private void BindControls()
    {
        SerializedProperty objectName = presetManagerSerialized.FindProperty("currentlyEditing.objectName");
        SerializedProperty objectColor = presetManagerSerialized.FindProperty("currentlyEditing.color");
        SerializedProperty objectSize = presetManagerSerialized.FindProperty("currentlyEditing.size");
        SerializedProperty objectRotation = presetManagerSerialized.FindProperty("currentlyEditing.rotation");
        SerializedProperty objectAnimationSpeed = presetManagerSerialized.FindProperty("currentlyEditing.animationSpeed");
        SerializedProperty objectIsAnimating = presetManagerSerialized.FindProperty("currentlyEditing.isAnimating");

        rootVisualElement.Q<TextField>("ObjectName").BindProperty(objectName);
        rootVisualElement.Q<ColorField>("ColorField").BindProperty(objectColor);
        rootVisualElement.Q<Vector3Field>("SizeField").BindProperty(objectSize);
        rootVisualElement.Q<Vector3Field>("RotationField").BindProperty(objectRotation);
        rootVisualElement.Q<FloatField>("AnimationSpeedField").BindProperty(objectAnimationSpeed);
        rootVisualElement.Q<Toggle>("IsAnimatingField").BindProperty(objectIsAnimating);
    }
}