using System;
using Microsoft.Xna.Framework;

namespace WorldBattleNaval;

public class Camera
{
    public Matrix View { get; private set; }
    public Matrix Projection { get; private set; }

    private float distance = 25f;
    private float rotateX = MathHelper.ToRadians(55f);
    private float rotateY = MathHelper.ToRadians(55f);

    public Camera()
    {
        Rebuild(1280f / 720f);
    }

    private void Rebuild(float aspectRatio)
    {
        var camX = distance * MathF.Sin(rotateY) * MathF.Cos(rotateX);
        var camY = distance * MathF.Sin(rotateX);
        var camZ = distance * MathF.Cos(rotateY) * MathF.Cos(rotateX);

        View = Matrix.CreateLookAt(
            new Vector3(camX, camY, camZ),
            Vector3.Zero,
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