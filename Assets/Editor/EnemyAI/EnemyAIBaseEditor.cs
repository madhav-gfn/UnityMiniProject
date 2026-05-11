#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

[CustomEditor(typeof(EnemyAIBase), true)]
[CanEditMultipleObjects]
public class EnemyAIBaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Enemy AI Tools", EditorStyles.boldLabel);

        if (GUILayout.Button("Ensure Required Components"))
        {
            foreach (Object targetObject in targets)
            {
                EnemyAIBase enemy = (EnemyAIBase)targetObject;
                Undo.RegisterFullObjectHierarchyUndo(enemy.gameObject, "Ensure Enemy Components");
                enemy.EnsureRequiredComponents();
                EditorUtility.SetDirty(enemy);
            }
        }

        if (GUILayout.Button("Create Animator Controller & Assign"))
        {
            foreach (Object targetObject in targets)
            {
                EnemyAIBase enemy = (EnemyAIBase)targetObject;
                CreateControllerForEnemy(enemy);
            }
        }

        EditorGUILayout.EndVertical();
    }

    private static void CreateControllerForEnemy(EnemyAIBase enemy)
    {
        if (enemy == null)
        {
            return;
        }

        enemy.EnsureRequiredComponents();

        string folderPath = "Assets/Generated/EnemyAI";
        EnsureFolderExists(folderPath);

        string assetName = enemy.gameObject.name + "_EnemyAI.controller";
        string controllerPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(folderPath, assetName).Replace("\\", "/"));

        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        AnimatorControllerLayer layer = controller.layers[0];
        AnimatorStateMachine stateMachine = layer.stateMachine;

        EnsureParameter(controller, enemy.moveSpeedParameter, AnimatorControllerParameterType.Float);
        EnsureParameter(controller, enemy.attackTrigger, AnimatorControllerParameterType.Trigger);
        EnsureParameter(controller, enemy.takeDamageTrigger, AnimatorControllerParameterType.Trigger);
        EnsureParameter(controller, enemy.deathTrigger, AnimatorControllerParameterType.Trigger);

        AnimatorState idleState = AddState(stateMachine, "Idle", enemy.idleAnimation);
        AnimatorState forwardState = AddState(stateMachine, "Forward", enemy.forwardAnimation);
        AnimatorState damageState = AddState(stateMachine, "TakeDamage", enemy.takeDamageAnimation);
        AnimatorState attackState = AddState(stateMachine, "Attack", enemy.attackAnimation);
        AnimatorState deathState = AddState(stateMachine, "Death", enemy.deathAnimation);

        stateMachine.defaultState = idleState;

        if (forwardState != null)
        {
            AnimatorStateTransition idleToForward = idleState.AddTransition(forwardState);
            idleToForward.hasExitTime = false;
            idleToForward.duration = 0.1f;
            idleToForward.AddCondition(AnimatorConditionMode.Greater, 0.1f, enemy.moveSpeedParameter);

            AnimatorStateTransition forwardToIdle = forwardState.AddTransition(idleState);
            forwardToIdle.hasExitTime = false;
            forwardToIdle.duration = 0.1f;
            forwardToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, enemy.moveSpeedParameter);
        }

        if (damageState != null)
        {
            AnimatorStateTransition anyToDamage = stateMachine.AddAnyStateTransition(damageState);
            anyToDamage.hasExitTime = false;
            anyToDamage.duration = 0.05f;
            anyToDamage.AddCondition(AnimatorConditionMode.If, 0f, enemy.takeDamageTrigger);

            AnimatorStateTransition damageToIdle = damageState.AddTransition(idleState);
            damageToIdle.hasExitTime = true;
            damageToIdle.exitTime = 0.9f;
            damageToIdle.duration = 0.05f;
        }

        if (attackState != null)
        {
            AnimatorStateTransition anyToAttack = stateMachine.AddAnyStateTransition(attackState);
            anyToAttack.hasExitTime = false;
            anyToAttack.duration = 0.05f;
            anyToAttack.AddCondition(AnimatorConditionMode.If, 0f, enemy.attackTrigger);

            AnimatorStateTransition attackToIdle = attackState.AddTransition(idleState);
            attackToIdle.hasExitTime = true;
            attackToIdle.exitTime = 0.9f;
            attackToIdle.duration = 0.05f;
        }

        if (deathState != null)
        {
            AnimatorStateTransition anyToDeath = stateMachine.AddAnyStateTransition(deathState);
            anyToDeath.hasExitTime = false;
            anyToDeath.duration = 0.05f;
            anyToDeath.AddCondition(AnimatorConditionMode.If, 0f, enemy.deathTrigger);
        }

        enemy.GetComponent<Animator>().runtimeAnimatorController = controller;

        EditorUtility.SetDirty(enemy);
        EditorUtility.SetDirty(controller);
        EditorUtility.SetDirty(enemy.GetComponent<Animator>());
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Enemy AI: created animator controller at {controllerPath}", enemy);
    }

    private static AnimatorState AddState(AnimatorStateMachine stateMachine, string stateName, AnimationClip clip)
    {
        AnimatorState state = stateMachine.AddState(stateName);
        state.motion = clip;
        return state;
    }

    private static void EnsureParameter(AnimatorController controller, string parameterName, AnimatorControllerParameterType type)
    {
        if (string.IsNullOrEmpty(parameterName))
        {
            return;
        }

        foreach (AnimatorControllerParameter existing in controller.parameters)
        {
            if (existing.name == parameterName)
            {
                return;
            }
        }

        controller.AddParameter(parameterName, type);
    }

    private static void EnsureFolderExists(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath))
        {
            return;
        }

        string[] parts = folderPath.Split('/');
        string currentPath = parts[0];

        for (int i = 1; i < parts.Length; i++)
        {
            string nextPath = currentPath + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(nextPath))
            {
                AssetDatabase.CreateFolder(currentPath, parts[i]);
            }

            currentPath = nextPath;
        }
    }
}
#endif
