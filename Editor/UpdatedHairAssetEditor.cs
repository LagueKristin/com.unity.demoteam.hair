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

    private Button buildStrandGroupButton;

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
                
                StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.unity.demoteam.hair/Editor/HairSystem.uss");
                ProceduralUXML.CloneTree(root);
                root.styleSheets.Add(styleSheet);
                SetupProceduralUI();
                root.TrackSerializedObjectValue(this.serializedObject, o =>
                {
                    Debug.Log("SO CHSNGE");
                });
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

        root.schedule.Execute(() =>
        {
            if (GUI.changed)
            {
                Debug.Log("TEEEEST");
            }
        }).Every(100);

        DrawRootSettings();
        DrawStrandsSettings();
        DrawCurlSettings();
        DrawLODSettings();
        DrawBuildButton();
    }

    private void BuildStrandGroupButtonClicked()
    {
        Debug.Log("BUILD");
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
        root.Query<EnumField>().ForEach(field =>
        {
            field.UnregisterValueChangedCallback(ChangeEventCheck);
            field.RegisterValueChangedCallback(ChangeEventCheck);
        });

        root.Query<ObjectField>().ForEach(field =>
        {
            field.UnregisterValueChangedCallback(ChangeEventCheck);
            field.RegisterValueChangedCallback(ChangeEventCheck);
        });

        root.Query<MaskField>().ForEach(field =>
        {
            field.UnregisterValueChangedCallback(ChangeEventCheck);
            field.RegisterValueChangedCallback(ChangeEventCheck);
        });

        root.Query<SliderInt>().ForEach(slider =>
        {
            slider.UnregisterValueChangedCallback(ChangeEventCheck);
            slider.RegisterValueChangedCallback(ChangeEventCheck);

        });

        root.Query<Slider>().ForEach(slider =>
        {
            slider.UnregisterValueChangedCallback(ChangeEventCheck);
            slider.RegisterValueChangedCallback(ChangeEventCheck);
        });

        root.Query<Toggle>().ForEach(toggle =>
        {
            toggle.UnregisterValueChangedCallback(ChangeEventCheck);
            toggle.RegisterValueChangedCallback(ChangeEventCheck);
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

    private void ChangeEventCheck<T>(ChangeEvent<T> evt)
    {
        Debug.Log("Something changed!");
        CheckForAutoGenerate();
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
        //DisableFoldoutStyle(root.Q<Foldout>("buildFO"));
        buildStrandGroupButton = root.Q<Button>("buildStrandGroupBT");
        buildStrandGroupButton.clicked -= BuildStrandGroupButtonClicked;
        buildStrandGroupButton.clicked += BuildStrandGroupButtonClicked;
    }

    private void DrawRootSettings()
    {
        EnumField placement = root.Q<EnumField>("placementEF");
        EnumField placementPrimitive = root.Q<EnumField>("primitiveEF");
        ObjectField placementProvider = root.Q<ObjectField>("placementProviderOF");
        ObjectField placementMesh = root.Q<ObjectField>("placementMeshOF");
        MaskField placementMeshGroups = root.Q<MaskField>("placementGroupsMF");
        ObjectField mappedDensity = root.Q<ObjectField>("mappedDensityOF");
        ObjectField mappedDirection = root.Q<ObjectField>("mappedDirectionOF");
        ObjectField mappedParameters = root.Q<ObjectField>("mappedParametersOF");

        
        Refresh();

        placement.RegisterValueChangedCallback(evt =>  Refresh());

        void Refresh()
        {
            placementPrimitive.style.display = targetScript.settingsProcedural.placement == HairAsset.SettingsProcedural.PlacementType.Primitive ? DisplayStyle.Flex : DisplayStyle.None;
            placementProvider.style.display = targetScript.settingsProcedural.placement == HairAsset.SettingsProcedural.PlacementType.Custom ? DisplayStyle.Flex : DisplayStyle.None;
            placementMesh.style.display = targetScript.settingsProcedural.placement == HairAsset.SettingsProcedural.PlacementType.Mesh ? DisplayStyle.Flex : DisplayStyle.None;
            placementMeshGroups.style.display = targetScript.settingsProcedural.placement == HairAsset.SettingsProcedural.PlacementType.Mesh ? DisplayStyle.Flex : DisplayStyle.None;
            mappedDensity.style.display = targetScript.settingsProcedural.placement == HairAsset.SettingsProcedural.PlacementType.Mesh ? DisplayStyle.Flex : DisplayStyle.None;
            mappedDirection.style.display = targetScript.settingsProcedural.placement == HairAsset.SettingsProcedural.PlacementType.Mesh ? DisplayStyle.Flex : DisplayStyle.None;
            mappedParameters.style.display = targetScript.settingsProcedural.placement == HairAsset.SettingsProcedural.PlacementType.Mesh ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    private void DrawStrandsSettings()
    {
        SliderInt strandCount = root.Q<SliderInt>("strandcountSD");
        SliderInt strandParticleCount = root.Q<SliderInt>("strandParticleCountSD");
        Toggle strandLengthVariation = root.Q<Toggle>("strandLengthVariationTG");
        Slider strandLengthVariationAmount = root.Q<Slider>("strandLengthVariantionFactorSD");

        DisableFoldoutStyle(root.Q<Foldout>("lengthVariationFO"));

        strandCount.highValue = HairSim.MAX_STRAND_COUNT;
        strandCount.lowValue = HairSim.MIN_STRAND_COUNT;
        strandParticleCount.highValue = HairSim.MAX_STRAND_PARTICLE_COUNT;
        strandParticleCount.lowValue = HairSim.MIN_STRAND_PARTICLE_COUNT;

        SetLengthVariationSetting();

        strandLengthVariation.RegisterValueChangedCallback(evt => SetLengthVariationSetting());

        void SetLengthVariationSetting()
        {
            strandLengthVariationAmount.SetEnabled(targetScript.settingsProcedural.strandLengthVariation);
        }
    }

    private void DrawCurlSettings()
    {
        Toggle curl = root.Q<Toggle>("curlTG");
        Slider curlRadius = root.Q<Slider>("curlRadiusSD");
        Slider curlSlope = root.Q<Slider>("curlSlopeSD");
        Toggle curlVariation = root.Q<Toggle>("curlVariationTG");
        Slider curlVariationRadius = root.Q<Slider>("curlVariationRadiusSD");
        Slider curlVariationSlope = root.Q<Slider>("curlVariationSlopeSD");
        EnumField curlSamplingStrategy = root.Q<EnumField>("curlSamplingEF");
        
        DisableFoldoutStyle(root.Q<Foldout>("curlEnableFO"));
        DisableFoldoutStyle(root.Q<Foldout>("curlVariationFO"));

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
            curlSamplingStrategy.SetEnabled(targetScript.settingsProcedural.curl);
            curlRadius.SetEnabled(targetScript.settingsProcedural.curl);
            curlSlope.SetEnabled(targetScript.settingsProcedural.curl);
            curlVariation.SetEnabled(targetScript.settingsProcedural.curl);
            curlVariationRadius.SetEnabled(targetScript.settingsProcedural.curlVariation && targetScript.settingsProcedural.curl);
            curlVariationSlope.SetEnabled(targetScript.settingsProcedural.curlVariation && targetScript.settingsProcedural.curl);
        }
    }

    private void DrawLODSettings()
    {
        Toggle lodGeneration = root.Q<Toggle>("lodGenerationTG");
        EnumField lodClusteringMode = root.Q<EnumField>("lodClusteringEF");
        EnumField lodClusterAllocation = root.Q<EnumField>("clusterAllocationEF");
        EnumField lodClusterAllocationOrder = root.Q<EnumField>("clusterAllocationOrderEF");
        Toggle lodClusterRefinement = root.Q<Toggle>("clusterRefinementTG");
        SliderInt clusterRefinementValue = root.Q<SliderInt>("clusterRefinementValueSD");
        Toggle highLOD = root.Q<Toggle>("highLODTG");
        EnumField highLodMode = root.Q<EnumField>("HighLODEF");
        ListView highLodClusters = root.Q<ListView>("highLodClustersLV");

        DisableFoldoutStyle(root.Q<Foldout>("lodEnableFO"));
        DisableFoldoutStyle(root.Q<Foldout>("lodClusteringFO"));
        DisableFoldoutStyle(root.Q<Foldout>("refinementFO"));
        DisableFoldoutStyle(root.Q<Foldout>("baselodFO"));
        DisableFoldoutStyle(root.Q<Foldout>("highlodFO"));
        DisableFoldoutStyle(root.Q<Foldout>("highLodEnableFO"));
        
        SetLodSettings();

        lodGeneration.RegisterValueChangedCallback(evt =>
        {
            SetLodSettings();
        });

        void SetLodSettings()
        {
            root.Q<Foldout>("lodEnableFO").SetEnabled(targetScript.settingsBasic.kLODClusters);
        }
        
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
            clusterRefinementValue.SetEnabled(targetScript.settingsLODClusters.clusterRefinement);
        }

        SetLodModeSettings();

        highLOD.RegisterValueChangedCallback(evt =>
        {
            SetLodModeSettings();
        });

        void SetLodModeSettings()
        {
            root.Q<Foldout>("highLodEnableFO").SetEnabled(targetScript.settingsLODClusters.highLOD.highLOD);
            highLodClusters.SetEnabled(targetScript.settingsLODClusters.highLOD.highLOD);
            highLodClusters.style.display = targetScript.settingsLODClusters.highLOD.highLODMode == HairAsset.SettingsLODClusters.HighLODMode.Manual ? DisplayStyle.Flex : DisplayStyle.None;
        }

        highLodMode.RegisterValueChangedCallback(evt =>
        {
            SetLodModeSettings();
        });
    }
}
