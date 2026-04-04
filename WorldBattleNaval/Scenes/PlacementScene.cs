using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using WorldBattleNaval.Entities;
using WorldBattleNaval.Interfaces;
using WorldBattleNaval.UI;

namespace WorldBattleNaval.Scenes;

public class PlacementScene : IScene
{
    private readonly GraphicsDevice graphicsDevice;
    private readonly SceneManager sceneManager;
    private UIContext uiCtx;

    private Camera camera;
    private List<Ship> pendingShips;
    private List<Ship> cpuShips;
    private int currentShipIndex;

    private Panel lateralPanel;

    public bool IsReady { get; private set; }

    public PlacementScene(GraphicsDevice graphicsDevice, SceneManager sceneManager)
    {
        this.graphicsDevice = graphicsDevice;
        this.sceneManager = sceneManager;
    }

    public void LoadContent(ContentManager content)
    {
        var model = sceneManager.Resources.SubmarineModel;
        var subImage = content.Load<Texture2D>("images/screenshot_submarine");
        uiCtx = sceneManager.UIContext;
        camera = new Camera();

        pendingShips =
        [
            new Ship(model, 3),
            new Ship(model, 3),
            new Ship(model, 3),
            new Ship(model, 3),
        ];

        cpuShips =
        [
            new Ship(model, 3),
            new Ship(model, 3),
            new Ship(model, 3),
            new Ship(model, 3),
        ];
        var lateralPanelWidth = 300;

        var stackPanel = new StackPanel(0, 50, 0) { Spacing = 10 };

        var shipPanel = new Panel(0, 0, 0, 100)
        {
            Background = UITheme.LightBlue1,
            BorderColor = UITheme.LightBlue2,
            Padding = 5
        };

        var shipStackPanel = new StackPanel(0, 0, 0)
        {
            Spacing = 10
        };

        var widthImageShip = 100;

        var imageShip = new Image(subImage, 100, 0, widthImageShip, 50);

        shipStackPanel.AddChild(new Label("Submarino", 0, 0, shipStackPanel.Width) { Font = sceneManager.Resources.SmallFont });
        shipStackPanel.AddChild(imageShip);

        shipPanel.AddChild(shipStackPanel);

        stackPanel.AddChild(shipPanel);

        lateralPanel = new Panel(graphicsDevice.Viewport.Width - lateralPanelWidth, 0, lateralPanelWidth, graphicsDevice.Viewport.Height)
        {
            Padding = 10,
            Background = UITheme.LightBlue1
        };

        lateralPanel.AddChild(new Label("Suas embarcações", 10, 0, 0));
        lateralPanel.AddChild(stackPanel);

        IsReady = true;
    }

    public void Unload() => IsReady = false;

    public void Update(GameTime gameTime)
    {
        if (currentShipIndex < pendingShips.Count)
            HandlePlacement();
    }

    public void Draw(GameTime gameTime)
    {
        var view = camera.View;
        var projection = camera.Projection;

        var player = sceneManager.GameState.Player;

        player.Board.Draw(graphicsDevice, view, projection);

        foreach (var ship in player.Ships)
            ship.Draw(graphicsDevice, ship.PlacedRow, ship.PlacedCol, view, projection);

        if (currentShipIndex < pendingShips.Count)
        {
            var current = pendingShips[currentShipIndex];
            var (row, col) = player.Board.CursorPosition;
            current.Draw(graphicsDevice, row, col, view, projection);
        }

        sceneManager.SpriteBatch.Begin();
        lateralPanel.Draw(uiCtx);
        sceneManager.SpriteBatch.End();
    }

    private void HandlePlacement()
    {
        var player = sceneManager.GameState.Player;
        var current = pendingShips[currentShipIndex];

        if (TryGetBoardCell(out int row, out int col))
            player.Board.SetCursor(row, col);

        if (InputManager.IsRightPressed)
            current.Rotate();

        if (InputManager.IsLeftClicked)
        {
            var (r, c) = player.Board.CursorPosition;

            if (player.Board.CanPlace(r, c, current.Size, current.IsHorizontal))
            {
                player.Board.Place(r, c, current.Size, current.IsHorizontal);
                current.Place(r, c);
                player.Ships.Add(current);
                currentShipIndex++;

                // if (currentShipIndex >= pendingShips.Count)
                // {
                //     sceneManager.GameState.Cpu.Setup(cpuShips);
                //     sceneManager.GameState.StartBattle();
                //     sceneManager.ChangeScene(new GameScene(graphicsDevice, sceneManager));
                // }
            }
        }
    }

    private bool TryGetBoardCell(out int row, out int col)
    {
        row = col = 0;

        var viewport = graphicsDevice.Viewport;
        var mp = InputManager.MousePosition;
        var mousePos = new Vector3(mp.X, mp.Y, 0f);

        var near = viewport.Unproject(mousePos with { Z = 0f }, camera.Projection, camera.View, Matrix.Identity);
        var far = viewport.Unproject(mousePos with { Z = 1f }, camera.Projection, camera.View, Matrix.Identity);
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
