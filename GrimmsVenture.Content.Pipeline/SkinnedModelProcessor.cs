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
    // Sem isso o ModelProcessor gera BasicEffect e o modelo nunca e skinado.
    public SkinnedModelProcessor()
    {
        DefaultEffect = MaterialProcessorDefaultEffect.SkinnedEffect;
    }

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
            inverseBindPose.Add(Matrix.Invert(bone.AbsoluteTransform)); // ABSOLUTA (cadeia de pais), nao a local
            BoneContent parent = bone.Parent as BoneContent;
            hierarchy.Add(parent != null ? bones.IndexOf(parent) : -1);
        }

        var clips = new Dictionary<string, AnimationClip>();
        // O importador guarda as animacoes no no do esqueleto (convencao XNA), nao na raiz:
        // varre a cena inteira.
        var found = new Dictionary<string, AnimationContent>();
        CollectAnimations(input, found);
        CollectAnimations(skeleton, found);
        context.Logger.LogImportantMessage("Animacoes encontradas: {0}", found.Count);

        foreach (var kv in found)
        {
            string rawName = kv.Key;
            string name = CleanName(rawName); // "Kakuteru_Rig|Idle" -> "Idle"
            if (!clips.ContainsKey(name))
            {
                clips[name] = ProcessAnimation(kv.Value, boneLookup);
                context.Logger.LogImportantMessage("Clip '{0}' (de '{1}'): {2} keyframes",
                    name, rawName, clips[name].Keyframes.Count);
            }
        }

        FixUnits(bindPose, clips, context.Logger);

        ModelContent model = base.Process(input, context);
        model.Tag = new SkinningData(clips, bindPose, inverseBindPose, hierarchy);
        return model;
    }

    // Nomes de clip que o jogo usa (SkinnedFighterRenderer.ChooseClip).
    static readonly HashSet<string> KnownClips = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { "Idle", "Walk", "Punch1", "Punch2", "Punch3", "Kick", "Guard", "HitStun", "KO", "Air" };

    // Aceita os dois formatos do Blender/Assimp:
    //   "Kakuteru_Rig|Idle"  (rig|acao)  -> "Idle"
    //   "Idle|Kakuteru_RigPose" (acao|pose) -> "Idle"
    // Para nomes de OSSO (sem '|') devolve o proprio nome.
    // O rig do Blender tem escala 100 (ossos em metros, meshes em cm). Se a bind pose
    // e os keyframes sairem em unidades diferentes, o boneco fica gigante/minusculo.
    // Compara o offset de cada osso na bind pose com o do 1o keyframe e corrige os
    // keyframes pela mediana da razao (so se for >10x ou <0.1x).
    static void FixUnits(List<Matrix> bindPose, Dictionary<string, AnimationClip> clips,
                         ContentBuildLogger log)
    {
        var ratios = new List<float>();
        for (int b = 0; b < bindPose.Count; b++)
        {
            float bl = bindPose[b].Translation.Length();
            if (bl < 1e-4f) continue;
            foreach (AnimationClip clip in clips.Values)
            {
                Keyframe first = clip.Keyframes.Find(k => k.Bone == b);
                if (first == null) continue;
                float kl = first.Transform.Translation.Length();
                if (kl > 1e-4f) ratios.Add(bl / kl);
                break;
            }
        }
        if (ratios.Count == 0) return;
        ratios.Sort();
        float ratio = ratios[ratios.Count / 2];
        log.LogImportantMessage("Razao de unidades bind/keyframes: {0}", ratio);
        if (ratio < 10f && ratio > 0.1f) return;

        foreach (AnimationClip clip in clips.Values)
            foreach (Keyframe kf in clip.Keyframes)
            {
                Matrix m = kf.Transform;
                m.Translation *= ratio;
                kf.Transform = m;
            }
        log.LogImportantMessage("Keyframes reescalados por {0}", ratio);
    }

    static void CollectAnimations(NodeContent node, Dictionary<string, AnimationContent> into)
    {
        foreach (var kv in node.Animations)
            if (!into.ContainsKey(kv.Key)) into[kv.Key] = kv.Value;
        foreach (NodeContent child in node.Children)
            CollectAnimations(child, into);
    }

    static string CleanName(string raw)
    {
        string[] parts = raw.Split('|');
        if (parts.Length == 1) return raw.Trim();
        foreach (string p in parts)
            if (KnownClips.Contains(p.Trim())) return p.Trim();
        // desconhecido: assume "rig|acao" (Blender atual) e fica com a ultima parte
        return parts[parts.Length - 1].Trim();
    }

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
                // SkinnedEffect exige UV (canal 0). Meshes de cor solida nao tem: cria UVs zeradas.
                if (!geo.Vertices.Channels.Contains(VertexChannelNames.TextureCoordinate(0)))
                    geo.Vertices.Channels.Add<Vector2>(
                        VertexChannelNames.TextureCoordinate(0),
                        new Vector2[geo.Vertices.VertexCount]);

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