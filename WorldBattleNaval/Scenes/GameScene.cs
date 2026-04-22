using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WorldBattleNaval.Entities;
using WorldBattleNaval.Enums;
using WorldBattleNaval.Interfaces;
using WorldBattleNaval.UI;

namespace WorldBattleNaval.Scenes;

public class GameScene : IScene
{
    private const int LateralPanelWidth = 300;
    private const int HeaderPanelHeight = 50;
    private const int GridSize = 600;
    private const float CpuTurnDelay = 1f;

    private readonly GraphicsDevice graphicsDevice;
    private readonly SceneManager sceneManager;

    private ContentManager sceneContent;
    private Camera camera;

    private Texture2D texIconSize;
    private Texture2D texWaterCircle;
    private Texture2D texTargetCircle;

    private Panel headerPanel;
    private Panel lateralPanel;
    private StackPanel shipListStack;
    private Label turnLabel;
    private GridAttack gridAttack;

    private float cpuDelay;
    private ParticleSystem particleSystem;

    public bool IsReady { get; private set; }

    public GameScene(GraphicsDevice graphicsDevice, SceneManager sceneManager)
    {
        this.graphicsDevice = graphicsDevice;
        this.sceneManager = sceneManager;
    }

    public void LoadContent(IServiceProvider services)
    {
        sceneContent = new ContentManager(services) { RootDirectory = "Content" };
        camera = new Camera();
        particleSystem = new ParticleSystem(graphicsDevice);

        texIconSize = sceneContent.Load<Texture2D>("images/icon_size_ship");
        texWaterCircle = sceneContent.Load<Texture2D>("images/water_circle");
        texTargetCircle = sceneContent.Load<Texture2D>("images/target_circle");

        var ssRadar = sceneContent.Load<Texture2D>("images/radar_spritesheet");

        int mainAreaWidth = graphicsDevice.Viewport.Width - LateralPanelWidth;
        int gridX = (mainAreaWidth - GridSize) / 2;
        int gridY = HeaderPanelHeight + (graphicsDevice.Viewport.Height - HeaderPanelHeight - GridSize) / 2;
        gridAttack = new GridAttack(ssRadar, gridX, gridY, GridSize);

        InitializeUI();

        sceneManager.GameState.StartBattle();
        gridAttack.Show();

        IsReady = true;
    }

    private void InitializeUI()
    {
        headerPanel = new Panel(0, 0, graphicsDevice.Viewport.Width, HeaderPanelHeight)
        {
            Background = UITheme.LightBlue1,
        };

        var text = "Batalha";
        var textSize = sceneManager.UIContext.Font.MeasureString(text);
        var labelX = (int)((graphicsDevice.Viewport.Width - textSize.X) / 2);
        var labelY = (int)((HeaderPanelHeight - textSize.Y) / 2);
        headerPanel.AddChild(new Label(text, labelX, labelY, 0));

        int lateralPanelHeight = graphicsDevice.Viewport.Height - HeaderPanelHeight;

        shipListStack = new StackPanel(0, 80, 0) { Spacing = 10 };

        lateralPanel = new Panel(graphicsDevice.Viewport.Width - LateralPanelWidth, HeaderPanelHeight, LateralPanelWidth, lateralPanelHeight)
        {
            Padding = 10,
            Background = UITheme.LightBlue1
        };

        turnLabel = new Label("Sua vez", 10, 0, 0) { Font = sceneManager.Resources.SmallFont };
        lateralPanel.AddChild(turnLabel);
        lateralPanel.AddChild(new Label("Vida das embarcações", 10, 30, 0));
        lateralPanel.AddChild(shipListStack);

        var ctrlInfo = new Label("Segure CTRL para ver o tabuleiro", 10, lateralPanelHeight - 50, LateralPanelWidth - 20)
        {
            Font = sceneManager.Resources.TinyFont,
            Color = Color.White
        };
        lateralPanel.AddChild(ctrlInfo);

        RefreshShipList();
    }

    private void RefreshShipList()
    {
        shipListStack.ClearChildren();
        foreach (var ship in sceneManager.GameState.Player.Ships)
            shipListStack.AddChild(CreateShipItem(ship));
    }

    private UIElement CreateShipItem(Ship ship)
    {
        var panel = new Panel(0, 0, 0, 60)
        {
            Background = UITheme.LightBlue1,
            Padding = 5
        };

        var contentStack = new StackPanel(0, 0, 0) { Spacing = 5 };
        var iconStack = new StackPanel(0, 0, 0) { Spacing = 10, IsVertical = false };

        iconStack.AddChild(new Image(texIconSize, 0, 0, 24));
        iconStack.AddChild(new Label($"{ship.Life} / {ship.Size} vidas", 0, 0, 0) { Font = sceneManager.Resources.TinyFont });

        contentStack.AddChild(new Label(ship.Name, 0, 0, 0) { Font = sceneManager.Resources.SmallFont });
        contentStack.AddChild(iconStack);

        panel.AddChild(contentStack);
        return panel;
    }

    public void Unload()
    {
        sceneContent?.Unload();
        sceneContent?.Dispose();
        sceneContent = null;
        texIconSize = null;
        texWaterCircle = null;
        texTargetCircle = null;
        headerPanel = null;
        lateralPanel = null;
        shipListStack = null;
        turnLabel = null;
        gridAttack = null;
        particleSystem = null;
        IsReady = false;
    }

    public void Update(GameTime gameTime)
    {
        if (sceneManager.GameState.IsGameOver) return;

        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        particleSystem.Update(dt);
        gridAttack.Update(gameTime);

        if (sceneManager.GameState.CurrentTurn == ETurn.PLAYER)
            UpdatePlayerTurn(gameTime);
        else
            UpdateCpuTurn(gameTime);

        camera.Update(gameTime);
    }

    private void UpdatePlayerTurn(GameTime gameTime)
    {
        turnLabel.Text = "Sua vez";

        if (!gridAttack.IsLocked) return;

        int row = gridAttack.LockedRow;
        int col = gridAttack.LockedCol;

        if (gridAttack.HasMarker(row, col))
        {
            gridAttack.Unlock();
            return;
        }

        var (hit, _) = sceneManager.GameState.Cpu.ReceiveAttack(row, col);
        gridAttack.AddMarker(row, col, hit ? texTargetCircle : texWaterCircle);
        gridAttack.Unlock();

        EndTurn();
    }

    private void UpdateCpuTurn(GameTime gameTime)
    {
        turnLabel.Text = "Vez da CPU";
        if (gridAttack.IsAnimating) return;

        cpuDelay += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (cpuDelay < CpuTurnDelay) return;
        cpuDelay = 0f;

        var cpu = sceneManager.GameState.Cpu;
        var (row, col) = cpu.ChooseShot();
        var (hit, sunk) = sceneManager.GameState.Player.ReceiveAttack(row, col);
        cpu.ReportResult(row, col, hit, sunk);

        if (hit)
        {
            float half = Board.Size * Board.CellSize / 2f;
            float x = -half + col * Board.CellSize + Board.CellSize / 2f;
            float z = -half + row * Board.CellSize + Board.CellSize / 2f;
            particleSystem.AddEmitter(new Vector3(x, Board.PlaneHeight, z));
            camera.Shake(0.4f, 1.5f);
        }

        RefreshShipList();

        EndTurn();
    }

    private void EndTurn()
    {
        sceneManager.GameState.CheckGameOver();
        if (sceneManager.GameState.IsGameOver)
        {
            sceneManager.ChangeScene(new GameResultScene(graphicsDevice, sceneManager, sceneManager.GameState.PlayerWon));
            return;
        }

        sceneManager.GameState.SwitchTurn();

        if (sceneManager.GameState.CurrentTurn == ETurn.PLAYER)
            gridAttack.Show();
        else
            gridAttack.Hide();
    }

    public void Draw(GameTime gameTime)
    {
        graphicsDevice.Clear(new Color(10, 31, 68, 255));

        var view = camera.View;
        var projection = camera.Projection;
        var player = sceneManager.GameState.Player;

        player.Board.Draw(graphicsDevice, view, projection);
        player.Board.DrawOccupied(graphicsDevice, view, projection);
        player.Board.DrawHit(graphicsDevice, view, projection);

        foreach (var ship in player.Ships)
        {
            if (ship.Life > 0)
                ship.Draw(graphicsDevice, ship.PlacedRow, ship.PlacedCol, view, projection, 0.5f);
        }

        particleSystem.Draw(graphicsDevice, camera);

        sceneManager.SpriteBatch.Begin();
        
        var uiCtx = sceneManager.UIContext;
        int sx = (int)(camera.ShakeOffset.X * 10f); // Amplify for 2D UI
        int sy = (int)(camera.ShakeOffset.Y * 10f);
        
        uiCtx.PushOffset(sx, sy);
        
        headerPanel.Draw(uiCtx);
        lateralPanel.Draw(uiCtx);

        if (!InputManager.IsKeyDown(Keys.LeftControl) && !InputManager.IsKeyDown(Keys.RightControl))
            gridAttack.Draw(uiCtx);

        uiCtx.PopOffset(sx, sy);
        
        sceneManager.SpriteBatch.End();
    }
}
