using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using WorldBattleNaval.Entities;
using WorldBattleNaval.Interfaces;
using WorldBattleNaval.Models;
using WorldBattleNaval.UI;

namespace WorldBattleNaval.Scenes;

public class PlacementScene : IScene
{
    private readonly GraphicsDevice graphicsDevice;
    private readonly SceneManager sceneManager;

    private ContentManager sceneContent;
    private Camera camera;
    private List<RadioShipModel> pendingShips;
    private List<Ship> cpuShips;
    private int selectedShipIndex;

    private Texture2D texSubmarine;
    private Texture2D texIconSize;
    private Texture2D texIconSelection;
    private Model submarineModel;

    private Panel headerPanel;
    private Panel lateralPanel;
    private StackPanel shipListStack;

    public bool IsReady { get; private set; }

    public PlacementScene(GraphicsDevice graphicsDevice, SceneManager sceneManager)
    {
        this.graphicsDevice = graphicsDevice;
        this.sceneManager = sceneManager;
    }

    public void LoadContent(IServiceProvider services)
    {
        sceneContent = new ContentManager(services) { RootDirectory = "Content" };
        camera = new Camera();

        texSubmarine = sceneContent.Load<Texture2D>("images/screenshot_submarine");
        texIconSize = sceneContent.Load<Texture2D>("images/icon_size_ship");
        texIconSelection = sceneContent.Load<Texture2D>("images/icon_selection");
        submarineModel = sceneContent.Load<Model>("models/Submarine");

        InitializeShips();
        InitializeUI();

        IsReady = true;
    }

    private void InitializeShips()
    {
        pendingShips = [
            new RadioShipModel { Ship = new Ship("Submarino", submarineModel, 3), IsSelected = true },
            new RadioShipModel { Ship = new Ship("Submarino", submarineModel, 3) },
            new RadioShipModel { Ship = new Ship("Submarino", submarineModel, 3) },
            new RadioShipModel { Ship = new Ship("Submarino", submarineModel, 3) }
        ];

        selectedShipIndex = 0;

        cpuShips = [
            new Ship("Submarino", submarineModel, 3), new Ship("Submarino", submarineModel, 3),
            new Ship("Submarino", submarineModel, 3), new Ship("Submarino", submarineModel, 3)
        ];
    }

    private void InitializeUI()
    {
        const int headerPanelHeight = 50;

        headerPanel = new Panel(0, 0, graphicsDevice.Viewport.Width, headerPanelHeight)
        {
            Background = UITheme.LightBlue1,
        };

        var text = "Monte seu tabuleiro";

        var textSize = sceneManager.UIContext.Font.MeasureString(text);
        var labelX = (int)((graphicsDevice.Viewport.Width - textSize.X) / 2);
        var labelY = (int)((headerPanelHeight - textSize.Y) / 2);

        headerPanel.AddChild(new Label(text, labelX, labelY, 0));

        const int lateralPanelWidth = 300;

        shipListStack = new StackPanel(0, 50, 0) { Spacing = 10 };

        lateralPanel = new Panel(graphicsDevice.Viewport.Width - lateralPanelWidth, headerPanelHeight, lateralPanelWidth, graphicsDevice.Viewport.Height - headerPanelHeight)
        {
            Padding = 10,
            Background = UITheme.LightBlue1
        };

        lateralPanel.AddChild(new Label("Suas embarcações", 10, 0, 0));
        lateralPanel.AddChild(shipListStack);

        RefreshShipList();
    }

    private void RefreshShipList()
    {
        shipListStack.ClearChildren();
        foreach (var radioShip in pendingShips)
            shipListStack.AddChild(CreateShipItem(radioShip.Ship.Name, radioShip.IsSelected));
    }

    private UIElement CreateShipItem(string name, bool isSelected)
    {

        var panel = new Panel(0, 0, 0, 70)
        {
            Background = UITheme.LightBlue1,
            BorderColor = isSelected ? UITheme.LightBlue2 : Color.Transparent,
            Padding = 5
        };

        var contentStack = new StackPanel(0, 0, 0) { IsVertical = false };
        var contentLeftStack = new StackPanel(0, 0, 0) { Spacing = 5 };
        var iconContentStack = new StackPanel(0, 0, 0) { Spacing = 10, IsVertical = false };

        iconContentStack.AddChild(new Image(texIconSize, 0, 0, 24));
        iconContentStack.AddChild(new Label("3 espaços", 0, 0, 0) { Font = sceneManager.Resources.TinyFont });

        contentLeftStack.AddChild(new Label(name, 0, 0, 0) { Font = sceneManager.Resources.SmallFont });
        contentLeftStack.AddChild(iconContentStack);
        contentStack.AddChild(contentLeftStack);
        contentStack.AddChild(new Image(texSubmarine, 50, 5, 100, 50));

        panel.AddChild(contentStack);
        if (isSelected) panel.AddChild(new Image(texIconSelection, 240, 0, 24));
        return panel;
    }

    public void Unload()
    {
        sceneContent?.Unload();
        sceneContent?.Dispose();
        sceneContent = null;
        texSubmarine = null;
        texIconSize = null;
        texIconSelection = null;
        headerPanel = null;
        lateralPanel = null;
        shipListStack = null;
        IsReady = false;
    }

    public void Update(GameTime gameTime)
    {
        if (pendingShips.Count > 0)
        {
            UpdateShipSelection();
            HandlePlacement();
        }
    }

    private void UpdateShipSelection()
    {
        if (!InputManager.IsLeftClicked) return;

        var mp = InputManager.MousePosition;
        int lateralX = graphicsDevice.Viewport.Width - 300;

        if (mp.X >= lateralX)
        {
            // Relative Y calculation considering lateralPanel position, padding, and shipListStack offset
            int startY = 50 + 10 + 50; // headerPanelHeight + lateralPanel.Padding + shipListStack.Y
            int relativeY = mp.Y - startY;

            if (relativeY >= 0)
            {
                int index = relativeY / (70 + 10); // shipItemHeight + shipListStack.Spacing
                if (index >= 0 && index < pendingShips.Count)
                {
                    int itemTop = index * (70 + 10);
                    if (relativeY >= itemTop && relativeY < itemTop + 70)
                    {
                        SelectShip(index);
                    }
                }
            }
        }
    }

    private void SelectShip(int index)
    {
        for (int i = 0; i < pendingShips.Count; i++)
            pendingShips[i].IsSelected = (i == index);

        selectedShipIndex = index;
        RefreshShipList();
    }

    public void Draw(GameTime gameTime)
    {
        var view = camera.View;
        var projection = camera.Projection;
        var player = sceneManager.GameState.Player;

        player.Board.Draw(graphicsDevice, view, projection);

        foreach (var ship in player.Ships)
            ship.Draw(graphicsDevice, ship.PlacedRow, ship.PlacedCol, view, projection);

        if (pendingShips.Count > 0)
        {
            var (row, col) = player.Board.CursorPosition;
            pendingShips[selectedShipIndex].Ship.Draw(graphicsDevice, row, col, view, projection);
        }

        sceneManager.SpriteBatch.Begin();
        headerPanel.Draw(sceneManager.UIContext);
        lateralPanel.Draw(sceneManager.UIContext);
        sceneManager.SpriteBatch.End();
    }

    private void HandlePlacement()
    {
        var player = sceneManager.GameState.Player;
        var current = pendingShips[selectedShipIndex].Ship;

        if (TryGetBoardCell(out int row, out int col))
            player.Board.SetCursor(row, col);

        if (InputManager.IsRightPressed)
            current.Rotate();

        if (InputManager.IsLeftClicked)
        {
            var mp = InputManager.MousePosition;
            if (mp.X >= graphicsDevice.Viewport.Width - 300) return; // Ignore click on lateral panel for placement

            var (r, c) = player.Board.CursorPosition;

            if (player.Board.CanPlace(r, c, current.Size, current.IsHorizontal))
            {
                player.Board.Place(r, c, current.Size, current.IsHorizontal);
                current.Place(r, c);
                player.Ships.Add(current);

                pendingShips.RemoveAt(selectedShipIndex);
                if (pendingShips.Count > 0)
                {
                    selectedShipIndex = Math.Clamp(selectedShipIndex, 0, pendingShips.Count - 1);
                    pendingShips[selectedShipIndex].IsSelected = true;
                    RefreshShipList();
                }
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
