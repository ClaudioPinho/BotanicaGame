using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BotanicaGame.Rendering;

public class GenericEffectAdapter: IEffect
{
    private readonly Effect _effect;
    
    public GenericEffectAdapter(Effect effect)
    {
        _effect = effect;
    }
    
    public void SetTexture2D(Texture2D texture)
    {
        _effect.Parameters["Texture"].SetValue(texture);
        
    }

    public void SetTiling(float tilingX, float tilingY)
    {
        _effect.Parameters["TillingX"].SetValue(tilingX);
        _effect.Parameters["TillingY"].SetValue(tilingY);
    }

    public void SetWorldViewProj(Matrix world, Matrix view, Matrix projection)
    {
        var worldViewProj = Matrix.Multiply(world, view);
        worldViewProj = Matrix.Multiply(worldViewProj, projection);
        _effect.Parameters["WorldViewProjection"].SetValue(worldViewProj);
    }

    public void SetDiffuseColor(Color color)
    {
        _effect.Parameters["DiffuseColor"].SetValue(color.ToVector4());
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