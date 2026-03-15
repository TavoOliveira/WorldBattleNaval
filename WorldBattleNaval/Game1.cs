using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace WorldBattleNaval;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private Model _model = null!;

    // Centro e raio calculados do bounding sphere do modelo
    private Vector3 _modelCenter;
    private float _modelRadius;

    // Rotação acumulada do modelo
    private float _rotationY = MathHelper.ToRadians(180f);
    private float _rotationX = 0f;

    // Câmera orbital
    private float _cameraDistance;
    private float _minDistance;
    private float _maxDistance;

    // Estado anterior do mouse
    private MouseState _prevMouse;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        _prevMouse = Mouse.GetState();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _model = Content.Load<Model>("models/Submarino");

        // Calcula o bounding sphere combinando todos os meshes (com transform do bone)
        var bounds = new BoundingSphere();
        foreach (var mesh in _model.Meshes)
        {
            var meshBounds = mesh.BoundingSphere.Transform(mesh.ParentBone.Transform);
            bounds = BoundingSphere.CreateMerged(bounds, meshBounds);
        }

        _modelCenter = bounds.Center;
        _modelRadius = bounds.Radius;

        // Distância inicial para caber o modelo inteiro na tela com 20% de margem
        float fovY = MathHelper.ToRadians(45f);
        float aspect = GraphicsDevice.Viewport.AspectRatio;
        float fovX = 2f * MathF.Atan(MathF.Tan(fovY / 2f) * aspect);
        float effectiveFov = MathF.Min(fovY, fovX);
        _cameraDistance = _modelRadius / MathF.Tan(effectiveFov / 2f) * 1.2f;

        _minDistance = _modelRadius * 0.5f;
        _maxDistance = _modelRadius * 10f;
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        var mouse = Mouse.GetState();

        // Botão esquerdo: arrastar para rotacionar o modelo
        if (mouse.LeftButton == ButtonState.Pressed && _prevMouse.LeftButton == ButtonState.Pressed)
        {
            int dx = mouse.X - _prevMouse.X;
            int dy = mouse.Y - _prevMouse.Y;

            _rotationY += dx * 0.01f;
            _rotationX += dy * 0.01f;
            _rotationX = MathHelper.Clamp(_rotationX, MathHelper.ToRadians(-80f), MathHelper.ToRadians(80f));
        }

        // Scroll do mouse: zoom in/out proporcional ao tamanho do modelo
        int scrollDelta = mouse.ScrollWheelValue - _prevMouse.ScrollWheelValue;
        if (scrollDelta != 0)
        {
            _cameraDistance -= scrollDelta * _modelRadius * 0.0002f;
            _cameraDistance = MathHelper.Clamp(_cameraDistance, _minDistance, _maxDistance);
        }

        _prevMouse = mouse;
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;

        // Centraliza o modelo subtraindo seu centro geométrico
        var world = Matrix.CreateTranslation(-_modelCenter) *
                    Matrix.CreateRotationX(_rotationX) *
                    Matrix.CreateRotationY(_rotationY);

        var view = Matrix.CreateLookAt(
            new Vector3(0, _cameraDistance * 0.3f, _cameraDistance),
            Vector3.Zero,
            Vector3.Up);

        var projection = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.ToRadians(45f),
            GraphicsDevice.Viewport.AspectRatio,
            _modelRadius * 0.01f,
            _modelRadius * 100f);

        foreach (var mesh in _model.Meshes)
        {
            foreach (BasicEffect effect in mesh.Effects)
            {
                effect.EnableDefaultLighting();
                effect.World = mesh.ParentBone.Transform * world;
                effect.View = view;
                effect.Projection = projection;

                effect.TextureEnabled = true;
            }

            mesh.Draw();
        }

        base.Draw(gameTime);
    }
}
