using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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

    private Texture2D texIconSize;
    private Texture2D texIconSelection;
    private Texture2D texButton;
    private Texture2D texButtonPressed;

    private Panel headerPanel;
    private Panel lateralPanel;
    private StackPanel shipListStack;
    private Button btnPlay;

    private bool isMouseOverBoard;

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

        texIconSize = sceneContent.Load<Texture2D>("images/icon_size_ship");
        texIconSelection = sceneContent.Load<Texture2D>("images/icon_selection");
        texButton = sceneContent.Load<Texture2D>("images/bg_button");
        texButtonPressed = sceneContent.Load<Texture2D>("images/bg_button_pressed");

        InitializeShips();
        InitializeUI();

        IsReady = true;
    }

    private void InitializeShips()
    {
        var carrierModel = sceneManager.Resources.CarrierModel;
        var battleshipModel = sceneManager.Resources.BattleshipModel;
        var submarineModel = sceneManager.Resources.SubmarineModel;
        var cruiserModel = sceneManager.Resources.CruiserModel;
        var destroyerModel = sceneManager.Resources.DestroyerModel;

        var carrierTex = sceneContent.Load<Texture2D>("images/screenshots/screenshot_aircraft_carrier");
        var battleshipTex = sceneContent.Load<Texture2D>("images/screenshots/screenshot_battleship");
        var submarineTex = sceneContent.Load<Texture2D>("images/screenshots/screenshot_submarine");
        var cruiserTex = sceneContent.Load<Texture2D>("images/screenshots/screenshot_cruiser");
        var destroyerTex = sceneContent.Load<Texture2D>("images/screenshots/screenshot_destroyer");

        var rot90 = MathHelper.ToRadians(-90f);

        pendingShips = [
            new RadioShipModel { Ship = new Ship("Porta-aviões", carrierModel, 5), Screenshot = carrierTex, IsSelected = true },
            new RadioShipModel { Ship = new Ship("Encouraçado", battleshipModel, 4, rot90), Screenshot = battleshipTex },
            new RadioShipModel { Ship = new Ship("Submarino", submarineModel, 3), Screenshot = submarineTex },
            new RadioShipModel { Ship = new Ship("Cruzador", cruiserModel, 3, rot90), Screenshot = cruiserTex },
            new RadioShipModel { Ship = new Ship("Destroier", destroyerModel, 2, rot90), Screenshot = destroyerTex }
        ];

        selectedShipIndex = 0;

        cpuShips = [
            new Ship("Porta-aviões", carrierModel, 5),
            new Ship("Encouraçado", battleshipModel, 4, rot90),
            new Ship("Submarino", submarineModel, 3),
            new Ship("Cruzador", cruiserModel, 3, rot90),
            new Ship("Destroier", destroyerModel, 2, rot90)
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
        int lateralPanelHeight = graphicsDevice.Viewport.Height - headerPanelHeight;

        shipListStack = new StackPanel(0, 50, 0) { Spacing = 10 };

        lateralPanel = new Panel(graphicsDevice.Viewport.Width - lateralPanelWidth, headerPanelHeight, lateralPanelWidth, lateralPanelHeight)
        {
            Padding = 10,
            Background = UITheme.LightBlue1
        };

        lateralPanel.AddChild(new Label("Suas embarcações", 10, 0, 0));
        lateralPanel.AddChild(shipListStack);

        var infoLabel = new Label("Clique direito ou R para girar", 10, lateralPanelHeight - 120, lateralPanelWidth - 20)
        {
            Font = sceneManager.Resources.SmallFont,
            Color = Color.White
        };
        lateralPanel.AddChild(infoLabel);

        btnPlay = Button.ReturnButton("Jogar", sceneManager.Resources, sceneManager.UIContext);
        btnPlay.X = 10;
        btnPlay.Y = lateralPanelHeight - 70;
        btnPlay.Width = lateralPanelWidth - 20;
        btnPlay.Height = 50;
        btnPlay.Enabled = false;
        
        lateralPanel.AddChild(btnPlay);

        RefreshShipList();
    }

    private void RefreshShipList()
    {
        shipListStack.ClearChildren();
        foreach (var radioShip in pendingShips)
            shipListStack.AddChild(CreateShipItem(radioShip));
    }

    private UIElement CreateShipItem(RadioShipModel radioShip)
    {
        var name = radioShip.Ship.Name;
        var isSelected = radioShip.IsSelected;
        var size = radioShip.Ship.Size;
        var screenshot = radioShip.Screenshot;

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
        iconContentStack.AddChild(new Label($"{size} espaços", 0, 0, 0) { Font = sceneManager.Resources.TinyFont });

        contentLeftStack.AddChild(new Label(name, 0, 0, 0) { Font = sceneManager.Resources.SmallFont });
        contentLeftStack.AddChild(iconContentStack);
        contentStack.AddChild(contentLeftStack);
        contentStack.AddChild(new Image(screenshot, 50, 5, 100, 50));

        panel.AddChild(contentStack);
        if (isSelected) panel.AddChild(new Image(texIconSelection, 240, 0, 24));
        return panel;
    }

    public void Unload()
    {
        sceneContent?.Unload();
        sceneContent?.Dispose();
        sceneContent = null;
        texIconSize = null;
        texIconSelection = null;
        texButton = null;
        texButtonPressed = null;
        headerPanel = null;
        lateralPanel = null;
        shipListStack = null;
        btnPlay = null;
        IsReady = false;
    }

    public void Update(GameTime gameTime)
    {
        if (pendingShips.Count > 0)
        {
            UpdateShipSelection();
            HandlePlacement();
        }
        else
        {
            btnPlay.Enabled = true;
            isMouseOverBoard = false;
        }

        btnPlay.Update();
        if (btnPlay.IsClicked && btnPlay.Enabled)
        {
            sceneManager.ChangeScene(new GameScene(graphicsDevice, sceneManager));
        }
    }

    private void UpdateShipSelection()
    {
        if (!InputManager.IsLeftClicked) return;

        var mp = InputManager.MousePosition;
        int lateralX = graphicsDevice.Viewport.Width - 300;

        if (mp.X >= lateralX)
        {
            int startY = 50 + 10 + 50;
            int relativeY = mp.Y - startY;

            if (relativeY >= 0)
            {
                int index = relativeY / (70 + 10);
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
        player.Board.DrawOccupied(graphicsDevice, view, projection);

        foreach (var ship in player.Ships)
            ship.Draw(graphicsDevice, ship.PlacedRow, ship.PlacedCol, view, projection, 0.5f);

        if (pendingShips.Count > 0)
        {
            var (row, col) = player.Board.CursorPosition;
            if (isMouseOverBoard)
            {
                var ship = pendingShips[selectedShipIndex].Ship;
                player.Board.DrawPreview(graphicsDevice, row, col, ship.Size, ship.IsHorizontal, view, projection);
                ship.Draw(graphicsDevice, row, col, view, projection, 0.5f);
            }
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

        isMouseOverBoard = TryGetBoardCell(out int row, out int col);
        if (isMouseOverBoard)
            player.Board.SetCursor(row, col);

        if (InputManager.IsRightPressed || InputManager.IsKeyPressed(Keys.R))
            current.Rotate();

        if (InputManager.IsLeftClicked)
        {
            var mp = InputManager.MousePosition;
            if (mp.X >= graphicsDevice.Viewport.Width - 300) return;

            var (r, c) = player.Board.CursorPosition;

            if (isMouseOverBoard && player.Board.CanPlace(r, c, current.Size, current.IsHorizontal))
            {
                player.Board.Place(r, c, current.Size, current.IsHorizontal);
                current.Place(r, c);
                player.Ships.Add(current);

                pendingShips.RemoveAt(selectedShipIndex);
                if (pendingShips.Count > 0)
                {
                    selectedShipIndex = Math.Clamp(selectedShipIndex, 0, pendingShips.Count - 1);
                    pendingShips[selectedShipIndex].IsSelected = true;
                }
                else
                {
                    sceneManager.GameState.Cpu.Setup(cpuShips);
                }
                
                RefreshShipList();
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

        float t = (Board.PlaneHeight - near.Y) / dir.Y;
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
