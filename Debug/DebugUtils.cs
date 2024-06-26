using System;
using System.Collections.Generic;
using BotanicaGame.Game;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BotanicaGame.Debug;

public static class DebugUtils
{
    private static Queue<WireObject> _wireObjectsToDraw;
    private static Queue<DebugTriangle> _debugTrianglesToDraw;

    private static BasicEffect _wireframeEffect;

    private static readonly Color DefaultDebugDrawColor = Color.Yellow;

    public static void Initialize(GraphicsDevice graphicsDevice)
    {
        _wireObjectsToDraw = new Queue<WireObject>();
        _debugTrianglesToDraw = new Queue<DebugTriangle>();
        _wireframeEffect = new BasicEffect(graphicsDevice)
        {
            VertexColorEnabled = true,
            LightingEnabled = false
        };
    }

    public static void Update(float deltaTime)
    {
    }

    public static void Draw(GraphicsDevice graphicsDevice, float deltaTime)
    {
        while (_wireObjectsToDraw.Count > 0)
        {
            var wireObjectToDraw = _wireObjectsToDraw.Dequeue();
            _wireframeEffect.World = wireObjectToDraw.UsesWorldPositions
                ? Matrix.Identity
                : wireObjectToDraw.Transform.WorldMatrix;

            _wireframeEffect.View = Camera.Current != null
                ? Camera.Current.ViewMatrix
                : Matrix.CreateLookAt(Vector3.Zero, Vector3.Forward, Vector3.Up);

            _wireframeEffect.Projection = Camera.Current != null
                ? Camera.Current.ProjectionMatrix
                : Matrix.CreatePerspectiveFieldOfView(MathF.PI / 2,
                    MainGame.GraphicsDeviceManager.GraphicsDevice.DisplayMode.AspectRatio, 0.01f, 1000f);

            _wireframeEffect.CurrentTechnique.Passes[0].Apply();
            graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineList, wireObjectToDraw.VertexPositions, 0,
                wireObjectToDraw.VertexPositions.Length, wireObjectToDraw.Indices, 0,
                wireObjectToDraw.Indices.Length / 2);
        }

        while (_debugTrianglesToDraw.Count > 0)
        {
            var triangleObjectToDraw = _debugTrianglesToDraw.Dequeue();
            _wireframeEffect.World = Matrix.Identity;
            _wireframeEffect.View = Camera.Current.ViewMatrix;
            _wireframeEffect.Projection = Camera.Current.ProjectionMatrix;

            _wireframeEffect.CurrentTechnique.Passes[0].Apply();
            graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineList, triangleObjectToDraw.VertexPositions, 0,
                triangleObjectToDraw.VertexPositions.Length, triangleObjectToDraw.Indices, 0,
                triangleObjectToDraw.Indices.Length / 2);
        }
    }

    public static void DrawTriangle(Vector3 pA, Vector3 pB, Vector3 pC, Color? color = null)
    {
        _debugTrianglesToDraw.Enqueue(new DebugTriangle(pA, pB, pC, color ?? DefaultDebugDrawColor));
    }

    public static void DrawWirePoint(Vector3 position, Quaternion? rotation = null, Vector3? scale = null,
        Color? color = null)
    {
        _wireObjectsToDraw.Enqueue(new WirePoint(position, rotation ?? Quaternion.Identity, scale ?? Vector3.One,
            color ?? DefaultDebugDrawColor));
    }

    public static void DrawWireSphere(Vector3 position, Quaternion? rotation = null, Vector3? scale = null,
        float? radius = null, Color? color = null)
    {
        _wireObjectsToDraw.Enqueue(new WireSphere(position, rotation ?? Quaternion.Identity, scale ?? Vector3.One,
            radius ?? 1f, color ?? DefaultDebugDrawColor));
    }

    public static void DrawWireCube(Vector3 position, Quaternion? rotation = null, Vector3? scale = null,
        Color? color = null, Vector3[] customCorners = null)
    {
        _wireObjectsToDraw.Enqueue(new WireCube(position, rotation ?? Quaternion.Identity, scale ?? Vector3.One,
            color ?? DefaultDebugDrawColor, customCorners));
    }

    public static void DrawDebugAxis(Vector3 position, Quaternion? rotation = null, Vector3? scale = null)
    {
        _wireObjectsToDraw.Enqueue(new WireAxis(position, rotation ?? Quaternion.Identity, scale ?? Vector3.One));
    }

    public static void PrintError(string error, object reference = null)
    {
        Console.WriteLine($"ERROR ({DateTime.Now:T}) | {error}");
    }

    public static void PrintException(Exception exception, object reference = null)
    {
        Console.WriteLine($"Exception ({DateTime.Now:T}) | {exception.Message}/n{exception.StackTrace}");
    }

    public static void PrintMessage(string message, object reference = null)
    {
        Console.WriteLine($"INFO ({DateTime.Now:T}) | {message}");
    }

    public static void PrintWarning(string warning, object reference = null)
    {
        Console.WriteLine($"WARNING ({DateTime.Now:T}) | {warning}");
    }

    private class WireObject
    {
        public Transform Transform;
        public Color WireframeColor;

        public bool UsesWorldPositions = false;

        public VertexPositionColor[] VertexPositions;
        public short[] Indices;
    }

    private class DebugTriangle
    {
        public VertexPositionColor[] VertexPositions;
        public short[] Indices;

        public DebugTriangle(Vector3 pA, Vector3 pB, Vector3 pC, Color color)
        {
            VertexPositions = new[]
            {
                new VertexPositionColor(pA, color),
                new VertexPositionColor(pB, color),
                new VertexPositionColor(pC, color)
            };
            Indices = new short[]
            {
                0, 1, 2, 0
            };
        }
    }

    private class WireAxis : WireObject
    {
        public WireAxis(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            Transform = new Transform
            {
                Position = position,
                Rotation = rotation,
                Scale = scale
            };

            VertexPositions = new[]
            {
                // Define vertices for lines from the origin to the local origin in each cardinal direction
                new VertexPositionColor(Vector3.Zero, Color.Red),
                new VertexPositionColor(Vector3.Zero, Color.Green),
                new VertexPositionColor(Vector3.Zero, Color.Blue),
                new VertexPositionColor(Vector3.UnitX, Color.Red),
                new VertexPositionColor(Vector3.UnitY, Color.Green),
                new VertexPositionColor(Vector3.UnitZ, Color.Blue),
            };
            Indices = new short[]
            {
                0, 3,
                1, 4,
                2, 5,
            };
        }
    }

    private class WirePoint : WireObject
    {
        public WirePoint(Vector3 position, Quaternion rotation, Vector3 scale, Color wireFrameColor)
        {
            WireframeColor = wireFrameColor;

            Transform = new Transform
            {
                Position = position,
                Rotation = rotation,
                Scale = scale
            };

            VertexPositions = new[]
            {
                // Define vertices for lines from the origin to the local origin in each cardinal direction
                new VertexPositionColor(Vector3.Zero, WireframeColor), // Origin
                new VertexPositionColor(Vector3.Left, WireframeColor), // Line towards left
                new VertexPositionColor(Vector3.Right, WireframeColor), // Line towards right
                new VertexPositionColor(Vector3.Up, WireframeColor), // Line towards up
                new VertexPositionColor(Vector3.Down, WireframeColor), // Line towards down
                new VertexPositionColor(Vector3.Forward, WireframeColor), // Line towards forward
                new VertexPositionColor(Vector3.Backward, WireframeColor) // Line towards backward
            };
            Indices = new short[]
            {
                0, 1, // Connect origin to left
                0, 2, // Connect origin to right
                0, 3, // Connect origin to up
                0, 4, // Connect origin to down
                0, 5, // Connect origin to forward
                0, 6 // Connect origin to backward
            };
        }
    }

    private class WireCube : WireObject
    {
        public WireCube(Vector3 position, Quaternion rotation, Vector3 scale, Color wireframeColor,
            IReadOnlyList<Vector3> customCorners = null)
        {
            Indices = new short[]
            {
                0, 1, 1, 2, 2, 3, 3, 0, // Bottom face
                4, 5, 5, 6, 6, 7, 7, 4, // Top face
                0, 4, 1, 5, 2, 6, 3, 7 // Vertical edges
            };
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
                    new VertexPositionColor(new Vector3(-0.5f, -0.5f, -0.5f), WireframeColor),
                    new VertexPositionColor(new Vector3(0.5f, -0.5f, -0.5f), WireframeColor),
                    new VertexPositionColor(new Vector3(0.5f, -0.5f, 0.5f), WireframeColor),
                    new VertexPositionColor(new Vector3(-0.5f, -0.5f, 0.5f), WireframeColor),
                    new VertexPositionColor(new Vector3(-0.5f, 0.5f, -0.5f), WireframeColor),
                    new VertexPositionColor(new Vector3(0.5f, 0.5f, -0.5f), WireframeColor),
                    new VertexPositionColor(new Vector3(0.5f, 0.5f, 0.5f), WireframeColor),
                    new VertexPositionColor(new Vector3(-0.5f, 0.5f, 0.5f), WireframeColor)
                };
            }
            else
            {
                UsesWorldPositions = true;
                VertexPositions = new VertexPositionColor[customCorners.Count];
                for (var i = 0; i < customCorners.Count; i++)
                {
                    VertexPositions[i] = new VertexPositionColor(customCorners[i], WireframeColor);
                }
            }
        }
    }

    private class WireSphere : WireObject
    {
        public float Radius;

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
            var vertices = new List<VertexPositionColor>();

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
                    vertices.Add(new VertexPositionColor(vertexPosition, WireframeColor));
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