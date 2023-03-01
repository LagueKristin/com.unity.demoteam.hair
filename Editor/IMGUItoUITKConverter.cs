using UnityEditor.UIElements;

namespace Unity.DemoTeam.Hair
{
    using UnityEngine;
    using UnityEditor;
    using UnityEngine.UIElements;

    public static class IMGUItoUITKConverter
    {
        public static VisualElement Convert(SerializedObject serializedObject)
        {
            var root = new VisualElement();

            var iterator = serializedObject.GetIterator();
            while (iterator.NextVisible(true))
            {
                // Skip properties that are not visible or are built-in Unity properties
                if (!iterator.propertyPath.Contains("m_") && iterator.propertyType != SerializedPropertyType.ObjectReference)
                {
                    var uiElement = CreateUIElement(iterator);
                    if (uiElement != null)
                    {
                        root.Add(uiElement);
                    }
                }
            }

            return root;
        }

        private static VisualElement CreateUIElement(SerializedProperty property)
        {
            // Check if the property is of a supported type
            switch (property.propertyType)
            {
                case SerializedPropertyType.Boolean:
                    Toggle toggle =  new Toggle(property.displayName);
                    toggle.value = property.boolValue;
                    toggle.bindingPath = property.name;
                    return toggle;
                case SerializedPropertyType.Float:
                    FloatField floatField =  new FloatField(property.displayName);
                    floatField.bindingPath = property.name;
                    Debug.Log(property.name);
                    floatField.value = property.floatValue;
                    return floatField;
                case SerializedPropertyType.Integer:
                    return new IntegerField(property.displayName, property.intValue);
                case SerializedPropertyType.Enum:
                    EnumField enumField =  new EnumField(property.displayName);
                    enumField.bindingPath = property.propertyPath;
                    return enumField;
                default:
                    return null;
            }
        }
    }
}