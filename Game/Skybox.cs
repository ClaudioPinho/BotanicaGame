using System;
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

    private float _skyboxSize = 500f;

    private static Matrix ViewMatrix => Camera.Current != null
        ? Camera.Current.ViewMatrix
        : Matrix.CreateLookAt(Vector3.Zero, Vector3.Forward, Vector3.Up);

    private static Matrix ProjectionMatrix => Camera.Current != null
        ? Camera.Current.ProjectionMatrix
        : Matrix.CreatePerspectiveFieldOfView(MathF.PI / 2,
            MainGame.GraphicsDeviceManager.GraphicsDevice.DisplayMode.AspectRatio, 0.01f, 1000f);

    private static Vector3 ViewPosition = Camera.Current != null ? Camera.Current.Transform.Position : Vector3.Zero;

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
                meshPart.Effect.Parameters["World"]
                    .SetValue(Matrix.CreateScale(_skyboxSize) * Matrix.CreateTranslation(ViewPosition));
                meshPart.Effect.Parameters["View"].SetValue(ViewMatrix);
                meshPart.Effect.Parameters["Projection"].SetValue(ProjectionMatrix);
                meshPart.Effect.Parameters["SkyBoxTexture"].SetValue(_skyboxTexture);
                meshPart.Effect.Parameters["CameraPosition"].SetValue(ViewPosition);
            }

            mesh.Draw();
        }
    }
}