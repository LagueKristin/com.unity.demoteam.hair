using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.DemoTeam.Hair
{
    public class HairOverlayPanel : Overlay
    {
        private VisualElement _root;
        private HairAsset _hairAsset;
        private HairInstance _hairInstance;
        private static GameObject _lastSelectedObject;

        private Editor _hairAssetEditor;

        public override VisualElement CreatePanelContent()
        {
            _root = new VisualElement();
            var selection = Selection.objects;
            foreach (var selectedObject in selection)
            {
                _hairInstance = (selectedObject as GameObject)?.GetComponent<HairInstance>();
                if (_hairInstance != null)
                {
                    _hairAsset = _hairInstance.strandGroupProviders[0].hairAsset;
                }
            }

            if (_hairAsset != null)
            {
                IMGUIContainer container = new IMGUIContainer(() =>
                {
                    DrawAssetEditor(_hairAsset,ref _hairAssetEditor);
                });
                _root.Add(container);
            }

            return _root;
        }
        
        private void DrawAssetEditor(HairAsset settings, ref Editor editor)
        {
            if (_hairAsset != null)
            {
                Editor.CreateCachedEditor(settings, null, ref editor);
                editor.OnInspectorGUI();
            }
        }
    }
}