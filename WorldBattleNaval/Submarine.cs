using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WorldBattleNaval;

public class Submarine
{
    public int Size { get; } = 3; // ocupa 3 células

    private readonly Model _model;
    private readonly Vector3 _modelCenter;
    private readonly float _modelRadius;

    private float _waveTime;

    // Parâmetros da animação de ondas
    private const float BobAmplitude  = 0.04f;  // altura do sobe/desce (unidades de mundo)
    private const float BobSpeed      = 1.8f;   // velocidade do bobbing
    private const float RollAmplitude = 0.04f;  // radianos de rolamento lateral
    private const float RollSpeed     = 1.3f;   // velocidade do roll
    private const float PitchAmplitude = 0.02f; // radianos de arfagem (frente/trás)
    private const float PitchSpeed    = 2.1f;   // velocidade do pitch

    public bool IsHorizontal { get; private set; } = true;
    public bool IsPlaced { get; private set; }
    public int PlacedRow { get; private set; }
    public int PlacedCol { get; private set; }
    public bool PlacedHorizontal { get; private set; }

    public Submarine(Model model)
    {
        _model = model;

        var bounds = new BoundingSphere();
        foreach (var mesh in model.Meshes)
        {
            var meshBounds = mesh.BoundingSphere.Transform(mesh.ParentBone.Transform);
            bounds = BoundingSphere.CreateMerged(bounds, meshBounds);
        }

        _modelCenter = bounds.Center;
        _modelRadius = bounds.Radius;
    }

    public void Update(GameTime gameTime)
    {
        _waveTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
    }

    public void Rotate() => IsHorizontal = !IsHorizontal;

    public void Place(int row, int col)
    {
        IsPlaced = true;
        PlacedRow = row;
        PlacedCol = col;
        PlacedHorizontal = IsHorizontal;
    }

    public void Reset()
    {
        IsPlaced = false;
    }

    /// <summary>Desenha o submarino na posição de célula especificada.</summary>
    /// <param name="alpha">0 = invisível, 1 = opaco.</param>
    public void Draw(GraphicsDevice gd, int row, int col, Matrix view, Matrix projection, float alpha = 1f)
    {
        float scale = (Size * Board.CellSize) / (2f * _modelRadius);
        float half = Board.Size * Board.CellSize / 2f;

        float centerX, centerZ;
        if (IsHorizontal)
        {
            centerX = -half + col * Board.CellSize + (Size * Board.CellSize) / 2f;
            centerZ = -half + row * Board.CellSize + Board.CellSize / 2f;
        }
        else
        {
            centerX = -half + col * Board.CellSize + Board.CellSize / 2f;
            centerZ = -half + row * Board.CellSize + (Size * Board.CellSize) / 2f;
        }

        // O modelo tem o eixo longo em X; quando vertical (ao longo de Z) gira 90°
        float rotY = IsHorizontal ? 0f : MathHelper.ToRadians(90f);

        // Animação de ondas: bobbing + roll + pitch
        float bob   = MathF.Sin(_waveTime * BobSpeed)   * BobAmplitude   * scale;
        float roll  = MathF.Sin(_waveTime * RollSpeed)  * RollAmplitude;
        float pitch = MathF.Sin(_waveTime * PitchSpeed) * PitchAmplitude;

        var world = Matrix.CreateTranslation(-_modelCenter)
                  * Matrix.CreateScale(scale)
                  * Matrix.CreateRotationZ(roll)
                  * Matrix.CreateRotationX(pitch)
                  * Matrix.CreateRotationY(rotY)
                  * Matrix.CreateTranslation(centerX, _modelRadius * scale * 0.1f + bob, centerZ);

        gd.DepthStencilState = DepthStencilState.Default;
        gd.SamplerStates[0] = SamplerState.PointClamp;
        gd.BlendState = alpha < 1f ? BlendState.AlphaBlend : BlendState.Opaque;

        foreach (var mesh in _model.Meshes)
        {
            foreach (BasicEffect effect in mesh.Effects)
            {
                effect.World = mesh.ParentBone.Transform * world;
                effect.View = view;
                effect.Projection = projection;
                effect.Alpha = alpha;
            }
            mesh.Draw();
        }
    }
}
