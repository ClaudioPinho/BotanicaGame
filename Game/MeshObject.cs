using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BotanicaGame.Game;

public class MeshObject(string name) : GameObject(name), IDrawable
{
    public int DrawOrder { get; }

    public bool Visible
    {
        get => _isVisible && IsActive;
        set => _isVisible = value;
    }
    
    public event EventHandler<EventArgs> DrawOrderChanged;
    public event EventHandler<EventArgs> VisibleChanged;
    
    public Model Model
    {
        get => _model;
        set => SwapModel(value);
    }

    public Effect SharedEffect;
    public Texture2D SharedTexture;
    public Vector2 TextureTiling = Vector2.One;
    public Color DiffuseColor = Color.White;
    
    private Model _model;
    private ModelMeshCollection _meshes;
    private ModelBoneCollection _bones;

    private bool _isVisible = true;

    private static Matrix ViewMatrix => Camera.Current != null
        ? Camera.Current.ViewMatrix
        : Matrix.CreateLookAt(Vector3.Zero, Vector3.Forward, Vector3.Up);

    private static Matrix ProjectionMatrix => Camera.Current != null
        ? Camera.Current.ProjectionMatrix
        : Matrix.CreatePerspectiveFieldOfView(MathF.PI / 2,
            MainGame.GraphicsDeviceManager.GraphicsDevice.DisplayMode.AspectRatio, 0.01f, 1000f);

    public virtual void Draw(GameTime gameTime)
    {
        if (!Visible)
            return;
        
        if (_meshes == null || _meshes.Count == 0)
            return;

        foreach (var mesh in _model.Meshes)
        {
            foreach (var meshPart in mesh.MeshParts)
            {
                if (SharedEffect != null)
                    meshPart.Effect = SharedEffect;
            }
            
            foreach (var effect in mesh.Effects)
            {
                if (effect is BasicEffect basicEffect)
                {
                    basicEffect.World = Transform.WorldMatrix;
                    basicEffect.View = ViewMatrix;
                    basicEffect.Projection = ProjectionMatrix;
                    basicEffect.Alpha = 1f;
                    basicEffect.DiffuseColor = DiffuseColor.ToVector3();
                    if (SharedTexture != null)
                    {
                        basicEffect.Texture = SharedTexture;
                        basicEffect.TextureEnabled = SharedTexture != null;
                    }
                }
                else
                {
                    effect.Parameters["WorldViewProjection"].SetValue(
                        Matrix.Multiply(Matrix.Multiply(Transform.WorldMatrix, ViewMatrix), ProjectionMatrix));
                    effect.Parameters["Texture"].SetValue(SharedTexture);
                    effect.Parameters["DiffuseColor"].SetValue(DiffuseColor.ToVector4());
                    effect.Parameters["Tiling"].SetValue(TextureTiling);
                }
            }
            mesh.Draw();
        }
    }
    
    private void SwapModel(Model newModel)
    {
        _meshes = newModel.Meshes;
        _bones = newModel.Bones;
        _model = newModel;
    }

}