using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace GrimmsVenture.Animation;

/// <summary>Um passo de animacao: osso, tempo e pose local.</summary>
public class Keyframe
{
    public Keyframe() { }
    public Keyframe(int bone, TimeSpan time, Matrix transform)
    {
        Bone = bone; Time = time; Transform = transform;
    }

    [ContentSerializer] public int Bone { get; set; }
    [ContentSerializer] public TimeSpan Time { get; set; }
    [ContentSerializer] public Matrix Transform { get; set; }
}

/// <summary>Uma clip = sequencia de keyframes + duracao.</summary>
public class AnimationClip
{
    public AnimationClip() { }
    public AnimationClip(TimeSpan duration, List<Keyframe> keyframes)
    {
        Duration = duration; Keyframes = keyframes;
    }

    [ContentSerializer] public TimeSpan Duration { get; set; }
    [ContentSerializer] public List<Keyframe> Keyframes { get; set; } = new List<Keyframe>();
}

/// <summary>Tudo que o pipeline extrai do FBX e guarda no Tag do Model.</summary>
public class SkinningData
{
    public SkinningData() { } // usado na desserializacao
    public SkinningData(Dictionary<string, AnimationClip> clips,
                        List<Matrix> bindPose, List<Matrix> inverseBindPose,
                        List<int> hierarchy)
    {
        AnimationClips = clips;
        BindPose = bindPose;
        InverseBindPose = inverseBindPose;
        SkeletonHierarchy = hierarchy;
    }

    [ContentSerializer] public Dictionary<string, AnimationClip> AnimationClips { get; set; } = new Dictionary<string, AnimationClip>();
    [ContentSerializer] public List<Matrix> BindPose { get; set; } = new List<Matrix>();
    [ContentSerializer] public List<Matrix> InverseBindPose { get; set; } = new List<Matrix>();
    [ContentSerializer] public List<int> SkeletonHierarchy { get; set; } = new List<int>();

    public AnimationClip GetClip(string name)
    {
        AnimationClip c;
        return AnimationClips.TryGetValue(name, out c) ? c : null;
    }
}
