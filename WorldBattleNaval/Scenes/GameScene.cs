using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WorldBattleNaval.Entities;
using WorldBattleNaval.Interfaces;

namespace WorldBattleNaval.Scenes;

public class GameScene : IScene
{
    private readonly GraphicsDevice graphicsDevice;
    private readonly SceneManager sceneManager;

    private Camera camera;
    private Board board;
    private Ship ship;

    private ButtonState previewLeftButton;

    public bool IsReady { get; private set; }

    public GameScene(GraphicsDevice graphicsDevice, SceneManager sceneManager)
    {
        this.graphicsDevice = graphicsDevice;
        this.sceneManager = sceneManager;
    }

    public void LoadContent(ContentManager content)
    {
        var model = content.Load<Model>("models/Submarino");

        camera = new Camera();
        board = new Board();
        ship = new Ship(model, 3);

        IsReady = true;
    }

    public void Unload()
    {
        IsReady = false;
    }

    public void Update(GameTime gameTime)
    {
        var mouse = Mouse.GetState();

        HandlePlacement(mouse);

        previewLeftButton = mouse.LeftButton;
    }

    public void Draw(GameTime gameTime)
    {
        var view = camera.View;
        var projection = camera.Projection;

        board.Draw(graphicsDevice, view, projection);

        if (ship.IsPlaced)
        {
            ship.Draw(graphicsDevice, ship.PlacedRow, ship.PlacedCol,
            view, projection);
        }
        else
        {
            var (row, col) = board.CursorPosition;
            ship.Draw(graphicsDevice, row, col, view, projection);
        }
    }

    private void HandlePlacement(MouseState mouse)
    {
        if (TryGetBoardCell(mouse, out int row, out int col))
            board.SetCursor(row, col);

        var leftClicked = mouse.LeftButton == ButtonState.Pressed &&
                            previewLeftButton == ButtonState.Released;

        if (leftClicked)
        {
            var (r, c) = board.CursorPosition;
            ship.Place(r, c);
        }
    }

    private bool TryGetBoardCell(MouseState mouse, out int row, out int col)
    {
        row = col = 0;

        var viewport = graphicsDevice.Viewport;
        var mousePos = new Vector3(mouse.X, mouse.Y, 0f);

        var near = viewport.Unproject(mousePos with { Z = 0f },
            camera.Projection, camera.View, Matrix.Identity);

        var far = viewport.Unproject(mousePos with { Z = 1f },
        camera.Projection, camera.View, Matrix.Identity);

        var dir = Vector3.Normalize(far - near);

        if (MathF.Abs(dir.Y) < 1e-6f) return false;

        float t = -near.Y / dir.Y;
        if (t < 0f) return false;

        var hit = near + t * dir;

        float half = Board.Size * Board.CellSize / 2f;
        int c = (int)MathF.Floor((hit.X + half) / Board.CellSize);
        int r = (int)MathF.Floor((hit.Z + half) / Board.CellSize);

        if (r < 0 || r >= Board.Size || c < 0 || c >= Board.Size) return false;

        row = r;
        col = c;
        return true;
    }
}
