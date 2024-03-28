using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TestMonoGame.Game;

namespace TestMonoGame.Rendering;

public class PrimitivePlane
{
    private VertexPositionNormalTexture[] _vertices = new VertexPositionNormalTexture[]
    {
        // First triangle
        new(new Vector3(-1, 0, -1), Vector3.Up, new Vector2(0, 1)), // Bottom-left
        new(new Vector3(1, 0, -1), Vector3.Up, new Vector2(1, 1)), // Bottom-right
        new(new Vector3(-1, 0, 1), Vector3.Up, new Vector2(0, 0)), // Top-left

        // Second triangle
        new(new Vector3(-1, 0, 1), Vector3.Up, new Vector2(0, 0)), // Top-left
        new(new Vector3(1, 0, -1), Vector3.Up, new Vector2(1, 1)), // Bottom-right
        new(new Vector3(1, 0, 1), Vector3.Up, new Vector2(1, 0)) // Top-right
    };

    private short[] _indices =
    {
        0, 1, 2, // First triangle
        3, 4, 5 // Second triangle
    };

    private VertexBuffer _vertexBuffer;
    private IndexBuffer _indexBuffer;

    private BasicEffect _basicEffect;
    private GraphicsDevice _graphicsDevice;

    public PrimitivePlane(GraphicsDevice graphicsDevice)
    {
        _vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionNormalTexture), _vertices.Length,
            BufferUsage.WriteOnly);
        _vertexBuffer.SetData(_vertices);

        _indexBuffer = new IndexBuffer(graphicsDevice, typeof(short), _indices.Length, BufferUsage.WriteOnly);
        _indexBuffer.SetData(_indices);

        _basicEffect = new BasicEffect(graphicsDevice);

        _graphicsDevice = graphicsDevice;
    }

    public void Draw(Camera camera, Texture2D texture)
    {
        _graphicsDevice.SetVertexBuffer(_vertexBuffer);
        _graphicsDevice.Indices = _indexBuffer;

        if (texture != null)
        {
            _basicEffect.TextureEnabled = true;
            _basicEffect.Texture = texture;
        }
        
        _basicEffect.World = Matrix.Identity + Matrix.CreateScale(100f, 0f, 100f);
        _basicEffect.Projection = camera.ProjectionMatrix;
        _basicEffect.View = camera.ViewMatrix;
        
        foreach (var pass in _basicEffect.CurrentTechnique.Passes)
        {
            pass.Apply();
            _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _vertices.Length);
        }
    }
}