using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public  class SteeringBehavior  
{
    public float Weight = 1;
    public virtual Vector2 CalculateSteering(BoidBase boid, Vector2 target) { return Vector2.zero; }
    public virtual void OnDrawGUI(BoidBase boid) {}
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(SteeringBehavior), true)] // 'true' enables inheritance
public class SteeringBehaviourDrawer : PropertyDrawer
{
     public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Retrieve the type name using managedReferenceFullTypename.
        // managedReferenceFullTypename usually comes in the format "AssemblyName TypeName"
        string typeName = property.managedReferenceFullTypename;
        if (!string.IsNullOrEmpty(typeName))
        {
            string[] split = typeName.Split(' ');
            if (split.Length >= 2)
                typeName = split[1]; // Use only the TypeName portion.
        }
        else
        {
            typeName = "null";
        }

        // Calculate the height of the type label (usually one line).
        float typeLabelHeight = EditorGUIUtility.singleLineHeight;
        // Calculate the height for the rest of the property (its children)
        float propertyHeight = EditorGUI.GetPropertyHeight(property, true);

        // Draw the type name label at the top.
        Rect typeLabelRect = new Rect(position.x, position.y, position.width, typeLabelHeight);
        EditorGUI.LabelField(typeLabelRect, $"[{typeName}]");

        // Draw the actual property below the type label.
        Rect propertyRect = new Rect(position.x, position.y + typeLabelHeight, position.width, propertyHeight);
        EditorGUI.PropertyField(propertyRect, property, GUIContent.none, true);

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Total height is the type label plus the height of the property (all children)
        return EditorGUIUtility.singleLineHeight + EditorGUI.GetPropertyHeight(property, true);
    }
}
#endif
