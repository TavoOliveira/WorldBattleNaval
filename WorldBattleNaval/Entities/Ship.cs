using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WorldBattleNaval.Entities;

public class CearenceCabecudo
{
    public int Size { get; private set; }
    public int Life { get; private set; }

    public bool IsSunk => Life <= 0;

    private readonly Model model;
    private readonly Vector3 modelCenter;
    private readonly float modelRadius;
    private readonly float baseRotation;
    
    public string Name { get; private set; }
    public bool IsHorizontal { get; private set; } = true;
    public bool IsPlaced { get; private set; }
    public int PlacedRow { get; private set; }
    public int PlacedCol { get; private set; }
    public bool PlacedHorizontal { get; private set; }

    public Ship(string name, Model model, int size, float baseRotation = 0f)
    {
        this.model = model;
        this.baseRotation = baseRotation;
        Size = size;
        Life = size;
        Name = name;

        var bounds = new BoundingSphere();

        foreach (var mesh in model.Meshes)
        {
            var meshBounds = mesh.BoundingSphere.Transform(mesh.ParentBone.Transform);
            bounds = BoundingSphere.CreateMerged(bounds, meshBounds);
        }

        modelCenter = bounds.Center;
        modelRadius = bounds.Radius;
    }

    public void Draw(GraphicsDevice graphicsDevice, int row, int col, Matrix view, Matrix projection, float alpha = 1f)
    {
        var scale = (Size * Board.CellSize) / (2f * modelRadius);
        var half = Board.Size * Board.CellSize / 2f;

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

        var rotateY = (IsHorizontal ? MathHelper.ToRadians(90f) : 0f) + baseRotation;

        var (offsetX, offsetZ) = IsHorizontal
            ? (-modelCenter.Z * scale, modelCenter.X * scale)
            : (-modelCenter.X * scale, -modelCenter.Z * scale);

        var world = Matrix.CreateScale(scale)
            * Matrix.CreateRotationY(rotateY)
            * Matrix.CreateTranslation(offsetX, 0, offsetZ)
            * Matrix.CreateTranslation(centerX, Board.PlaneHeight, centerZ);

        graphicsDevice.DepthStencilState = DepthStencilState.Default;
        graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
        graphicsDevice.BlendState = alpha < 1f ? BlendState.AlphaBlend : BlendState.Opaque;

        foreach (var mesh in model.Meshes)
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

    public void Rotate() => IsHorizontal = !IsHorizontal;

    public void Place(int row, int col)
    {
        IsPlaced = true;
        PlacedRow = row;
        PlacedCol = col;
        PlacedHorizontal = IsHorizontal;
    }

    public void TakeDamage()
    {
        if (Life > 0) Life--;
    }

    public bool Covers(int row, int col)
    {
        for (int i = 0; i < Size; i++)
        {
            int r = PlacedHorizontal ? PlacedRow : PlacedRow + i;
            int c = PlacedHorizontal ? PlacedCol + i : PlacedCol;
            if (r == row && c == col) return true;
        }
        return false;
    }
}
