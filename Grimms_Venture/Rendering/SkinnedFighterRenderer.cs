using System;
using GrimmsVenture.Animation;
using GrimmsVenture.Combat;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GrimmsVenture.Rendering;

/// <summary>
/// Desenha um Fighter com o modelo FBX animado (SkinnedEffect),
/// mapeando FighterState -> clip. Cai no boneco procedural se falhar.
/// A escala e calculada sozinha: o modelo e medido ao carregar e ajustado
/// para TargetHeight (mesma altura do boneco procedural antigo, ~2.3).
/// </summary>
public sealed class SkinnedFighterRenderer
{
    // ================== CALIBRACAO VISUAL ==================
    const float TargetHeight = 2.3f;   // altura do boneco antigo (pes em 0, topo da cabeca 2.28)
    const float HeightMultiplier = 1f; // ajuste fino: 1.1f = 10% maior
    const float Yaw = MathHelper.Pi;              // modelo de costas? troque para MathHelper.Pi

    readonly Model _model = null!;
    readonly SkinningData _skin = null!;
    readonly AnimationPlayer _player = null!;
    readonly GraphicsDevice _gd = null!;

    Texture2D _white = null!; // SkinnedEffect sempre amostra textura: sem ela o modelo fica preto
    float _fitScale = 1f;
    float _footOffset = 0f;
    Matrix _root = Matrix.Identity;

    public bool Available { get; private set; }

    public SkinnedFighterRenderer(ContentManager content, string asset)
    {
        try
        {
            _model = content.Load<Model>(asset);
            _skin = (_model.Tag as SkinningData)
                ?? throw new Exception("Model sem SkinningData no Tag (processor errado no .mgcb?)");
            _player = new AnimationPlayer(_skin);
            _gd = (content.ServiceProvider.GetService(typeof(IGraphicsDeviceService)) as IGraphicsDeviceService)
                    ?.GraphicsDevice
                ?? throw new Exception("IGraphicsDeviceService indisponivel no ContentManager");

            _white = new Texture2D(_gd, 1, 1);
            _white.SetData(new[] { Color.White });

            // Mede o modelo JA SKINADO (Idle, t=0, sem raiz): independe da escala do bind.
            Vector3 min, max;
            bool ok = Measure(_model, out min, out max);
            if (ok) Console.WriteLine($"[Grimm] bind  Y=[{min.Y:F2}..{max.Y:F2}] X=[{min.X:F2}..{max.X:F2}]");
            Console.WriteLine($"[Grimm] ossos={_skin.BindPose.Count} clips=[{string.Join(", ", _skin.AnimationClips.Keys)}]");
            AnimationClip? idle = _skin.GetClip("Idle");
            if (idle == null)
                foreach (var c in _skin.AnimationClips.Values) { idle = c; break; }
            if (idle != null)
            {
                _player.StartClip(idle, true);
                _player.Update(TimeSpan.Zero, Matrix.Identity);
                if (MeasureSkinned(_model, _player.GetSkinTransforms(), out Vector3 smin, out Vector3 smax)
                    && smax.Y - smin.Y > 1e-4f)
                {
                    Console.WriteLine($"[Grimm] skin  Y=[{smin.Y:F2}..{smax.Y:F2}] X=[{smin.X:F2}..{smax.X:F2}]");
                    min = smin; max = smax; ok = true;
                }
            }

            if (ok)
            {
                Vector3 size = max - min;
                float h = MathF.Max(size.Y, 0.0001f);
                _fitScale = TargetHeight * HeightMultiplier / h;
                _footOffset = -min.Y;
                Console.WriteLine($"[Grimm] extents X={size.X:F3} Y={size.Y:F3} Z={size.Z:F3}  minY={min.Y:F3} -> escala={_fitScale:F5}");
            }
            else
            {
                Console.WriteLine("[Grimm] nao consegui medir o modelo; escala 0.01 de fallback");
                _fitScale = 0.01f;
            }
            Available = true;
        }
        catch (Exception e)
        {
            Console.WriteLine("[Grimm] falha ao carregar modelo: " + e.Message);
            Available = false; // sem modelo: o jogo usa o boneco procedural
        }
    }

    // Caixa envolvente das posicoes de vertice (espaco do modelo, pose de bind).
    static bool Measure(Model model, out Vector3 min, out Vector3 max)
    {
        min = new Vector3(float.MaxValue);
        max = new Vector3(float.MinValue);
        bool any = false;

        foreach (ModelMesh mesh in model.Meshes)
        foreach (ModelMeshPart part in mesh.MeshParts)
        {
            VertexBuffer vb = part.VertexBuffer;
            VertexDeclaration decl = vb.VertexDeclaration;
            int stride = decl.VertexStride, off = -1;
            foreach (VertexElement e in decl.GetVertexElements())
                if (e.VertexElementUsage == VertexElementUsage.Position &&
                    e.VertexElementFormat == VertexElementFormat.Vector3)
                { off = e.Offset; break; }
            if (off < 0) continue;

            var data = new byte[vb.VertexCount * stride];
            vb.GetData(data);
            for (int i = part.VertexOffset; i < part.VertexOffset + part.NumVertices; i++)
            {
                int p = i * stride + off;
                var v = new Vector3(BitConverter.ToSingle(data, p),
                                    BitConverter.ToSingle(data, p + 4),
                                    BitConverter.ToSingle(data, p + 8));
                min = Vector3.Min(min, v);
                max = Vector3.Max(max, v);
                any = true;
            }
        }
        return any;
    }

    // Igual ao Measure, mas aplicando as matrizes de pele (CPU skinning) em cada vertice.
    static bool MeasureSkinned(Model model, Matrix[] skin, out Vector3 min, out Vector3 max)
    {
        min = new Vector3(float.MaxValue);
        max = new Vector3(float.MinValue);
        bool any = false;

        foreach (ModelMesh mesh in model.Meshes)
        foreach (ModelMeshPart part in mesh.MeshParts)
        {
            VertexBuffer vb = part.VertexBuffer;
            VertexDeclaration decl = vb.VertexDeclaration;
            int stride = decl.VertexStride, posOff = -1, idxOff = -1, wOff = -1;
            VertexElementFormat idxFmt = VertexElementFormat.Byte4, wFmt = VertexElementFormat.Vector4;
            foreach (VertexElement e in decl.GetVertexElements())
            {
                if (e.VertexElementUsage == VertexElementUsage.Position) posOff = e.Offset;
                else if (e.VertexElementUsage == VertexElementUsage.BlendIndices) { idxOff = e.Offset; idxFmt = e.VertexElementFormat; }
                else if (e.VertexElementUsage == VertexElementUsage.BlendWeight) { wOff = e.Offset; wFmt = e.VertexElementFormat; }
            }
            if (posOff < 0 || idxOff < 0 || wOff < 0) continue;

            var data = new byte[vb.VertexCount * stride];
            vb.GetData(data);
            for (int i = part.VertexOffset; i < part.VertexOffset + part.NumVertices; i++)
            {
                int b = i * stride;
                var pos = new Vector3(BitConverter.ToSingle(data, b + posOff),
                                      BitConverter.ToSingle(data, b + posOff + 4),
                                      BitConverter.ToSingle(data, b + posOff + 8));
                Vector3 acc = Vector3.Zero;
                float total = 0f;
                for (int k = 0; k < 4; k++)
                {
                    int idx = idxFmt == VertexElementFormat.Short4
                        ? BitConverter.ToUInt16(data, b + idxOff + k * 2)
                        : data[b + idxOff + k];
                    float w = wFmt == VertexElementFormat.Vector4
                        ? BitConverter.ToSingle(data, b + wOff + k * 4)
                        : wFmt == VertexElementFormat.NormalizedShort4
                            ? BitConverter.ToInt16(data, b + wOff + k * 2) / 32767f
                            : data[b + wOff + k] / 255f;
                    if (w <= 0f || idx >= skin.Length) continue;
                    acc += Vector3.Transform(pos, skin[idx]) * w;
                    total += w;
                }
                if (total <= 0f) continue;
                acc /= total;
                min = Vector3.Min(min, acc);
                max = Vector3.Max(max, acc);
                any = true;
            }
        }
        return any;
    }

    public void Update(GameTime gt, Fighter f)
    {
        if (!Available) return;
        ChooseClip(f, out string name, out bool loop);

        AnimationClip? clip = _skin.GetClip(name);
        if (clip != null) _player.StartClip(clip, loop);

        _root = RootMatrix(f);
        _player.Update(gt.ElapsedGameTime, _root);
    }

    void ChooseClip(Fighter f, out string name, out bool loop)
    {
        name = "Idle";
        loop = true;

        switch (f.State)
        {
            case FighterState.Attacking:
                name = f.CurrentAttack?.Name switch
                {
                    "Jab"      => "Punch1",
                    "Straight" => "Punch2",
                    "Cross"    => "Punch3",
                    "Kick"     => "Kick",
                    _          => "Punch1"
                };
                loop = false;
                break;

            case FighterState.Guarding: name = "Guard"; break;
            case FighterState.HitStun:  name = "HitStun"; loop = false; break;
            case FighterState.KO:       name = "KO"; loop = false; break;
            case FighterState.Dashing:  name = "Walk"; break;
            default:
                name = f.IsAirborne ? "Air" : f.State == FighterState.Moving ? "Walk" : "Idle";
                break;
        }
    }

    Matrix RootMatrix(Fighter f)
        => Matrix.CreateTranslation(0f, _footOffset, 0f)
         * Matrix.CreateScale(_fitScale)
         * Matrix.CreateRotationY(Yaw)
         * Matrix.CreateWorld(f.Position, f.Forward, Vector3.Up);

    public void Draw(Camera camera, float aspect)
    {
        if (!Available) return;
        Matrix[] bones = _player.GetSkinTransforms();

        // FBX do Blender tem winding CCW; o Renderer3D do jogo usa CW.
        _gd.RasterizerState = RasterizerState.CullCounterClockwise;

        foreach (ModelMesh mesh in _model.Meshes)
        {
            foreach (Effect effect in mesh.Effects)
            {
                if (effect is SkinnedEffect skinned)
                {
                    if (skinned.Texture == null) skinned.Texture = _white;
                    skinned.EnableDefaultLighting();
                    skinned.PreferPerPixelLighting = true;
                    skinned.SetBoneTransforms(bones);
                    skinned.World = Matrix.Identity;
                    skinned.View = camera.View;
                    skinned.Projection = camera.GetProjection(aspect);
                }
                else if (effect is BasicEffect basic)
                {
                    // mesh sem skin: segue o lutador (posicao/escala/rotacao), sem animar
                    basic.EnableDefaultLighting();
                    basic.World = _root;
                    basic.View = camera.View;
                    basic.Projection = camera.GetProjection(aspect);
                }
            }
            mesh.Draw();
        }

        // devolve o estado que o resto do jogo espera
        _gd.RasterizerState = RasterizerState.CullClockwise;
    }
}