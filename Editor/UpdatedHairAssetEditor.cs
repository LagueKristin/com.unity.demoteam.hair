using System;
using System.Reflection;
using Unity.DemoTeam.Hair;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

[CustomEditor(typeof(HairAsset))]
public class UpdatedHairAssetEditor : Editor
{
    public VisualTreeAsset ProceduralUXML;
    public VisualTreeAsset AlembicUXML;
    public VisualTreeAsset CustomUXML;

    private VisualElement root;
    private HairAsset targetScript;
    private SerializedObject serializedTarget;

    private Button buildStrandGroupButton;
    private bool isDragging;

    public override VisualElement CreateInspectorGUI()
    {
        targetScript = target as HairAsset;
        if (targetScript == null)
        {
            Debug.LogError("Target script is null, this should never happen!");
            return null;
        }

        serializedTarget = new SerializedObject(target);

        root = new VisualElement();

        switch (targetScript.settingsBasic.type)
        {
            case HairAsset.Type.Procedural:
                
                StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.unity.demoteam.hair/Editor/HairSystem.uss");
                ProceduralUXML.CloneTree(root);
                root.styleSheets.Add(styleSheet);
                SetupProceduralUI();
                break;
            
            case HairAsset.Type.Alembic:
                SetupAlembicUI();
                AlembicUXML.CloneTree(root);
                break;
            
            case HairAsset.Type.Custom:
                SetupCustomUI();
                CustomUXML.CloneTree(root);
                break;
        }
        
        SubscribeToAllElementChanges();
        return root;
    }

    private void SetupProceduralUI()
    {
        if (root == null)
        {
            Debug.LogError("Something unexpected happened and we couldn't find the root visual element");
            return;
        }
        
        DrawRootSettings();
        DrawStrandsSettings();
        DrawCurlSettings();
        DrawLODSettings();
        DrawBuildButton();
    }

    private void BuildStrandGroupButtonClicked()
    {
        //Debug.Log("BUILD");
        HairAssetBuilder.BuildHairAsset(targetScript);
        serializedObject.Update();
    }

    private void SetupAlembicUI() { }

    private void SetupCustomUI() { }

    [MenuItem("Assets/Create/Hair/Hair Asset/Procedural")]
    public static void CreateProceduralHairAsset()
    {
        CreateHairAssetOfType(HairAsset.Type.Procedural);
    }

    [MenuItem("Assets/Create/Hair/Hair Asset/Alembic")]
    public static void CreateAlembicHairAsset()
    {
        CreateHairAssetOfType(HairAsset.Type.Alembic);
    }

    [MenuItem("Assets/Create/Hair/Hair Asset/Custom")]
    public static void CreateCustomHairAsset()
    {
        CreateHairAssetOfType(HairAsset.Type.Custom);
    }

    private static void CreateHairAssetOfType(HairAsset.Type assetType)
    {
        HairAsset addedAsset = CreateInstance<HairAsset>();
        addedAsset.settingsBasic.type = assetType;
        
        MethodInfo getActiveFolderPath = typeof(ProjectWindowUtil).GetMethod("GetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);
        if (getActiveFolderPath != null)
        {
            string folderPath = (string) getActiveFolderPath.Invoke(null, null);
            AssetDatabase.CreateAsset(addedAsset, AssetDatabase.GenerateUniqueAssetPath(folderPath + $"/{assetType.ToString()} HairAsset.asset"));
        }
        else
        {
            //defensive coding - this should never happen
            DestroyImmediate(addedAsset);
            Debug.LogError("Something went wrong and we couldn't create the Hair Asset! Try again!");
            return;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = addedAsset;
    }

    private void SubscribeToAllElementChanges()
    {
        root.Query<PropertyField>().ForEach(field =>
        {
            TrackPropertyField(field);
        });
    }

    private void TrackPropertyField(PropertyField field)
    {
        field.TrackPropertyValue(serializedTarget.FindProperty(field.bindingPath), property =>
        {
            if (!MouseCaptureController.HasMouseCapture(field))
            {
                Debug.Log("Object changed - regenerate if needed");
                //CheckForAutoGenerate();
            }
        });
    }

    private void CheckForAutoGenerate()
    {
        serializedObject.ApplyModifiedProperties();
        if (targetScript.strandGroupsAutoBuild)
        {
            BuildStrandGroupButtonClicked();
        }
    }

    private void DisableFoldoutStyle(Foldout foldout)
    {
        var foldoutToggle = foldout.Q<Toggle>(className: "unity-foldout__toggle");
        var foldoutCheckmark = foldoutToggle.Q(className: Toggle.checkmarkUssClassName);
        foldoutToggle.style.display = DisplayStyle.None;
        foldoutCheckmark.style.display = DisplayStyle.None;
    }

    private void DrawBuildButton()
    {
        buildStrandGroupButton = root.Q<Button>("buildStrandGroupBT");
        buildStrandGroupButton.clicked -= BuildStrandGroupButtonClicked;
        buildStrandGroupButton.clicked += BuildStrandGroupButtonClicked;
    }

    private void DrawRootSettings()
    {
        PropertyField placement = root.Q<PropertyField>("placementPF");
        
        Refresh();
        placement.TrackPropertyValue(serializedTarget.FindProperty(placement.bindingPath), property => Refresh());

        void Refresh()
        {
            root.Q<PropertyField>("primitivePF").style.display = targetScript.settingsProcedural.placement == HairAsset.SettingsProcedural.PlacementType.Primitive ? DisplayStyle.Flex : DisplayStyle.None;
            root.Q<PropertyField>("placementProviderPF").style.display = targetScript.settingsProcedural.placement == HairAsset.SettingsProcedural.PlacementType.Custom ? DisplayStyle.Flex : DisplayStyle.None;
            root.Q<PropertyField>("placementMeshPF").style.display = targetScript.settingsProcedural.placement == HairAsset.SettingsProcedural.PlacementType.Mesh ? DisplayStyle.Flex : DisplayStyle.None;
            root.Q<PropertyField>("placementGroupsPF").style.display = targetScript.settingsProcedural.placement == HairAsset.SettingsProcedural.PlacementType.Mesh ? DisplayStyle.Flex : DisplayStyle.None;
            root.Q<PropertyField>("mappedDensityPF").style.display = targetScript.settingsProcedural.placement == HairAsset.SettingsProcedural.PlacementType.Mesh ? DisplayStyle.Flex : DisplayStyle.None;
            root.Q<PropertyField>("mappedDirectionPF").style.display = targetScript.settingsProcedural.placement == HairAsset.SettingsProcedural.PlacementType.Mesh ? DisplayStyle.Flex : DisplayStyle.None;
            root.Q<PropertyField>("mappedParametersPF").style.display = targetScript.settingsProcedural.placement == HairAsset.SettingsProcedural.PlacementType.Mesh ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    private void DrawStrandsSettings()
    {
        PropertyField strandLengthVariation = root.Q<PropertyField>("strandLengthVariationPF");
        DisableFoldoutStyle(root.Q<Foldout>("lengthVariationFO"));
        
        Refresh();
        strandLengthVariation.TrackPropertyValue(serializedTarget.FindProperty(strandLengthVariation.bindingPath), property => Refresh());

        void Refresh()
        {
            root.Q<PropertyField>("strandLengthVariationFactorPF").SetEnabled(targetScript.settingsProcedural.strandLengthVariation);
        }
    }

    private void DrawCurlSettings()
    {
        PropertyField curl = root.Q<PropertyField>("curlPF");
        PropertyField curlVariation = root.Q<PropertyField>("curlVariationPF");

        DisableFoldoutStyle(root.Q<Foldout>("curlEnableFO"));
        DisableFoldoutStyle(root.Q<Foldout>("curlVariationFO"));

        curl.TrackPropertyValue(serializedTarget.FindProperty(curl.bindingPath), property => Refresh());
        curlVariation.TrackPropertyValue(serializedTarget.FindProperty(curlVariation.bindingPath), property => Refresh());

        Refresh();
        
        void Refresh()
        {
            root.Q<PropertyField>("curlSamplingPF").SetEnabled(targetScript.settingsProcedural.curl);
            root.Q<PropertyField>("curlRadiusPF").SetEnabled(targetScript.settingsProcedural.curl);
            root.Q<PropertyField>("curlSlopePF").SetEnabled(targetScript.settingsProcedural.curl);
            root.Q<PropertyField>("curlVariationPF").SetEnabled(targetScript.settingsProcedural.curl);
            root.Q<PropertyField>("curlVariationRadiusPF").SetEnabled(targetScript.settingsProcedural.curlVariation && targetScript.settingsProcedural.curl);
            root.Q<PropertyField>("curlVariationSlopePF").SetEnabled(targetScript.settingsProcedural.curlVariation && targetScript.settingsProcedural.curl);
        }
    }

    private void DrawLODSettings()
    {
        PropertyField lodGeneration = root.Q<PropertyField>("lodGenerationPF");
        PropertyField lodClusterAllocation = root.Q<PropertyField>("clusterAllocationPF");
        PropertyField lodClusterRefinement = root.Q<PropertyField>("clusterRefinementPF");
        PropertyField highLOD = root.Q<PropertyField>("highLODPF");
        PropertyField highLodMode = root.Q<PropertyField>("HighLODPF");
        PropertyField highLodClusters = root.Q<PropertyField>("highLodClustersPF");

        DisableFoldoutStyle(root.Q<Foldout>("lodEnableFO"));
        DisableFoldoutStyle(root.Q<Foldout>("lodClusteringFO"));
        DisableFoldoutStyle(root.Q<Foldout>("refinementFO"));
        DisableFoldoutStyle(root.Q<Foldout>("baselodFO"));
        DisableFoldoutStyle(root.Q<Foldout>("highlodFO"));
        DisableFoldoutStyle(root.Q<Foldout>("highLodEnableFO"));
        
        Refresh();

        lodGeneration.TrackPropertyValue(serializedTarget.FindProperty(lodGeneration.bindingPath), property => Refresh());
        lodClusterAllocation.TrackPropertyValue(serializedTarget.FindProperty(lodClusterAllocation.bindingPath), property => Refresh());
        lodClusterRefinement.TrackPropertyValue(serializedTarget.FindProperty(lodClusterRefinement.bindingPath),evt => Refresh());
        highLOD.TrackPropertyValue(serializedTarget.FindProperty(highLOD.bindingPath), property => Refresh());
        highLodMode.TrackPropertyValue(serializedTarget.FindProperty(highLodMode.bindingPath), property => Refresh());
        
        void Refresh()
        {
            root.Q<PropertyField>("clusterRefinementValuePF").SetEnabled(targetScript.settingsLODClusters.clusterRefinement);
            root.Q<Foldout>("lodEnableFO").SetEnabled(targetScript.settingsBasic.kLODClusters);
            root.Q<PropertyField>("clusterAllocationOrderPF").style.display = targetScript.settingsLODClusters.clusterAllocation != ClusterAllocationPolicy.Global ? DisplayStyle.Flex : DisplayStyle.None;
            root.Q<Foldout>("highLodEnableFO").SetEnabled(targetScript.settingsLODClusters.highLOD.highLOD);
            highLodClusters.SetEnabled(targetScript.settingsLODClusters.highLOD.highLOD);
            highLodClusters.style.display = targetScript.settingsLODClusters.highLOD.highLODMode == HairAsset.SettingsLODClusters.HighLODMode.Manual ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
