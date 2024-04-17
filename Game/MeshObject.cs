using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TestMonoGame.Game;

public class MeshObject(string name) : GameObject(name), IDrawable
{
    public int DrawOrder { get; }
    public bool Visible { get; }
    public event EventHandler<EventArgs> DrawOrderChanged;
    public event EventHandler<EventArgs> VisibleChanged;
    
    // public Vector2 TextureTiling = new(1f, 1f);
    // public Color DiffuseColor = Color.White;
    // public bool ReceiveLighting = true;
    public Model Model
    {
        get => _model;
        set => SwapModel(value);
    }

    public Texture2D SharedTexture
    {
        get => _sharedTexture;
        set => SetSharedTexture(value);
    }

    public Effect SharedEffect
    {
        get => _sharedEffect;
        set => SetSharedEffect(value);
    }

    public Vector2 TextureTiling
    {
        get => _textureTiling;
        set => SetTextureTiling(value);
    }
    
    private Model _model;
    private ModelMeshCollection _meshes;
    private ModelBoneCollection _bones;

    private Texture2D _sharedTexture;
    private Effect _sharedEffect;

    private Vector2 _textureTiling;

    private static Matrix ViewMatrix => Camera.Current != null
        ? Camera.Current.ViewMatrix
        : Matrix.CreateLookAt(Vector3.Zero, Vector3.Forward, Vector3.Up);

    private static Matrix ProjectionMatrix => Camera.Current != null
        ? Camera.Current.ProjectionMatrix
        : Matrix.CreatePerspectiveFieldOfView(MathF.PI / 2,
            MainGame.GraphicsDeviceManager.GraphicsDevice.DisplayMode.AspectRatio, 0.01f, 1000f);

    public virtual void Draw(GameTime gameTime)
    {
        if (_meshes == null || _meshes.Count == 0)
            return;
        
        foreach (var mesh in _meshes)
        {
            foreach (var effect in mesh.Effects)
            {
                UpdateMatrix(effect);
            }
            mesh.Draw();
        }
    }

    private void UpdateMatrix(Effect effect)
    {
        if (effect is BasicEffect basicEffect)
        {
            basicEffect.World = Transform.WorldMatrix;
            basicEffect.View = ViewMatrix;
            basicEffect.Projection = ProjectionMatrix;
            basicEffect.Alpha = 1f;
        }
        else
        {
            effect.Parameters["WorldViewProjection"].SetValue(
                Matrix.Multiply(Matrix.Multiply(Transform.WorldMatrix, ViewMatrix), ProjectionMatrix));
        }
    }
    
    private void SwapModel(Model newModel)
    {
        _meshes = newModel.Meshes;
        _bones = newModel.Bones;
        _model = newModel;
    }
    
    private void SetSharedTexture(Texture2D texture)
    {
        foreach (var meshEffect in _meshes.SelectMany(mesh => mesh.Effects))
        {
            if (meshEffect is BasicEffect basicEffect)
            {
                basicEffect.Texture = texture;
                basicEffect.TextureEnabled = texture != null;
            }
            else
            {
                meshEffect.Parameters["Texture"].SetValue(texture);
            }
        }

        _sharedTexture = texture;
    }

    private void SetSharedEffect(Effect effect)
    {
        foreach (var meshPart in _meshes.SelectMany(mesh => mesh.MeshParts))
        {
            meshPart.Effect = effect;
        }

        _sharedEffect = effect;
    }

    private void SetTextureTiling(Vector2 tiling)
    {
        _sharedEffect.Parameters["Tiling"].SetValue(tiling);
        _textureTiling = tiling;
    }
}