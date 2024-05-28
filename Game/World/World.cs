using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WorldEngine;
using WorldEngine.Layers;
using WorldEngine.Noise;
using WorldEngine.Rules;
using WorldEngine.World;

namespace BotanicaGame.Game.World;

public class World
{
    public Block[,] WorldBlocks;

    private Ruleset _ruleset;
    private Generator _generator;

    private int _width;
    private int _length;

    private const int BlockSize = 2;

    private WorldEngine.World.World _generatedWorld;

    const float frequencyMultiplier = 0.6f;

    public World(int seed, int width, int length)
    {
        _width = width;
        _length = length;

        Generate(seed);
    }

    public void Update(GameTime gameTime)
    {
    }

    public void Draw(GraphicsDevice graphicsDevice, GameTime gameTime)
    {
        foreach (var block in WorldBlocks)
        {
            block.Draw(graphicsDevice);
        }
    }

    private void Generate(int seed)
    {
        WorldBlocks = new Block[_width, _length];

        var altitudeNoise = new FractalPerlinNoise(0.005f * frequencyMultiplier, 6, 0.4f, 2);
        var precipitationNoise = new FractalPerlinNoise(0.005f * frequencyMultiplier, 6, 0.4f, 2);
        var temperatureNoise = new FractalPerlinNoise(0.005f * frequencyMultiplier, 6, 0.4f, 2);

        _ruleset = new Ruleset("", seed, 1200);
        _generator = new Generator(_ruleset,
            new Layer("", LayerTypes.Altitude, altitudeNoise, 0, 2500),
            new Layer("", LayerTypes.Precipitation, precipitationNoise, 0, 350),
            new Layer("", LayerTypes.Temperature, temperatureNoise, -10, 50));

        _generatedWorld = _generator.CreateWorld();

        for (var y = 0; y < _length; y++)
        {
            for (var x = 0; x < _length; x++)
            {
                var worldTile = _generatedWorld.GetTileAt(x, y);
                WorldBlocks[x, y] = new Block(new Vector3(x * BlockSize, 0f, y * BlockSize),
                    GetColorFromBiome(worldTile.Biome), BlockSize);
            }
        }
    }

    private Color GetColorFromBiome(BiomeTypes biomeTypes)
    {
        switch (biomeTypes)
        {
            case BiomeTypes.DeepOcean:
                return Color.DarkBlue;
            case BiomeTypes.ShallowOcean:
                return Color.Blue;
            case BiomeTypes.FrozenOcean:
                return Color.LightSteelBlue;
            case BiomeTypes.Forest:
                return Color.ForestGreen;
            case BiomeTypes.TropicalForest:
                return Color.PaleGreen;
            case BiomeTypes.Savanna:
                return Color.GreenYellow;
            case BiomeTypes.Desert:
                return Color.Yellow;
            case BiomeTypes.Grassland:
                return Color.Green;
            case BiomeTypes.Tundra:
                return Color.DarkGreen;
            case BiomeTypes.ArcticForest:
                return Color.LightGreen;
            case BiomeTypes.MountainBase:
                return Color.Gray;
            case BiomeTypes.Mountain:
                return Color.Gray;
            case BiomeTypes.MountainTop:
                return Color.WhiteSmoke;
            case BiomeTypes.Summit:
                return Color.White;
            case BiomeTypes.Beach:
                return Color.LightYellow;
            case BiomeTypes.Null:
            default:
                return Color.DeepPink;
        }
    }
}