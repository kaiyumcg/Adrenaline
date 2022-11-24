using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Adrenaline
{
    [CustomEditor(typeof(ObstacleReceiver), editorForChildClasses: true)]
    [CanEditMultipleObjects]
    public class ObstacleReceiverEditor : Editor
    {
        ObstacleReceiver owner = null;
        SerializedProperty ownerActor, useTagBasedRestriction, allowableObstacleTags;

        void OnEnable()
        {
            owner = target as ObstacleReceiver;
            ownerActor = serializedObject.FindProperty("ownerActor");
            useTagBasedRestriction = serializedObject.FindProperty("useTagBasedRestriction");
            allowableObstacleTags = serializedObject.FindProperty("allowableObstacleTags");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(ownerActor);
            if (owner.OwnerActorScript == null)
            {
                EditorGUILayout.HelpBox("No owner script. It will try to search in parent",
                 MessageType.Info);
            }
            else if (!(owner.OwnerActorScript is IObstacleReceiverActor))
            {
                EditorGUILayout.HelpBox("The owner script does not implement 'IObstacleReceiverActor' interface!", 
                    MessageType.Error);
            }
            EditorGUILayout.PropertyField(useTagBasedRestriction);
            if (useTagBasedRestriction.boolValue)
            {
                EditorGUILayout.PropertyField(allowableObstacleTags);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}