using GrimmsVenture.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GrimmsVenture.Rendering;

/// <summary>Wrapper fino sobre BasicEffect + meshes primitivas compartilhadas.</summary>
public sealed class Renderer3D : IDisposable
{
    readonly GraphicsDevice _gd;
    readonly BasicEffect _fx;

    public Mesh Box { get; }
    public Mesh Sphere { get; }
    public Mesh SphereLow { get; }
    public Mesh Cylinder { get; }
    public Mesh Quad { get; }

    public Renderer3D(GraphicsDevice gd)
    {
        _gd = gd;
        _fx = new BasicEffect(gd)
        {
            VertexColorEnabled = false,
            PreferPerPixelLighting = true,
            FogEnabled = true,
            FogColor = GameConstants.SkyColor.ToVector3(),
            FogStart = 70f,
            FogEnd = 260f
        };
        _fx.EnableDefaultLighting();
        _fx.AmbientLightColor = new Vector3(0.35f, 0.35f, 0.42f);

        Box = MeshFactory.Box(gd);
        Sphere = MeshFactory.Sphere(gd, 12, 16);
        SphereLow = MeshFactory.Sphere(gd, 5, 8);
        Cylinder = MeshFactory.Cylinder(gd, 48, 3.5f);
        Quad = MeshFactory.Quad(gd, 1f);
    }

    public void BeginScene(Camera camera, float aspect)
    {
        _fx.View = camera.View;
        _fx.Projection = camera.GetProjection(aspect);
        SetOpaqueState();
    }

    public void BeginTransparent()
    {
        _gd.BlendState = BlendState.AlphaBlend;
        _gd.DepthStencilState = DepthStencilState.DepthRead;
    }

    public void EndTransparent() => SetOpaqueState();

    void SetOpaqueState()
    {
        _gd.BlendState = BlendState.Opaque;
        _gd.DepthStencilState = DepthStencilState.Default;
        _gd.RasterizerState = RasterizerState.CullClockwise; 
        _gd.SamplerStates[0] = SamplerState.LinearWrap;
    }

    public void Draw(Mesh mesh, Matrix world, Color color, float alpha = 1f, bool unlit = false, Texture2D? texture = null)
    {
        _fx.World = world;
        _fx.DiffuseColor = color.ToVector3();
        _fx.Alpha = alpha;
        _fx.LightingEnabled = !unlit;
        _fx.TextureEnabled = texture != null;
        if (texture != null) _fx.Texture = texture;
        mesh.Draw(_fx);
    }

    public void Dispose()
    {
        Box.Dispose();
        Sphere.Dispose();
        SphereLow.Dispose();
        Cylinder.Dispose();
        Quad.Dispose();
        _fx.Dispose();
    }
}
