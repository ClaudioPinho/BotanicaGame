using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TestMonoGame.Rendering;

namespace TestMonoGame.Game;

public class MeshObject : GameObject
{
    public IEffect MeshEffect;

    public Texture2D Texture;

    public Vector2 TextureTiling = new(1f, 1f);

    public Color DiffuseColor = Color.White;

    public bool ReceiveLighting = true;

    public Model Model;

    public virtual void Draw(GraphicsDevice graphicsDevice, GameTime gameTime)
    {
        // todo: should we output an error or something here?
        if (Model == null)
            return;

        // creates the default mesh effect if none defined
        MeshEffect ??= new BasicEffectAdapter(new BasicEffect(graphicsDevice));

        foreach (var mesh in Model.Meshes)
        {
            MeshEffect.SetWorldViewProj(Transform.WorldMatrix, Camera.Current.ViewMatrix,
                Camera.Current.ProjectionMatrix);
            MeshEffect.SetTexture2D(Texture);
            MeshEffect.SetTiling(TextureTiling.X, TextureTiling.Y);
            MeshEffect.SetDiffuseColor(DiffuseColor);
            MeshEffect.Draw(mesh);
        }
    }
}