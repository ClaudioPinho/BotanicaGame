using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TestMonoGame.Game;

namespace TestMonoGame.Debug;

public static class DebugUtils
{
    private static Queue<WireCube> _wireCubesToDraw;
    private static Queue<WireSphere> _wireSpheresToDraw;

    private static BasicEffect _wireframeEffect;

    public static void Initialize()
    {
        _wireCubesToDraw = new Queue<WireCube>();
        _wireSpheresToDraw = new Queue<WireSphere>();
        _wireframeEffect = new BasicEffect(MainGame.GameInstance.GraphicsDevice)
        {
            VertexColorEnabled = true,
            LightingEnabled = false
        };
    }

    public static void Update(GameTime gameTime)
    {
    }

    public static void Draw(GameTime gameTime)
    {
        while (_wireCubesToDraw.Count > 0)
        {
            var wireCubeToDraw = _wireCubesToDraw.Dequeue();
            _wireframeEffect.World = wireCubeToDraw.Transform.WorldMatrix;
            _wireframeEffect.View = Camera.Current.ViewMatrix;
            _wireframeEffect.Projection = Camera.Current.ProjectionMatrix;
            _wireframeEffect.DiffuseColor = wireCubeToDraw.WireframeColor.ToVector3();

            _wireframeEffect.CurrentTechnique.Passes[0].Apply();
            MainGame.GameInstance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineList,
                wireCubeToDraw.VertexPositions, 0, wireCubeToDraw.VertexPositions.Length, wireCubeToDraw.Indices, 0,
                wireCubeToDraw.Indices.Length / 2);
        }

        while (_wireSpheresToDraw.Count > 0)
        {
            var wireSphereToDraw = _wireSpheresToDraw.Dequeue();

            _wireframeEffect.World = wireSphereToDraw.Transform.WorldMatrix;
            _wireframeEffect.View = Camera.Current.ViewMatrix;
            _wireframeEffect.Projection = Camera.Current.ProjectionMatrix;
            _wireframeEffect.DiffuseColor = wireSphereToDraw.WireframeColor.ToVector3();

            _wireframeEffect.CurrentTechnique.Passes[0].Apply();
            MainGame.GameInstance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineList,
                wireSphereToDraw.VertexPositions, 0, wireSphereToDraw.VertexPositions.Length, wireSphereToDraw.Indices,
                0, wireSphereToDraw.Indices.Length / 2);
        }
    }

    public static void DrawWireSphere(Vector3 position, Quaternion rotation, Vector3 scale, float radius, Color color)
    {
        _wireSpheresToDraw.Enqueue(new WireSphere(position, rotation, scale, radius, color));
    }

    public static void DrawWireCube(Transform transform, Color color, Vector3[] customCorners = null)
    {
        _wireCubesToDraw.Enqueue(new WireCube(transform.Position, transform.Rotation, transform.Scale, color,
            customCorners));
    }

    public static void DrawWireCube(Vector3 position, Quaternion rotation, Vector3 scale, Color color,
        Vector3[] customCorners = null)
    {
        _wireCubesToDraw.Enqueue(new WireCube(position, rotation, scale, color, customCorners));
    }

    public static void PrintError(string error, object reference = null)
    {
        Console.WriteLine($"ERROR | {error}");
    }

    public static void PrintMessage(string message, object reference = null)
    {
        Console.WriteLine($"INFO | {message}");
    }

    public static void PrintWarning(string warning, object reference = null)
    {
        Console.WriteLine($"WARNING | {warning}");
    }
    
    private class WireCube
    {
        public readonly Transform Transform;
        public Color WireframeColor;

        public readonly VertexPosition[] VertexPositions;

        public readonly short[] Indices =
        {
            0, 1, 1, 2, 2, 3, 3, 0, // Bottom face
            4, 5, 5, 6, 6, 7, 7, 4, // Top face
            0, 4, 1, 5, 2, 6, 3, 7 // Vertical edges
        };

        public WireCube(Vector3 position, Quaternion rotation, Vector3 scale, Color wireframeColor,
            IReadOnlyList<Vector3> customCorners = null)
        {
            Transform = new Transform
            {
                Position = position,
                Scale = scale,
                Rotation = rotation
            };
            WireframeColor = wireframeColor;
            if (customCorners == null)
            {
                VertexPositions = new[]
                {
                    new VertexPosition(new Vector3(-0.5f, -0.5f, -0.5f)),
                    new VertexPosition(new Vector3(0.5f, -0.5f, -0.5f)),
                    new VertexPosition(new Vector3(0.5f, -0.5f, 0.5f)),
                    new VertexPosition(new Vector3(-0.5f, -0.5f, 0.5f)),
                    new VertexPosition(new Vector3(-0.5f, 0.5f, -0.5f)),
                    new VertexPosition(new Vector3(0.5f, 0.5f, -0.5f)),
                    new VertexPosition(new Vector3(0.5f, 0.5f, 0.5f)),
                    new VertexPosition(new Vector3(-0.5f, 0.5f, 0.5f))
                };
            }
            else
            {
                VertexPositions = new VertexPosition[customCorners.Count];
                for (var i = 0; i < customCorners.Count; i++)
                {
                    VertexPositions[i] = new VertexPosition(customCorners[i]);
                }
            }
        }
    }

    private class WireSphere
    {
        public readonly Transform Transform;
        public Color WireframeColor;

        public float Radius;

        public readonly VertexPosition[] VertexPositions;
        public readonly short[] Indices;

        public WireSphere(Vector3 position, Quaternion rotation, Vector3 scale, float radius, Color wireframeColor)
        {
            Transform = new Transform
            {
                Position = position,
                Rotation = rotation,
                Scale = scale
            };
            Radius = radius;
            WireframeColor = wireframeColor;

            var latitudeBands = 16;
            var longitudeBands = 16;
            var vertices = new List<VertexPosition>();

            // Generate vertices along the surface of the sphere
            for (var lat = 0; lat <= latitudeBands; lat++)
            {
                var theta = lat * Math.PI / latitudeBands;
                var sinTheta = Math.Sin(theta);
                var cosTheta = Math.Cos(theta);

                for (var lon = 0; lon <= longitudeBands; lon++)
                {
                    var phi = lon * 2 * Math.PI / longitudeBands;
                    var sinPhi = Math.Sin(phi);
                    var cosPhi = Math.Cos(phi);

                    var x = (float)(cosPhi * sinTheta);
                    var y = (float)cosTheta;
                    var z = (float)(sinPhi * sinTheta);

                    var vertexPosition = position + new Vector3(x, y, z) * radius;
                    vertices.Add(new VertexPosition(vertexPosition));
                }
            }

            // Generate indices to connect the vertices
            var indices = new List<short>();
            for (var lat = 0; lat < latitudeBands; lat++)
            {
                for (var lon = 0; lon < longitudeBands; lon++)
                {
                    var first = (lat * (longitudeBands + 1)) + lon;
                    var second = first + longitudeBands + 1;
                    indices.Add((short)first);
                    indices.Add((short)second);

                    if (lon == longitudeBands - 1)
                    {
                        indices.Add((short)second);
                        indices.Add((short)(first + 1));
                    }
                }
            }

            VertexPositions = vertices.ToArray();
            Indices = indices.ToArray();
        }
    }
}