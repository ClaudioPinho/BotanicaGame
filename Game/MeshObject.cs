using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TestMonoGame.Game;

public class MeshObject : GameObject
{
    public Texture2D Texture;
    public Model Model;

    public void Draw(GameTime gameTime)
    {
        // todo: should we output an error or something here?
        if (Model == null)
        {
            return;
        }
        foreach (var mesh in Model.Meshes)
        {
            foreach (var basicEffect in mesh.Effects.Cast<BasicEffect>())
            {
                if (Texture != null)
                {
                    basicEffect.TextureEnabled = true;
                    basicEffect.Texture = Texture;
                }
                else
                {
                    basicEffect.TextureEnabled = false;
                }

                // effect.EnableDefaultLighting();
                // effect.DiffuseColor = Color.Gray.ToVector3();
                basicEffect.AmbientLightColor = Color.White.ToVector3();

                basicEffect.World = Matrix.CreateWorld(Transform.Position, Vector3.Forward, Vector3.Up) *
                                    Matrix.CreateFromQuaternion(Transform.Rotation) *
                                    Matrix.CreateScale(Transform.Scale);
                basicEffect.View = Camera.Current.ViewMatrix;
                basicEffect.Projection = Camera.Current.ProjectionMatrix;
            }

            mesh.Draw();
        }
    }
}