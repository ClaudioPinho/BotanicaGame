using Jitter.Collision.Shapes;
using Jitter.Dynamics;
using Jitter.LinearMath;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TestMonoGame.Debug;
using TestMonoGame.Extensions;
using TestMonoGame.Physics;

namespace TestMonoGame.Game.World;

public class Block
{
    public Vector3 Position;

    public Color Color;

    private static VertexPositionColor[] _vertexPosition;
    private static short[] _indices;
    private static BasicEffect _basicEffect;

    private RigidBody _createdBody;
    
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
        _createdBody = new RigidBody(new BoxShape(JVector.One * blockSize), new Material()
        {
            KineticFriction = -100f,
            Restitution = -100f,
            StaticFriction = -100f
        }, true)
        {
            Position = Position.ToJVector(),
            IsStatic = true,
            
            EnableSpeculativeContacts = false,
            EnableDebugDraw = true
        };
        GamePhysics.World.AddBody(_createdBody);
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