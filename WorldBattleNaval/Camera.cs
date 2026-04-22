using System;
using Microsoft.Xna.Framework;

namespace WorldBattleNaval;

public class Camera
{
    public Matrix View { get; private set; }
    public Matrix Projection { get; private set; }
    public Vector3 Position { get; private set; }
    public Vector3 Right { get; private set; }
    public Vector3 Up { get; private set; }

    private float distance = 30f;
    private float rotateX = MathHelper.ToRadians(89f);
    private float rotateY = MathHelper.ToRadians(0f);

    private Vector3 target = Vector3.Zero;
    private float aspectRatio = 1280f / 720f;

    public Camera()
    {
        Move(5, 0);
    }

    public void Move(float deltaX, float deltaZ)
    {
        target.X += deltaX;
        target.Z += deltaZ;
        Rebuild();
    } 

    private void Rebuild()
    {
        var camX = distance * MathF.Sin(rotateY) * MathF.Cos(rotateX);
        var camY = distance * MathF.Sin(rotateX);
        var camZ = distance * MathF.Cos(rotateY) * MathF.Cos(rotateX);

        Position = new Vector3(camX, camY, camZ) + target;

        View = Matrix.CreateLookAt(
            Position,
            target,
            Vector3.Up
        );

        Right = Vector3.Normalize(Vector3.Cross(Vector3.Up, Position - target));
        Up = Vector3.Normalize(Vector3.Cross(Position - target, Right));

        Projection = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.ToRadians(45f),
            aspectRatio,
            0.1f,
            300f
        );
    }
}