using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Adrenaline
{
    [CustomEditor(typeof(GameObstacle), editorForChildClasses: true)]
    [CanEditMultipleObjects]
    public class GameObstacleEditor : Editor
    {
        protected GameObstacle obstacle = null;
        SerializedProperty hardObstacle, makeInactiveAfterReceiverDeath, justInteract, interactionCooldownTime, oneHitKill, damageAmount, useTagBasedDetection, 
            receiverTags, useDelay, delayAmount, startEffects, interactEffects, receiverDeathEffects, damageReceiverEffects, 
            onStart, onInteract, onReceiverDeath, onReceiverDamage;
        protected Rigidbody rgd = null;
        static bool basicSettingFold = false, effectFold = false, unityEventFold = false;

        readonly string[] defaultProps = new string[] { nameof(hardObstacle), nameof(makeInactiveAfterReceiverDeath), nameof(justInteract),
            nameof(interactionCooldownTime), nameof(oneHitKill), nameof(damageAmount), nameof(useTagBasedDetection), nameof(receiverTags), nameof(useDelay), 
            nameof(delayAmount), nameof(startEffects), nameof(interactEffects), nameof(receiverDeathEffects), 
            nameof(damageReceiverEffects), nameof(onStart), nameof(onInteract), nameof(onReceiverDeath), nameof(onReceiverDamage)
        };

        void OnEnable()
        {
            obstacle = target as GameObstacle;
            rgd = obstacle.GetComponent<Rigidbody>();
            hardObstacle = serializedObject.FindProperty(nameof(hardObstacle));
            makeInactiveAfterReceiverDeath = serializedObject.FindProperty(nameof(makeInactiveAfterReceiverDeath));
            justInteract = serializedObject.FindProperty(nameof(justInteract));
            interactionCooldownTime = serializedObject.FindProperty(nameof(interactionCooldownTime));
            oneHitKill = serializedObject.FindProperty(nameof(oneHitKill));
            damageAmount = serializedObject.FindProperty(nameof(damageAmount));
            useTagBasedDetection = serializedObject.FindProperty(nameof(useTagBasedDetection));
            receiverTags = serializedObject.FindProperty(nameof(receiverTags));
            useDelay = serializedObject.FindProperty(nameof(useDelay));
            delayAmount = serializedObject.FindProperty(nameof(delayAmount));
            
            startEffects = serializedObject.FindProperty(nameof(startEffects));
            interactEffects = serializedObject.FindProperty(nameof(interactEffects));
            receiverDeathEffects = serializedObject.FindProperty(nameof(receiverDeathEffects));
            damageReceiverEffects = serializedObject.FindProperty(nameof(damageReceiverEffects));
            onStart = serializedObject.FindProperty(nameof(onStart));
            onInteract = serializedObject.FindProperty(nameof(onInteract));
            onReceiverDeath = serializedObject.FindProperty(nameof(onReceiverDeath));
            onReceiverDamage = serializedObject.FindProperty(nameof(onReceiverDamage));
            OnStartEditor();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            bool hasOneVolume = false, hasOneHardCol = false;
            var cols = obstacle.GetComponentsInChildren<Collider>();
            if (cols != null && cols.Length > 0)
            {
                for (int i = 0; i < cols.Length; i++)
                {
                    var c = cols[i];
                    if (c == null) { continue; }
                    if (c.isTrigger)
                    {
                        hasOneVolume = true;
                    }
                    else if (!c.isTrigger)
                    {
                        hasOneHardCol = true;
                    }
                }
                if (hardObstacle.boolValue && !hasOneHardCol)
                {
                    EditorGUILayout.HelpBox("You set this as hard obstacle, yet there is no collider within." +
                        " Trigger(s) will not work hard obstacle.", MessageType.Error);
                }
                if (!hardObstacle.boolValue && !hasOneVolume)
                {
                    EditorGUILayout.HelpBox("You set this as volume/trigger obstacle, yet there is no volume/trigger within." +
                        " You should add one or more trigger in this gameobject or inside.", MessageType.Warning);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No collider or trigger is exists. The obstacle will not work", MessageType.Error);
            }

            DrawPropertiesExcluding(serializedObject, defaultProps);
            OnUpdateEditor();

            basicSettingFold = EditorGUILayout.Foldout(basicSettingFold, "Basic Settings");
            if (basicSettingFold)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.HelpBox("Hard obstacle works on non-triggered colliders.", MessageType.None);
                EditorGUILayout.PropertyField(hardObstacle);

                if (rgd != null)
                {
                    var setPrefabDirty = false;
                    if (hardObstacle.boolValue && rgd.isKinematic) { rgd.isKinematic = false; setPrefabDirty = true; }
                    else if (!hardObstacle.boolValue && !rgd.isKinematic) { rgd.isKinematic = true; setPrefabDirty = true; }
                    if (setPrefabDirty)
                    {
                        PrefabUtility.RecordPrefabInstancePropertyModifications(rgd);
                        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                    }
                }

                EditorGUILayout.PropertyField(justInteract);
                if (!justInteract.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(makeInactiveAfterReceiverDeath);
                    EditorGUILayout.PropertyField(oneHitKill);
                    if (!oneHitKill.boolValue)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(damageAmount);
                        EditorGUI.indentLevel--;
                    }
                    EditorGUI.indentLevel--;
                }
                else
                {
                    EditorGUILayout.PropertyField(interactionCooldownTime);
                }

                EditorGUILayout.PropertyField(useTagBasedDetection);
                if (useTagBasedDetection.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(receiverTags);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.PropertyField(useDelay);
                if (useDelay.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(delayAmount);
                    EditorGUI.indentLevel--;
                }

                effectFold = EditorGUILayout.Foldout(effectFold, "Effects");
                if(effectFold)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(startEffects);
                    EditorGUILayout.PropertyField(receiverDeathEffects);
                    EditorGUILayout.PropertyField(damageReceiverEffects);
                    if (justInteract.boolValue)
                    {
                        EditorGUILayout.PropertyField(interactEffects);
                    }
                    EditorGUI.indentLevel--;
                }

                unityEventFold = EditorGUILayout.Foldout(unityEventFold, "Unity Events");
                if (unityEventFold)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(onStart);
                    EditorGUILayout.PropertyField(onReceiverDeath);
                    EditorGUILayout.PropertyField(onReceiverDamage);
                    if (justInteract.boolValue)
                    {
                        EditorGUILayout.PropertyField(onInteract);
                    }
                    EditorGUI.indentLevel--;
                }


                EditorGUI.indentLevel--;
            }
            
            serializedObject.ApplyModifiedProperties();
        }
        protected virtual void OnStartEditor() { }
        protected virtual void OnUpdateEditor() { }
    }
}