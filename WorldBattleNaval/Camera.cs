using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace WorldBattleNaval;

public class Camera
{
    public Matrix View { get; private set; }
    public Matrix Projection { get; private set; }

    private float _distance = 25f;
    private float _rotationX = MathHelper.ToRadians(55f);  // elevação
    private float _rotationY = MathHelper.ToRadians(-20f); // yaw

    private MouseState _prevMouse;

    public void Initialize(MouseState mouse)
    {
        _prevMouse = mouse;
        Rebuild(1280f / 720f);
    }

    public void Update(MouseState mouse, float aspectRatio)
    {
        // Botão direito: arrastar para orbitar
        if (mouse.RightButton == ButtonState.Pressed && _prevMouse.RightButton == ButtonState.Pressed)
        {
            float dx = (mouse.X - _prevMouse.X) * 0.01f;
            float dy = (mouse.Y - _prevMouse.Y) * 0.01f;
            _rotationY += dx;
            _rotationX = MathHelper.Clamp(_rotationX + dy,
                MathHelper.ToRadians(10f),
                MathHelper.ToRadians(89f));
        }

        // Scroll: zoom
        int scrollDelta = mouse.ScrollWheelValue - _prevMouse.ScrollWheelValue;
        if (scrollDelta != 0)
        {
            _distance -= scrollDelta * 0.015f;
            _distance = MathHelper.Clamp(_distance, 8f, 60f);
        }

        _prevMouse = mouse;
        Rebuild(aspectRatio);
    }

    private void Rebuild(float aspectRatio)
    {
        float camX = _distance * MathF.Sin(_rotationY) * MathF.Cos(_rotationX);
        float camY = _distance * MathF.Sin(_rotationX);
        float camZ = _distance * MathF.Cos(_rotationY) * MathF.Cos(_rotationX);

        View = Matrix.CreateLookAt(new Vector3(camX, camY, camZ), Vector3.Zero, Vector3.Up);
        Projection = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.ToRadians(45f),
            aspectRatio,
            0.1f,
            300f);
    }
}
