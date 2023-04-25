using System.Reflection;
using Unity.DemoTeam.Hair;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(HairAsset))]
public class UpdatedHairAssetEditor : Editor
{
    public VisualTreeAsset ProceduralUXML;
    public VisualTreeAsset AlembicUXML;
    public VisualTreeAsset CustomUXML;

    private VisualElement root;
    private HairAsset targetScript;

    public override VisualElement CreateInspectorGUI()
    {
        targetScript = target as HairAsset;
        if (targetScript == null)
        {
            Debug.LogError("Target script is null, this should never happen!");
            return null;
        }

        root = new VisualElement();

        switch (targetScript.settingsBasic.type)
        {
            case HairAsset.Type.Procedural:
                //StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.unity.demoteam.hair/Editor/HairSimulation.uss");
                ProceduralUXML.CloneTree(root);
                //root.styleSheets.Add(styleSheet);
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
        
        return root;
    }

    private void SetupProceduralUI()
    {
        if (root == null)
        {
            Debug.LogError("Something unexpected happened and we couldn't find the root visual element");
            return;
        }

        Toggle lodGeneration = root.Q<Toggle>("lodGenerationTG");
        EnumField lodClusteringMode = root.Q<EnumField>("lodClusteringEF");
        VisualElement lodSettings = root.Q<VisualElement>("lodSettingsVE");
        EnumField placement = root.Q<EnumField>("placementEF");
        EnumField placementPrimitive = root.Q<EnumField>("primitiveEF");
        ObjectField placementProvider = root.Q<ObjectField>("placementProviderOF");
        ObjectField placementMesh = root.Q<ObjectField>("placementMeshOF");
        MaskField placementMeshGroups = root.Q<MaskField>("placementGroupsMF");
        ObjectField mappedDensity = root.Q<ObjectField>("mappedDensityOF");
        ObjectField mappedDirection = root.Q<ObjectField>("mappedDirectionOF");
        ObjectField mappedParameters = root.Q<ObjectField>("mappedParametersOF");
        SliderInt strandCount = root.Q<SliderInt>("strandcountSD");
        SliderInt strandParticleCount = root.Q<SliderInt>("strandParticleCountSD");
        Toggle strandLengthVariation = root.Q<Toggle>("strandLengthVariationTG");
        Slider strandLengthVariationAmount = root.Q<Slider>("strandLengthVariantionFactorSD");
        Toggle curl = root.Q<Toggle>("curlTG");
        Slider curlRadius = root.Q<Slider>("curlRadiusSD");
        Slider curlSlope = root.Q<Slider>("curlSlopeSD");
        Toggle curlVariation = root.Q<Toggle>("curlVariationTG");
        Slider curlVariationRadius = root.Q<Slider>("curlVariationRadiusSD");
        Slider curlVariationSlope = root.Q<Slider>("curlVariationSlopeSD");
        EnumField curlSamplingStrategy = root.Q<EnumField>("curlSamplingEF");
        
        SetLodSettings();

        lodGeneration.RegisterValueChangedCallback(evt =>
        {
            SetLodSettings();
        });

        void SetLodSettings()
        {
            lodClusteringMode.style.display = targetScript.settingsBasic.kLODClusters ? DisplayStyle.Flex : DisplayStyle.None;
            //lodSettings.style.display = targetScript.settingsBasic.kLODClusters ? DisplayStyle.Flex : DisplayStyle.None;
        }

        SetPlacementSettings();
        
        placement.RegisterValueChangedCallback(evt =>
        {
            SetPlacementSettings();
        });

        void SetPlacementSettings()
        {
            placementPrimitive.style.display = targetScript.settingsProcedural.placement == HairAsset.SettingsProcedural.PlacementType.Primitive ? DisplayStyle.Flex : DisplayStyle.None;
            placementProvider.style.display = targetScript.settingsProcedural.placement == HairAsset.SettingsProcedural.PlacementType.Custom ? DisplayStyle.Flex : DisplayStyle.None;
            placementMesh.style.display = targetScript.settingsProcedural.placement == HairAsset.SettingsProcedural.PlacementType.Mesh ? DisplayStyle.Flex : DisplayStyle.None;
            placementMeshGroups.style.display = targetScript.settingsProcedural.placement == HairAsset.SettingsProcedural.PlacementType.Mesh ? DisplayStyle.Flex : DisplayStyle.None;
            mappedDensity.style.display = targetScript.settingsProcedural.placement == HairAsset.SettingsProcedural.PlacementType.Mesh ? DisplayStyle.Flex : DisplayStyle.None;
            mappedDirection.style.display = targetScript.settingsProcedural.placement == HairAsset.SettingsProcedural.PlacementType.Mesh ? DisplayStyle.Flex : DisplayStyle.None;
            mappedParameters.style.display = targetScript.settingsProcedural.placement == HairAsset.SettingsProcedural.PlacementType.Mesh ? DisplayStyle.Flex : DisplayStyle.None;
        }

        strandCount.highValue = HairSim.MAX_STRAND_COUNT;
        strandCount.lowValue = HairSim.MIN_STRAND_COUNT;
        strandParticleCount.highValue = HairSim.MAX_STRAND_PARTICLE_COUNT;
        strandParticleCount.lowValue = HairSim.MIN_STRAND_PARTICLE_COUNT;

        SetLengthVariationSetting();

        strandLengthVariation.RegisterValueChangedCallback(evt => SetLengthVariationSetting());

        void SetLengthVariationSetting()
        {
            strandLengthVariationAmount.style.display = targetScript.settingsProcedural.strandLengthVariation ? DisplayStyle.Flex : DisplayStyle.None;
        }

        curlRadius.highValue = 10.0f;
        curlRadius.lowValue = 0.0f;
        curlSlope.highValue = 1.0f;
        curlSlope.lowValue = 0.0f;
        curlVariationRadius.highValue = 1.0f;
        curlVariationRadius.lowValue = 0.0f;
        curlVariationSlope.highValue = 1.0f;
        curlVariationSlope.lowValue = 0.0f;

        curl.RegisterCallback<ChangeEvent<bool>>(evt => RedrawCurlSettings());
        curlVariation.RegisterValueChangedCallback(evt => RedrawCurlSettings());
        
        RedrawCurlSettings();
        
        void RedrawCurlSettings()
        {
            //curlRadius.style.display = targetScript.settingsProcedural.curl ? DisplayStyle.Flex : DisplayStyle.None;
            curlRadius.SetEnabled(targetScript.settingsProcedural.curl);
            curlSlope.style.display = targetScript.settingsProcedural.curl ? DisplayStyle.Flex : DisplayStyle.None;
            curlVariation.style.display = targetScript.settingsProcedural.curl ? DisplayStyle.Flex : DisplayStyle.None;
            curlSamplingStrategy.style.display = targetScript.settingsProcedural.curl ? DisplayStyle.Flex : DisplayStyle.None;
            curlVariationRadius.style.display = targetScript.settingsProcedural.curlVariation && targetScript.settingsProcedural.curl ? DisplayStyle.Flex : DisplayStyle.None;
            curlVariationSlope.style.display = targetScript.settingsProcedural.curlVariation && targetScript.settingsProcedural.curl ? DisplayStyle.Flex : DisplayStyle.None;
        }

        EnumField lodClusterAllocation = root.Q<EnumField>("clusterAllocationEF");
        EnumField lodClusterAllocationOrder = root.Q<EnumField>("clusterAllocationOrderEF");
        Toggle lodClusterRefinement = root.Q<Toggle>("clusterRefinementTG");
        SliderInt clusterRefinementValue = root.Q<SliderInt>("clusterRefinementValueSD");
        Toggle highLOD = root.Q<Toggle>("highLODTG");
        EnumField highLodMode = root.Q<EnumField>("HighLODEF");
        ListView highLodClusters = root.Q<ListView>("highLodClustersLV");
        Button buildStrandGroupButton = root.Q<Button>("buildStrandGroupBT");
        
        SetClustorAllocationSetting();
        
        lodClusterAllocation.RegisterValueChangedCallback(evt => SetClustorAllocationSetting());

        void SetClustorAllocationSetting()
        {
            lodClusterAllocationOrder.style.display = targetScript.settingsLODClusters.clusterAllocation != ClusterAllocationPolicy.Global ? DisplayStyle.Flex : DisplayStyle.None;
        }

        SetClusterRefinementSetting();

        lodClusterRefinement.RegisterValueChangedCallback(evt => SetClusterRefinementSetting());

        void SetClusterRefinementSetting()
        {
            clusterRefinementValue.style.display = targetScript.settingsLODClusters.clusterRefinement
                ? DisplayStyle.Flex
                : DisplayStyle.None;
        }

        SetLodModeSettings();

        highLOD.RegisterValueChangedCallback(evt =>
        {
            SetLodModeSettings();
        });

        void SetLodModeSettings()
        {
            highLodMode.style.display =
                targetScript.settingsLODClusters.highLOD.highLOD ? DisplayStyle.Flex : DisplayStyle.None;
            highLodClusters.style.display = targetScript.settingsLODClusters.highLOD.highLOD && targetScript.settingsLODClusters.highLOD.highLODMode == HairAsset.SettingsLODClusters.HighLODMode.Manual ? DisplayStyle.Flex : DisplayStyle.None;
        }

        highLodMode.RegisterValueChangedCallback(evt =>
        {
            SetLodModeSettings();
        });
        
        buildStrandGroupButton.clicked -= BuildStrandGroupButtonClicked;
        buildStrandGroupButton.clicked += BuildStrandGroupButtonClicked;

    }

    private void BuildStrandGroupButtonClicked()
    {
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
}
