using System;
using Microsoft.Xna.Framework;

namespace WorldBattleNaval;

public class Camera
{
    public Matrix View { get; private set; }
    public Matrix Projection { get; private set; }

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

        var position = new Vector3(camX, camY, camZ) + target;

        View = Matrix.CreateLookAt(
            position,
            target,
            Vector3.Up
        );

        Projection = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.ToRadians(45f),
            aspectRatio,
            0.1f,
            300f
        );
    }
}