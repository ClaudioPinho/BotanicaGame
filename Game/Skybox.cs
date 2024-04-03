using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TestMonoGame.Game;

public class Skybox
{
    private Model _skyboxModel;

    private TextureCube _skyboxTexture;

    private Effect _skyboxEffect;

    private float _skyboxSize = 100f;

    public Skybox(string skyboxTextureName, ContentManager content)
    {
        _skyboxModel = content.Load<Model>("Models/cube-inv-normals");
        _skyboxTexture = content.Load<TextureCube>(skyboxTextureName);
        _skyboxEffect = content.Load<Effect>("Effects/Skybox");
    }

    public void Draw()
    {
        foreach (var mesh in _skyboxEffect.CurrentTechnique.Passes.SelectMany(_ => _skyboxModel.Meshes))
        {
            foreach (var meshPart in mesh.MeshParts)
            {
                meshPart.Effect = _skyboxEffect;
                meshPart.Effect.Parameters["World"].SetValue(Matrix.CreateScale(_skyboxSize) *
                                                             Matrix.CreateTranslation(Camera.Current.Transform.Position));
                meshPart.Effect.Parameters["View"].SetValue(Camera.Current.ViewMatrix);
                meshPart.Effect.Parameters["Projection"].SetValue(Camera.Current.ProjectionMatrix);
                meshPart.Effect.Parameters["SkyBoxTexture"].SetValue(_skyboxTexture);
                meshPart.Effect.Parameters["CameraPosition"].SetValue(Camera.Current.Transform.Position);
            }
                
            mesh.Draw();
        }
    }
}