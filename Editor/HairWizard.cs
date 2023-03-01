using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.DemoTeam.Hair
{
    public class HairWizard : EditorWindow
    {

        [SerializeField] private VisualTreeAsset hairWizardUXML;

        private ObjectField _geometryAlembic;
        private ObjectField _hairDataAlembic;
        private Button _generateButton;
        
        [MenuItem("Tools/Digital Characters/Hair Import Wizard")]
        public static void ShowMyEditor()
        {
            // This method is called when the user selects the menu item in the Editor
            EditorWindow wnd = GetWindow<HairWizard>();
            wnd.titleContent = new GUIContent("Hair Import Wizard");
        }
        
        public void CreateGUI()
        {
            hairWizardUXML.CloneTree(rootVisualElement);

            _geometryAlembic = rootVisualElement.Q<ObjectField>("geometryAlembic");
            _hairDataAlembic = rootVisualElement.Q<ObjectField>("hairDataAlembic");
            _generateButton = rootVisualElement.Q<Button>("generateButton");
            
            _generateButton.SetEnabled(_geometryAlembic.value != null || _hairDataAlembic.value != null);
            
            _generateButton.clicked -= () => SetupAsset();
            _generateButton.clicked += () => SetupAsset();

        }

        private void SetupAsset()
        {
            if (_geometryAlembic.value == null || _hairDataAlembic.value == null)
            {
                return;
            }
            
            
        }

    }
}