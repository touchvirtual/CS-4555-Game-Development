#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
public static class BombaCatMotionSetup
{
    private const string ModelPath = "Assets/Ladymito/Free_cat/Models/cat.fbx";
    private const string ControllerPath = "Assets/BombaCat/Resources/BombaCatLocomotion.controller";
    [MenuItem("Tools/BombaCat/Build Cat Locomotion")]
    public static void Build()
    {
        var clips = AssetDatabase.LoadAllAssetsAtPath(ModelPath).OfType<AnimationClip>()
            .Where(c => !c.name.StartsWith("__preview__", StringComparison.Ordinal)).ToArray();
        var idle = Find(clips, "idle"); var walk = Find(clips, "walk");
        if (idle == null || walk == null) throw new InvalidOperationException("Native cat idle/walk clips missing.");
        var authored = BombaCatMotionClips.Build(idle);
        Directory.CreateDirectory(Path.GetDirectoryName(ControllerPath)); AssetDatabase.Refresh();
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null) controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        else
        {
            var old = AssetDatabase.LoadAllAssetsAtPath(ControllerPath);
            controller.layers = Array.Empty<AnimatorControllerLayer>();
            controller.parameters = Array.Empty<AnimatorControllerParameter>();
            foreach (var item in old) if (item != controller && item != null) UnityEngine.Object.DestroyImmediate(item, true);
            controller.AddLayer("Base Layer");
        }
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("WalkRate", AnimatorControllerParameterType.Float);
        controller.AddParameter("Grounded", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Running", AnimatorControllerParameterType.Bool);
        controller.AddParameter("RunRate", AnimatorControllerParameterType.Float);
        controller.AddParameter("Jumping", AnimatorControllerParameterType.Bool);
        controller.AddParameter("JumpTime", AnimatorControllerParameterType.Float);
        var parameters = controller.parameters;
        foreach (var p in parameters)
        {
            if (p.name == "WalkRate" || p.name == "RunRate") p.defaultFloat = 1f;
            if (p.name == "Grounded") p.defaultBool = true;
        }
        controller.parameters = parameters;
        var machine = controller.layers[0].stateMachine;
        var grounded = machine.AddState("Grounded locomotion");
        grounded.writeDefaultValues = true;
        grounded.speedParameter = "WalkRate"; grounded.speedParameterActive = true;
        var tree = new BlendTree { name = "BombaCat Idle Walk", blendType = BlendTreeType.Simple1D,
            blendParameter = "Speed", useAutomaticThresholds = false };
        AssetDatabase.AddObjectToAsset(tree, controller);
        tree.AddChild(idle, 0f); tree.AddChild(walk, 0.18f);
        var children = tree.children; children[0].timeScale = 1f;
        children[1].timeScale = Mathf.Max(0.01f, walk.length) * 5f / 2.8f;
        tree.children = children; grounded.motion = tree; machine.defaultState = grounded;
        var running = machine.AddState("Running gait"); running.motion = authored[0];
        running.speedParameter = "RunRate"; running.speedParameterActive = true;
        Transition(grounded,running,.12f).AddCondition(AnimatorConditionMode.If,0,"Running");
        Transition(running,grounded,.12f).AddCondition(AnimatorConditionMode.IfNot,0,"Running");
        var airborne = machine.AddState("Articulated jump"); airborne.motion = authored[1];
        airborne.timeParameter = "JumpTime"; airborne.timeParameterActive = true;
        // A physics-driven phase holds the tuck at the apex and extends during descent.
        var takeoff = machine.AddAnyStateTransition(airborne);
        takeoff.canTransitionToSelf = false; takeoff.hasExitTime = false;
        takeoff.hasFixedDuration = true; takeoff.duration = .045f;
        takeoff.AddCondition(AnimatorConditionMode.If,0,"Jumping");
        var landing = machine.AddState("Landing compression"); landing.motion = authored[2];
        var touch = Transition(airborne,landing,.04f);
        touch.AddCondition(AnimatorConditionMode.If,0,"Grounded");
        touch.AddCondition(AnimatorConditionMode.IfNot,0,"Jumping");
        var recover = Transition(landing,grounded,.07f); recover.hasExitTime = true; recover.exitTime = .82f;
        EditorUtility.SetDirty(tree); EditorUtility.SetDirty(controller); AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(ControllerPath, ImportAssetOptions.ForceUpdate);
        Debug.Log("BombaCat locomotion ready: native idle/walk plus articulated run, jump and landing.");
    }
    private static AnimatorStateTransition Transition(AnimatorState from, AnimatorState to, float duration)
    {
        var transition = from.AddTransition(to); transition.hasExitTime = false;
        transition.hasFixedDuration = true; transition.duration = duration;
        return transition;
    }
    private static AnimationClip Find(AnimationClip[] clips, string name)
    {
        return clips.FirstOrDefault(c => string.Equals(c.name, name, StringComparison.OrdinalIgnoreCase)
            || c.name.EndsWith("|" + name, StringComparison.OrdinalIgnoreCase));
    }
}
#endif
