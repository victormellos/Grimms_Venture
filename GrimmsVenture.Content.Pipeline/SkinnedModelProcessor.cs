using System;
using System.Collections.Generic;
using GrimmsVenture.Animation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace GrimmsVenture.Content.Pipeline;

/// <summary>
/// Igual ao ModelProcessor padrao, MAS guarda bind pose + animacoes no Tag.
/// Escrito para a API da MonoGame 3.8.4 (FindSkeleton/FlattenSkeleton no
/// MeshHelper; AnimationChannel e colecao de keyframes com nome do osso
/// na chave do dicionario Channels).
/// </summary>
[ContentProcessor(DisplayName = "Skinned Model - GrimmsVenture")]
public class SkinnedModelProcessor : ModelProcessor
{
    public override ModelContent Process(NodeContent input, ContentProcessorContext context)
    {
        ValidateMeshes(input, context);

        BoneContent skeleton = MeshHelper.FindSkeleton(input);
        if (skeleton == null)
        {
            // Nao aborta: o modelo renderiza estatico na bind pose.
            // A reexportacao do Blender (meshes parenteados ao rig com
            // automatic weights) traz o skeleton de volta.
            context.Logger.LogWarning(null, null,
                "Armature/rig nao encontrado — Grimm estatico na bind pose. " +
                "Reexporte do Blender com os meshes parenteados ao rig (automatic weights).");
            ModelContent bare = base.Process(input, context);
            bare.Tag = new SkinningData(new Dictionary<string, AnimationClip>(),
                new List<Matrix>(), new List<Matrix>(), new List<int>());
            return bare;
        }

        FlattenTransforms(input, skeleton);

        IList<BoneContent> bones = MeshHelper.FlattenSkeleton(skeleton);

        // nome do osso -> indice (aceita nome bruto e sem sufixo "|Kakuteru_RigPose")
        var boneLookup = new Dictionary<string, int>();
        for (int i = 0; i < bones.Count; i++)
        {
            boneLookup[bones[i].Name] = i;
            string clean = CleanName(bones[i].Name);
            if (!boneLookup.ContainsKey(clean))
                boneLookup[clean] = i;
        }

        var bindPose = new List<Matrix>();
        var inverseBindPose = new List<Matrix>();
        var hierarchy = new List<int>();
        foreach (BoneContent bone in bones)
        {
            bindPose.Add(bone.Transform);
            inverseBindPose.Add(Matrix.Invert(bone.Transform));
            BoneContent parent = bone.Parent as BoneContent;
            hierarchy.Add(parent != null ? bones.IndexOf(parent) : -1);
        }

        var clips = new Dictionary<string, AnimationClip>();
        foreach (string rawName in input.Animations.Keys)
        {
            string name = CleanName(rawName); // "Idle|Kakuteru_RigPose" -> "Idle"
            if (!clips.ContainsKey(name))
                clips[name] = ProcessAnimation(input.Animations[rawName], boneLookup);
        }

        ModelContent model = base.Process(input, context);
        model.Tag = new SkinningData(clips, bindPose, inverseBindPose, hierarchy);
        return model;
    }

    static string CleanName(string raw)
        => raw.Split('|')[0].Trim();

    static AnimationClip ProcessAnimation(AnimationContent animation, Dictionary<string, int> boneLookup)
    {
        var keyframes = new List<Keyframe>();
        foreach (var channel in animation.Channels) // KeyValuePair<string, AnimationChannel>
        {
            int boneIndex;
            if (!boneLookup.TryGetValue(channel.Key, out boneIndex) &&
                !boneLookup.TryGetValue(CleanName(channel.Key), out boneIndex))
                continue; // canal que nao corresponde a nenhum osso do skeleton

            foreach (AnimationKeyframe kf in channel.Value)
                keyframes.Add(new Keyframe(boneIndex, kf.Time, kf.Transform));
        }
        keyframes.Sort((a, b) => a.Time.CompareTo(b.Time));

        if (keyframes.Count == 0)
            throw new InvalidContentException("Animacao '" + animation.Name + "' sem keyframes.");

        return new AnimationClip(animation.Duration, keyframes);
    }

    static void ValidateMeshes(NodeContent node, ContentProcessorContext context)
    {
        if (node is MeshContent mesh)
        {
            foreach (GeometryContent geo in mesh.Geometry)
            {
                bool hasWeights = geo.Vertices.Channels.Contains(VertexChannelNames.Weights());
                if (!hasWeights)
                    context.Logger.LogWarning(null, null,
                        "Mesh '{0}' sem pesos de skinning — vai renderizar estatico na bind pose. " +
                        "No Blender: parente o mesh ao rig (automatic weights) e reexporte.",
                        mesh.Name);
            }
        }
        foreach (NodeContent child in node.Children)
            ValidateMeshes(child, context);
    }

    static void FlattenTransforms(NodeContent node, BoneContent skeleton)
    {
        foreach (NodeContent child in node.Children)
        {
            if (child == skeleton) continue;
            MeshHelper.TransformScene(child, child.Transform);
            child.Transform = Matrix.Identity;
            FlattenTransforms(child, skeleton);
        }
    }
}
