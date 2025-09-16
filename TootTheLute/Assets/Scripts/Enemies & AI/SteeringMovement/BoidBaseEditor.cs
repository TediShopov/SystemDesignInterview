
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BoidBase))]
public class BoidBaseEditor : Editor
{
    SerializedProperty Behaviors;
    private void OnEnable()
    {
        Behaviors = serializedObject.FindProperty("Behaviors");

    }
    public override void OnInspectorGUI()
    {
        //serializedObject.Update();
        //EditorGUILayout.PropertyField(boidBase);
        serializedObject.Update();

        base.OnInspectorGUI();

        EditorGUILayout.LabelField("Derived Class Inspector", EditorStyles.boldLabel);
        //UpdateList();
        BoidBase boidBase = (BoidBase)target;
        // Hacky hack for demo
        if (GUILayout.Button("Add Seek"))
        {
            boidBase.Behaviors.Add(new Seek());
        }
        if (GUILayout.Button("Add Separate"))
        {
            boidBase.Behaviors.Add(new Separation());
        }
        if (GUILayout.Button("Add Flee"))
        {
            boidBase.Behaviors.Add(new Flee());
        }
        if (GUILayout.Button("Add ObstacleAvoidance"))
        {
            boidBase.Behaviors.Add(new ObstacleAvoidance());
        }
    }
    void UpdateList()
    {
        for (int i = 0;i < Behaviors.arraySize;i++)
        {


            SerializedProperty element =  Behaviors.GetArrayElementAtIndex(i);
            if (element.GetType() == typeof(Seek))
            {
                EditorGUILayout.LabelField(typeof(Seek).Name);
                EditorGUILayout.PropertyField(element,true);
            }
            if (element.GetType() == typeof(Separation))
            { 
                EditorGUILayout.LabelField(typeof(Separation).Name);
                EditorGUILayout.PropertyField(element,true);
            }
            if (element.GetType() == typeof(Flee))
            {
                EditorGUILayout.LabelField(typeof(Flee).Name);
                EditorGUILayout.PropertyField(element,true);
            }
            if (element.GetType() == typeof(ObstacleAvoidance))
            {
                EditorGUILayout.LabelField(typeof(ObstacleAvoidance).Name);
                EditorGUILayout.PropertyField(element,true);
            }
        }
    }
}
#endif
