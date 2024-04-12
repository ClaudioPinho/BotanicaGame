using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TestMonoGame.Rendering;

namespace TestMonoGame.Game;

public class MeshObject(
    string name,
    Vector3? position = null,
    Quaternion? rotation = null,
    Vector3? scale = null,
    Transform parent = null)
    : GameObject(name, position, rotation, scale, parent)
{
    public IEffect MeshEffect;
    public Model Model;
    public Texture2D Texture;
    public Vector2 TextureTiling = new(1f, 1f);
    public Color DiffuseColor = Color.White;
    public bool ReceiveLighting = true;

    public virtual void Draw(GraphicsDevice graphicsDevice, GameTime gameTime)
    {
        // todo: should we output an error or something here?
        if (Model == null)
            return;

        foreach (var mesh in Model.Meshes)
        {
            foreach (var effect in mesh.Effects)
            {
                if (effect is BasicEffect basicEffect)
                {
                    basicEffect.World = Transform.WorldMatrix;
                    basicEffect.View = Camera.Current.ViewMatrix;
                    basicEffect.Projection = Camera.Current.ProjectionMatrix;
                    basicEffect.Alpha = 1f;

                }
                else
                {
                    effect.Parameters["WorldViewProjection"].SetValue(Matrix.Multiply(
                        Matrix.Multiply(Transform.WorldMatrix, Camera.Current.ViewMatrix),
                        Camera.Current.ProjectionMatrix));
                }

            }
            mesh.Draw();
        }
    }
}