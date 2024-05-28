using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BotanicaGame.Rendering;

public class BasicEffectAdapter : IEffect
{
    private readonly BasicEffect _effect;
    
    public BasicEffectAdapter(BasicEffect basicEffect)
    {
        _effect = basicEffect;
    }
    
    public void SetTexture2D(Texture2D texture)
    {
        _effect.TextureEnabled = true;
        _effect.Texture = texture;
    }

    public void SetWorldViewProj(Matrix world, Matrix view, Matrix projection)
    {
        _effect.World = world;
        _effect.View = view;
        _effect.Projection = projection;
    }

    public void SetDiffuseColor(Color color)
    {
        _effect.DiffuseColor = color.ToVector3();
    }

    public void SetTiling(float tilingX, float tilingY)
    {
        // no texture tiling for this effect
    }

    public void Draw(ModelMesh mesh)
    {
        foreach (var meshPart in mesh.MeshParts)
        {
            meshPart.Effect = _effect;
            foreach (var pass in meshPart.Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
            }
            mesh.Draw();
        }
    }
}