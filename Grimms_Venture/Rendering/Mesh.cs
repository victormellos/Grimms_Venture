using Microsoft.Xna.Framework.Graphics;

namespace GrimmsVenture.Rendering;

public sealed class Mesh : IDisposable
{
    readonly VertexBuffer _vb;
    readonly IndexBuffer _ib;
    readonly int _triangles;

    public Mesh(GraphicsDevice gd, VertexPositionNormalTexture[] vertices, ushort[] indices)
    {
        _vb = new VertexBuffer(gd, VertexPositionNormalTexture.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
        _vb.SetData(vertices);
        _ib = new IndexBuffer(gd, IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);
        _ib.SetData(indices);
        _triangles = indices.Length / 3;
    }

    public void Draw(Effect effect)
    {
        GraphicsDevice gd = _vb.GraphicsDevice;
        gd.SetVertexBuffer(_vb);
        gd.Indices = _ib;
        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _triangles);
        }
    }

    public void Dispose()
    {
        _vb.Dispose();
        _ib.Dispose();
    }
}
