using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using WorldBattleNaval.Interfaces;
using WorldBattleNaval.UI;

namespace WorldBattleNaval.Scenes;

public class MainMenuScene : IScene
{
    private readonly GraphicsDevice graphicsDevice;
    private readonly SceneManager sceneManager;
    private UIContext uiCtx;

    private ContentManager sceneContent;
    private Image logoImg;
    private StackPanel menuStack;

    private Button pvsCpuBtn;
    private Button pvsPBtn;
    private Button settingsBtn;
    private Button quitBtn;

    public bool IsReady { get; private set; }

    public MainMenuScene(GraphicsDevice graphicsDevice, SceneManager sceneManager)
    {
        this.graphicsDevice = graphicsDevice;
        this.sceneManager = sceneManager;
    }

    public void LoadContent(IServiceProvider services)
    {
        sceneContent = new ContentManager(services) { RootDirectory = "Content" };

        var res = sceneManager.Resources;
        uiCtx = sceneManager.UIContext;

        var logoTexture = sceneContent.Load<Texture2D>("images/logo");
        var logoX = graphicsDevice.Viewport.Width / 2 - logoTexture.Width / 2;
        logoImg = new Image(logoTexture, logoX, 100);

        pvsCpuBtn = CreateMenuButton("Player vs CPU", res);

        pvsPBtn = CreateMenuButton("Player vs Player", res);
        settingsBtn = CreateMenuButton("Configurações", res);
        quitBtn = CreateMenuButton("Sair", res);

        var menuX = 100;
        var menuY = graphicsDevice.Viewport.Height - 270;

        menuStack = new StackPanel(menuX, menuY, 0) { Spacing = 10 };
        menuStack.AddChild(pvsCpuBtn);
        menuStack.AddChild(pvsPBtn);
        menuStack.AddChild(settingsBtn);
        menuStack.AddChild(quitBtn);

        IsReady = true;
    }

    public void Unload()
    {
        sceneContent?.Unload();
        sceneContent?.Dispose();
        sceneContent = null;
        logoImg = null;
        menuStack = null;
        pvsCpuBtn = null;
        pvsPBtn = null;
        settingsBtn = null;
        quitBtn = null;
        IsReady = false;
    }

    public void Update(GameTime gameTime)
    {
        pvsCpuBtn.Update();
        pvsPBtn.Update();
        settingsBtn.Update();
        quitBtn.Update();

        if (pvsCpuBtn.IsClicked || pvsPBtn.IsClicked)
            sceneManager.ChangeScene(new PlacementScene(graphicsDevice, sceneManager));

        if (quitBtn.IsClicked)
            sceneManager.QuitGame();
    }

    public void Draw(GameTime gameTime)
    {
        graphicsDevice.Clear(new Color(10, 31, 68, 255));

        sceneManager.SpriteBatch.Begin();
        logoImg.Draw(uiCtx);
        menuStack.Draw(uiCtx);
        sceneManager.SpriteBatch.End();
    }

    private Button CreateMenuButton(string text, ResourceManager res)
    {
        const int buttonWidth  = 250;
        const int buttonHeight = 50;
        var gold = new Color(220, 160, 20);

        var textSize = uiCtx.Font.MeasureString(text);
        var labelX = (int)((buttonWidth  - textSize.X) / 2);
        var labelY = (int)((buttonHeight - textSize.Y) / 2);

        var label = new Label(text, labelX, labelY, 0) { Color = gold };
        return new Button(label, res.ButtonTexture, res.ButtonPressedTexture, 0, 0, buttonWidth);
    }
}
