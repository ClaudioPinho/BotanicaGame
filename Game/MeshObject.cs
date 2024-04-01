using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TestMonoGame.Rendering;

namespace TestMonoGame.Game;

public class MeshObject : GameObject
{
    public IEffect MeshEffect;

    public Texture2D Texture;

    public Color DiffuseColor = Color.White;

    public bool ReceiveLighting = true;

    public Model Model;

    public void Draw(GameTime gameTime)
    {
        // todo: should we output an error or something here?
        if (Model == null)
            return;

        // crreates the default mesh effect if none defined
        MeshEffect ??= new BasicEffectAdapter(new BasicEffect(MainGame.GameInstance.GraphicsDevice));

        foreach (var mesh in Model.Meshes)
        {
            MeshEffect.SetWorldViewProj(Transform.WorldMatrix, Camera.Current.ViewMatrix,
                Camera.Current.ProjectionMatrix);
            MeshEffect.SetTexture2D(Texture);
            MeshEffect.SetDiffuseColor(DiffuseColor);
            MeshEffect.Draw(mesh);
        }
    }
}