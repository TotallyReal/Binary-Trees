using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

/// <summary>
/// Instead of the variable, draws a button which runs a function instead.
/// The function can return a single object or IEnumerable<Object> which are then marked as dirty.
/// If it returns void, the object containing it is marked as dirty.
/// </summary>
[CustomPropertyDrawer(typeof(ButtonInsteadAttribute))]
public class ButtonInsteadDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight + 4;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var attr = (ButtonInsteadAttribute)attribute;

        if (GUI.Button(position, attr.ButtonLabel))
        {
            object target = property.serializedObject.targetObject;
            var method = target.GetType().GetMethod(attr.MethodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            if (method == null)
            {
                Debug.LogWarning($"[ButtonInstead] Method '{attr.MethodName}' not found on {target.GetType().Name}");
                return;
            }

            object result = method.Invoke(target, null);
            if (Application.isPlaying)
                return;

            // Handle returned dirty objects
            List<Object> dirtyObjects = new();

            if (result is Object singleObj)
            {
                dirtyObjects.Add(singleObj);
            }
            else if (result is IEnumerable<Object> multipleObjs)
            {
                dirtyObjects.AddRange(multipleObjs);
            }

            var self = target as UnityEngine.Object;
            if (dirtyObjects.Count == 0 && self!= null)
            {
                dirtyObjects.Add(self);
            }

            foreach (var obj in dirtyObjects)
            {
                if (obj == null) continue;

                EditorUtility.SetDirty(obj);

                if (obj is GameObject go)
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(go.scene);
                else if (obj is Component comp)
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(comp.gameObject.scene);
                
            }
        }

    }
}
