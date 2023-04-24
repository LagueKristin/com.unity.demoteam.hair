using System.Collections.Generic;
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

        root.Q<EnumField>("memoryLayoutEF").Bind(serializedObject);

        Toggle lodGeneration = root.Q<Toggle>("lodGenerationTG");
        lodGeneration.Bind(serializedObject);

        EnumField lodClusteringMode = root.Q<EnumField>("lodClusteringEF");
        lodClusteringMode.Bind(serializedObject);

        lodGeneration.RegisterValueChangedCallback(evt => lodClusteringMode.style.display = targetScript.settingsBasic.kLODClusters ? DisplayStyle.Flex : DisplayStyle.None);

        EnumField placement = root.Q<EnumField>("placementEF");
        placement.Bind(serializedObject);

        EnumField placementPrimitive = root.Q<EnumField>("primitiveEF");
        placementPrimitive.Bind(serializedObject);

        ObjectField placementProvider = root.Q<ObjectField>("placementProviderOF");
        placementProvider.Bind(serializedObject);

        ObjectField placementMesh = root.Q<ObjectField>("placementMeshOF");
        placementMesh.Bind(serializedObject);

        MaskField placementMeshGroups = root.Q<MaskField>("placementGroupsMF");
        placementMeshGroups.Bind(serializedObject);

        ObjectField mappedDensity = root.Q<ObjectField>("mappedDensityOF");
        mappedDensity.Bind(serializedObject);

        ObjectField mappedDirection = root.Q<ObjectField>("mappedDirectionOF");
        mappedDirection.Bind(serializedObject);

        ObjectField mappedParameters = root.Q<ObjectField>("mappedParametersOF");
        mappedParameters.Bind(serializedObject);

        placement.RegisterValueChangedCallback(evt =>
        {
            Debug.Log("Should repaint now");
            placementPrimitive.style.display = targetScript.settingsProcedural.placement == HairAsset.SettingsProcedural.PlacementType.Primitive ? DisplayStyle.Flex : DisplayStyle.None;
            placementProvider.style.display = targetScript.settingsProcedural.placement == HairAsset.SettingsProcedural.PlacementType.Custom ? DisplayStyle.Flex : DisplayStyle.None;
            placementMesh.style.display = targetScript.settingsProcedural.placement == HairAsset.SettingsProcedural.PlacementType.Mesh ? DisplayStyle.Flex : DisplayStyle.None;
            placementMeshGroups.style.display = targetScript.settingsProcedural.placement == HairAsset.SettingsProcedural.PlacementType.Mesh ? DisplayStyle.Flex : DisplayStyle.None;
            mappedDensity.style.display = targetScript.settingsProcedural.placement == HairAsset.SettingsProcedural.PlacementType.Mesh ? DisplayStyle.Flex : DisplayStyle.None;
            mappedDirection.style.display = targetScript.settingsProcedural.placement == HairAsset.SettingsProcedural.PlacementType.Mesh ? DisplayStyle.Flex : DisplayStyle.None;
            mappedParameters.style.display = targetScript.settingsProcedural.placement == HairAsset.SettingsProcedural.PlacementType.Mesh ? DisplayStyle.Flex : DisplayStyle.None;
        });
        
        SliderInt strandCount = root.Q<SliderInt>("strandcountSD");
        strandCount.highValue = HairSim.MAX_STRAND_COUNT;
        strandCount.lowValue = HairSim.MIN_STRAND_COUNT;
        strandCount.Bind(serializedObject);

        SliderInt strandParticleCount = root.Q<SliderInt>("strandParticleCountSD");
        strandParticleCount.highValue = HairSim.MAX_STRAND_PARTICLE_COUNT;
        strandParticleCount.lowValue = HairSim.MIN_STRAND_PARTICLE_COUNT;
        strandParticleCount.Bind(serializedObject);

        Slider strandLength = root.Q<Slider>("strandLengthSD");
        strandLength.Bind(serializedObject);

        Toggle strandLengthVariation = root.Q<Toggle>("strandLengthVariationTG");
        strandLengthVariation.Bind(serializedObject);

        Slider strandLengthVariationAmount = root.Q<Slider>("strandLengthVariantionFactorSD");
        strandLengthVariationAmount.Bind(serializedObject);

        strandLengthVariation.RegisterValueChangedCallback(evt => strandLengthVariationAmount.style.display = targetScript.settingsProcedural.strandLengthVariation ? DisplayStyle.Flex : DisplayStyle.None);

        Toggle curl = root.Q<Toggle>("curlTG");
        curl.Bind(serializedObject);

        Slider curlRadius = root.Q<Slider>("curlRadiusSD");
        curlRadius.highValue = 10.0f;
        curlRadius.lowValue = 0.0f;
        curlRadius.Bind(serializedObject);

        Slider curlSlope = root.Q<Slider>("curlSlopeSD");
        curlSlope.highValue = 1.0f;
        curlSlope.lowValue = 0.0f;
        curlSlope.Bind(serializedObject);

        Toggle curlVariation = root.Q<Toggle>("curlVariationTG");
        curlVariation.Bind(serializedObject);
        
        Slider curlVariationRadius = root.Q<Slider>("curlVariationRadiusSD");
        curlVariationRadius.highValue = 1.0f;
        curlVariationRadius.lowValue = 0.0f;
        curlVariationRadius.Bind(serializedObject);

        Slider curlVariationSlope = root.Q<Slider>("curlVariationSlopeSD");
        curlVariationSlope.highValue = 1.0f;
        curlVariationSlope.lowValue = 0.0f;
        curlVariationSlope.Bind(serializedObject);

        EnumField curlSamplingStrategy = root.Q<EnumField>("curlSamplingEF");
        curlSamplingStrategy.Bind(serializedObject);

        curl.RegisterCallback<ChangeEvent<bool>>(evt => RedrawCurlSettings());
        curlVariation.RegisterValueChangedCallback(evt => RedrawCurlSettings());
        
        void RedrawCurlSettings()
        {
            curlRadius.style.display = targetScript.settingsProcedural.curl ? DisplayStyle.Flex : DisplayStyle.None;
            curlSlope.style.display = targetScript.settingsProcedural.curl ? DisplayStyle.Flex : DisplayStyle.None;
            curlVariation.style.display = targetScript.settingsProcedural.curl ? DisplayStyle.Flex : DisplayStyle.None;
            curlSamplingStrategy.style.display = targetScript.settingsProcedural.curl ? DisplayStyle.Flex : DisplayStyle.None;
            curlVariationRadius.style.display = targetScript.settingsProcedural.curlVariation && targetScript.settingsProcedural.curl ? DisplayStyle.Flex : DisplayStyle.None;
            curlVariationSlope.style.display = targetScript.settingsProcedural.curlVariation && targetScript.settingsProcedural.curl ? DisplayStyle.Flex : DisplayStyle.None;
        }
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
            AssetDatabase.CreateAsset(addedAsset, AssetDatabase.GenerateUniqueAssetPath(folderPath + "/HairAsset.asset"));
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
