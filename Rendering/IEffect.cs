using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BotanicaGame.Rendering;

public interface IEffect
{
    void SetTexture2D(Texture2D texture);
    void SetWorldViewProj(Matrix world, Matrix view, Matrix projection);
    void SetDiffuseColor(Color color);
    void SetTiling(float tilingX, float tilingY);
    void Draw(ModelMesh mesh);
}