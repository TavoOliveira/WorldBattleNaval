using System;
using System.Collections.Generic;
using System.Security.Cryptography;
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

    private Panel headerPanel;
    private Panel lateralPanel;

    public bool IsReady { get; private set; }

    public PlacementScene(GraphicsDevice graphicsDevice, SceneManager sceneManager)
    {
        this.graphicsDevice = graphicsDevice;
        this.sceneManager = sceneManager;
    }

    public void LoadContent(ContentManager content)
    {
        uiCtx = sceneManager.UIContext;
        camera = new Camera();

        InitializeShips();
        InitializeUI(content);

        IsReady = true;
    }

    private void InitializeShips()
    {
        var model = sceneManager.Resources.SubmarineModel;

        pendingShips = [
            new Ship("Submarino", model, 3), new Ship("Submarino", model, 3),
            new Ship("Submarino", model, 3), new Ship("Submarino", model, 3)
        ];

        cpuShips = [
            new Ship("Submarino", model, 3), new Ship("Submarino", model, 3),
            new Ship("Submarino", model, 3), new Ship("Submarino", model, 3)
        ];
    }

    private void InitializeUI(ContentManager content)
    {
        const int headerPanelHeight = 50;

        headerPanel = new Panel(0, 0, graphicsDevice.Viewport.Width, headerPanelHeight)
        {
            Background = UITheme.LightBlue1,
        };

        var text = "Monte seu tabuleiro";

        var textSize = uiCtx.Font.MeasureString(text);
        var labelX = (int)((graphicsDevice.Viewport.Width  - textSize.X) / 2);
        var labelY = (int)((headerPanelHeight - textSize.Y) / 2);

        headerPanel.AddChild(new Label(text, labelX, labelY, 0));

        const int lateralPanelWidth = 300;

        var shipListStack = new StackPanel(0, 50, 0) { Spacing = 10 };

        foreach (var ship in pendingShips)
            shipListStack.AddChild(CreateShipItem(content, ship.Name, "images/screenshot_submarine"));

        lateralPanel = new Panel(graphicsDevice.Viewport.Width - lateralPanelWidth, headerPanelHeight, lateralPanelWidth, graphicsDevice.Viewport.Height - headerPanelHeight)
        {
            Padding = 10,
            Background = UITheme.LightBlue1
        };

        lateralPanel.AddChild(new Label("Suas embarcações", 10, 0, 0));
        lateralPanel.AddChild(shipListStack);
    }

    private UIElement CreateShipItem(ContentManager content, string name, string imagePath)
    {
        var subImage = content.Load<Texture2D>(imagePath);
        var iconSizeShip = content.Load<Texture2D>("images/icon_size_ship");

        var panel = new Panel(0, 0, 0, 70)
        {
            Background = UITheme.LightBlue1,
            BorderColor = UITheme.LightBlue2,
            Padding = 5
        };

        var contentStack = new StackPanel(0, 0, 0) { IsVertical = false };
        var contentLeftStack = new StackPanel(0, 0, 0) { Spacing = 5 };
        var iconContentStack = new StackPanel(0, 0, 0) { Spacing = 10, IsVertical = false };

        iconContentStack.AddChild(new Image(iconSizeShip, 0, 0, 24));
        iconContentStack.AddChild(new Label("3 espaços", 0, 0, 0) { Font = sceneManager.Resources.TinyFont });

        contentLeftStack.AddChild(new Label(name, 0, 0, 0) { Font = sceneManager.Resources.SmallFont });
        contentLeftStack.AddChild(iconContentStack);
        contentStack.AddChild(contentLeftStack);
        contentStack.AddChild(new Image(subImage, 50, 5, 100, 50));

        panel.AddChild(contentStack);
        return panel;
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
            var (row, col) = player.Board.CursorPosition;
            pendingShips[currentShipIndex].Draw(graphicsDevice, row, col, view, projection);
        }

        sceneManager.SpriteBatch.Begin();
        headerPanel.Draw(uiCtx);
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
