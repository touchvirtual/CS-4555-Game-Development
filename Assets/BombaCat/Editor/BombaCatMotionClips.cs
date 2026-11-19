#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>Bakes articulated poses onto this cat's own generic skeleton; never moves its gameplay root.</summary>
public static class BombaCatMotionClips
{
    private const string Folder = "Assets/BombaCat/Animations";
    private sealed class Rig
    {
        public GameObject cat;
        public Transform[] bones;
        public Vector3[] positions, scales;
        public Quaternion[] rotations;
        public Dictionary<string, Transform> names;
        public Transform Bone(string name) => names[name];
        public void Reset()
        {
            for (int i = 0; i < bones.Length; i++)
            { bones[i].localPosition = positions[i]; bones[i].localRotation = rotations[i]; bones[i].localScale = scales[i]; }
        }
    }

    public static AnimationClip[] Build(AnimationClip idle)
    {
        Directory.CreateDirectory(Folder); AssetDatabase.Refresh();
        var cat = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Ladymito/Free_cat/Models/cat.fbx"));
        cat.hideFlags = HideFlags.HideAndDontSave;
        cat.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity); cat.transform.localScale = Vector3.one;
        var animator = cat.GetComponent<Animator>(); if (animator != null) animator.enabled = false;
        idle.SampleAnimation(cat, 0f);
        var bones = cat.GetComponentsInChildren<Transform>().Where(t => t != cat.transform).ToArray();
        var rig = new Rig { cat = cat, bones = bones, positions = bones.Select(t => t.localPosition).ToArray(),
            rotations = bones.Select(t => t.localRotation).ToArray(), scales = bones.Select(t => t.localScale).ToArray(),
            names = bones.GroupBy(t => t.name).ToDictionary(g => g.Key, g => g.First()) };
        try
        {
            var run = Bake(rig, "BombaCat_Run", .46f, true, (r,t) => RunPose(r,t));
            var jump = Bake(rig, "BombaCat_Jump", 1f, false, (r,t) => JumpPose(r,t));
            var land = Bake(rig, "BombaCat_Land", .20f, false, (r,t) => LandPose(r,t));
            AssetDatabase.SaveAssets(); return new[] { run, jump, land };
        }
        finally { UnityEngine.Object.DestroyImmediate(cat); }
    }

    private static AnimationClip Bake(Rig rig, string name, float duration, bool loop, Action<Rig,float> pose)
    {
        string path = Folder + "/" + name + ".anim";
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (clip == null) { clip = new AnimationClip(); AssetDatabase.CreateAsset(clip, path); }
        clip.ClearCurves(); clip.name = name; clip.frameRate = 60f;
        int frames = Mathf.CeilToInt(duration * 60f);
        var curves = new AnimationCurve[rig.bones.Length,7];
        for (int b = 0; b < rig.bones.Length; b++) for (int c = 0; c < 7; c++) curves[b,c] = new AnimationCurve();
        var prior = new Quaternion[rig.bones.Length];
        for (int f = 0; f <= frames; f++)
        {
            float t = (float)f / frames; rig.Reset(); pose(rig,t);
            for (int b = 0; b < rig.bones.Length; b++)
            {
                var bone = rig.bones[b]; var q = bone.localRotation;
                if (f > 0 && Quaternion.Dot(prior[b],q) < 0) q = new Quaternion(-q.x,-q.y,-q.z,-q.w);
                prior[b] = q; var p = bone.localPosition;
                float[] values = {p.x,p.y,p.z,q.x,q.y,q.z,q.w};
                for (int c = 0; c < 7; c++) curves[b,c].AddKey(t * duration, values[c]);
            }
        }
        string[] properties = {"m_LocalPosition.x","m_LocalPosition.y","m_LocalPosition.z",
            "m_LocalRotation.x","m_LocalRotation.y","m_LocalRotation.z","m_LocalRotation.w"};
        for (int b = 0; b < rig.bones.Length; b++)
            for (int c = 0; c < 7; c++)
            {
                var keys = curves[b,c].keys;
                if (keys.All(k => Mathf.Abs(k.value - keys[0].value) < .000001f))
                    curves[b,c] = AnimationCurve.Constant(0f,duration,keys[0].value);
                for (int k = 0; k < curves[b,c].length; k++)
                {
                    AnimationUtility.SetKeyLeftTangentMode(curves[b,c],k,AnimationUtility.TangentMode.ClampedAuto);
                    AnimationUtility.SetKeyRightTangentMode(curves[b,c],k,AnimationUtility.TangentMode.ClampedAuto);
                }
                AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(
                    AnimationUtility.CalculateTransformPath(rig.bones[b],rig.cat.transform),typeof(Transform),properties[c]), curves[b,c]);
            }
        clip.EnsureQuaternionContinuity();
        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop; settings.loopBlend = loop;
        settings.keepOriginalPositionXZ = true; settings.keepOriginalPositionY = true;
        AnimationUtility.SetAnimationClipSettings(clip,settings); EditorUtility.SetDirty(clip);
        return clip;
    }

    private static void Body(Rig r, float height, float pitch, float curl, float tail)
    {
        var root = r.Bone("root"); root.position += Vector3.up * height;
        root.rotation = Quaternion.AngleAxis(pitch,Vector3.right) * root.rotation;
        foreach (string name in new[] {"DEF_back.002","DEF_back.003","DEF_back.004"})
            Rotate(r.Bone(name),curl / 3f);
        Rotate(r.Bone("DEF_head"),-pitch - curl * .6f);
        for (int i=1;i<=5;i++) Rotate(r.Bone("DEF_tail.00"+i),tail * (1f - i * .09f));
    }
    private static void Rotate(Transform bone,float degrees)
    { bone.rotation = Quaternion.AngleAxis(degrees,Vector3.right) * bone.rotation; }

    private static void RunPose(Rig r,float t)
    {
        float wave = Mathf.Sin(t * Mathf.PI * 2f);
        // Gather and extend the spine with a slightly staggered pair of fore/hind contacts.
        Body(r, .004f + .007f * Mathf.Cos(t * Mathf.PI * 4f), wave * 5f, -wave * 9f, -wave * 6f);
        RunFoot(r,true,".L",t); RunFoot(r,true,".R",t+.10f);
        RunFoot(r,false,".L",t+.48f); RunFoot(r,false,".R",t+.58f);
    }
    private static void RunFoot(Rig r,bool front,string side,float phase)
    {
        float p = Mathf.Repeat(phase,1f), swing = Mathf.Clamp01((p-.42f)/.58f);
        float z = p < .42f ? Mathf.Lerp(.037f,-.037f,p/.42f) : Mathf.Lerp(-.037f,.037f,Mathf.SmoothStep(0,1,swing));
        float lift = p < .42f ? 0f : Mathf.Sin(swing * Mathf.PI) * (front ? .039f : .033f);
        Foot(r,front,side,z,lift,front ? -Mathf.Sin(swing*Mathf.PI)*18f : Mathf.Sin(swing*Mathf.PI)*12f);
    }

    private static float Envelope(float t,float[] times,float[] values)
    {
        for(int i=1;i<times.Length;i++) if(t<=times[i])
            return Mathf.Lerp(values[i-1],values[i],Mathf.SmoothStep(0f,1f,Mathf.InverseLerp(times[i-1],times[i],t)));
        return values[values.Length-1];
    }
    private static void JumpPose(Rig r,float t)
    {
        float[] keys={0f,.18f,.25f,.48f,.63f,.85f,1f};
        float crouch=Envelope(t,keys,new[]{0f,1f,0f,0f,0f,0f,0f});
        float tuck=Envelope(t,keys,new[]{0f,0f,0f,1f,1f,.3f,0f});
        float reach=Envelope(t,keys,new[]{0f,0f,1f,.15f,0f,.5f,.7f});
        float pitch=Envelope(t,keys,new[]{0f,4f,-12f,-5f,1f,9f,4f});
        Body(r,-.018f*crouch+.006f*reach,pitch,5f*tuck+5f*crouch,-8f*reach+11f*tuck);
        foreach(string side in new[]{".L",".R"})
        {
            Foot(r,true,side,.028f*reach-.015f*tuck,.025f*tuck, -18f*tuck);
            Foot(r,false,side,-.026f*reach+.012f*tuck,.015f*tuck,12f*tuck);
        }
    }
    private static void LandPose(Rig r,float t)
    {
        float settle = Envelope(t,new[]{0f,.25f,1f},new[]{0f,1f,0f});
        Body(r,-.021f*settle,5f*settle,8f*settle,-5f*settle);
        foreach(string side in new[]{".L",".R"})
        { Foot(r,true,side,.015f*(1f-t),0f,0f); Foot(r,false,side,-.006f*(1f-t),0f,0f); }
    }

    private static void Foot(Rig r,bool front,string side,float stride,float lift,float pawPitch)
    {
        Transform upper=r.Bone((front?"DEF_upperArm":"DEF_upperLeg")+side);
        Transform lower=r.Bone((front?"DEF_lowerArm":"DEF_lowerLeg")+side);
        Transform end=r.Bone((front?"DEF_hand":"DEF_foot")+side);
        var endRotation=end.rotation;
        float x=side==".L"?.018f:-.018f;
        Vector3 target=new Vector3(x,(front?.014f:.033f)+lift,(front?.054f:-.065f)+stride);
        Solve(upper,lower,end,target,front?Vector3.back:Vector3.forward);
        end.rotation=Quaternion.AngleAxis(pawPitch,Vector3.right)*endRotation;
    }
    private static void Solve(Transform upper,Transform lower,Transform end,Vector3 target,Vector3 bend)
    {
        Vector3 origin=upper.position;
        float a=Vector3.Distance(origin,lower.position),b=Vector3.Distance(lower.position,end.position);
        Vector3 delta=target-origin;
        float length=Mathf.Clamp(delta.magnitude,Mathf.Abs(a-b)+.0001f,(a+b)*.995f);
        Vector3 axis=delta.normalized;
        Vector3 pole=Vector3.ProjectOnPlane(bend,axis).normalized;
        float cosine=Mathf.Clamp((a*a+length*length-b*b)/(2f*a*length),-1f,1f);
        Vector3 joint=origin+axis*(cosine*a)+pole*(Mathf.Sqrt(1f-cosine*cosine)*a);
        upper.rotation=Quaternion.RotateTowards(upper.rotation,Quaternion.FromToRotation(lower.position-origin,joint-origin)*upper.rotation,48f);
        lower.rotation=Quaternion.RotateTowards(lower.rotation,Quaternion.FromToRotation(end.position-lower.position,origin+axis*length-lower.position)*lower.rotation,65f);
    }
}
#endif
