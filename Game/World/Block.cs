using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TestMonoGame.Game.World;

public class Block
{
    public Vector3 Position;

    public Color Color;

    private static VertexPositionColor[] _vertexPosition;
    private static short[] _indices;
    private static BasicEffect _basicEffect;

    public Block(Vector3 position, Color color, float blockSize = 1)
    {
        Position = position;
        Color = color;
        _vertexPosition ??= new[]
        {
            new VertexPositionColor(new Vector3(-1, -1, -1), Color.White),
            new VertexPositionColor(new Vector3(-1, -1, 1), Color.White),
            new VertexPositionColor(new Vector3(1, -1, 1), Color.White),
            new VertexPositionColor(new Vector3(1, -1, -1), Color.White),
            new VertexPositionColor(new Vector3(-1, 1, -1), Color.White),
            new VertexPositionColor(new Vector3(-1, 1, 1), Color.White),
            new VertexPositionColor(new Vector3(1, 1, 1), Color.White),
            new VertexPositionColor(new Vector3(1, 1, -1), Color.White),
        };
        _indices ??= new short[]
        {
            // Bottom face
            0, 1, 2,
            0, 2, 3,

            // Top face
            4, 6, 5,
            4, 7, 6,

            // Front face
            0, 4, 5,
            0, 5, 1,

            // Back face
            3, 2, 6,
            3, 6, 7,

            // Left face
            0, 3, 7,
            0, 7, 4,

            // Right face
            1, 5, 6,
            1, 6, 2
        };
        _basicEffect ??= new BasicEffect(MainGame.GameInstance.GraphicsDevice);
    }

    public void Draw(GraphicsDevice graphicsDevice)
    {
        // _createdBody.DebugDraw(DebugUtils.CollisionDrawer);
        
        _basicEffect.World = Matrix.CreateTranslation(Position);
        _basicEffect.View = Camera.Current.ViewMatrix;
        _basicEffect.Projection = Camera.Current.ProjectionMatrix;
        _basicEffect.DiffuseColor = Color.ToVector3();
        
        _basicEffect.CurrentTechnique.Passes[0].Apply();

        graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, _vertexPosition, 0,
            _vertexPosition.Length, _indices, 0, _indices.Length / 3);
    }
}