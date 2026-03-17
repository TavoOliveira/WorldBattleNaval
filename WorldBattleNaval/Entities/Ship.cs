using System.Buffers.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WorldBattleNaval.Entities;

public class Ship
{
    public int Size { get; private set; }

    private readonly Model model;
    private readonly Vector3 modelCenter;
    private readonly float modelRadius;

    public bool IsHorizontal { get; private set; } = true;

    public Ship(Model model, int size)
    {
        this.model = model;
        Size = size;

        var bounds = new BoundingSphere();

        foreach (var mesh in model.Meshes)
        {
            var meshBounds = mesh.BoundingSphere.Transform(mesh.ParentBone.Transform);
            bounds = BoundingSphere.CreateMerged(bounds, meshBounds);
        }

        modelCenter = bounds.Center;
        modelRadius = bounds.Radius;
    }

    public void Rotate() => IsHorizontal = !IsHorizontal;

    public void Draw(GraphicsDevice graphicsDevice, int row, int col, Matrix view, Matrix projection)
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

        var rotateY = IsHorizontal ? 0f : MathHelper.ToRadians(90f);

        var (offsetX, offsetZ) = IsHorizontal
            ? (-modelCenter.X * scale, -modelCenter.Z * scale)
            : (-modelCenter.Z * scale, modelCenter.X * scale);

        var world = Matrix.CreateScale(scale)
            * Matrix.CreateRotationY(rotateY)
            * Matrix.CreateTranslation(offsetX, 0, offsetZ)
            * Matrix.CreateTranslation(centerX, 0.18f, centerZ);

        graphicsDevice.DepthStencilState = DepthStencilState.Default;
        graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
        graphicsDevice.BlendState = BlendState.Opaque;

        foreach (var mesh in model.Meshes)
        {
            foreach (BasicEffect effect in mesh.Effects)
            {
                effect.World = mesh.ParentBone.Transform * world;
                effect.View = view;
                effect.Projection = projection;
            }

            mesh.Draw();
        }
    }
}
