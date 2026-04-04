using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using WorldBattleNaval.Interfaces;
using WorldBattleNaval.UI;

namespace WorldBattleNaval.Scenes;

public class MainMenuScene : IScene
{
    private GraphicsDevice graphicsDevice;
    private SceneManager sceneManager;
    private UIContext uiCtx;

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

    public void LoadContent(ContentManager content)
    {
        var res = sceneManager.Resources;
        uiCtx = sceneManager.UIContext;

        var logoX = graphicsDevice.Viewport.Width / 2 - res.LogoTexture.Width / 2;
        logoImg = new Image(res.LogoTexture, logoX, 100);

        var menuWidth = 250;
        var menuX = 100;
        var menuY = graphicsDevice.Viewport.Height - 270;
        var gold = new Color(220, 160, 20);

        pvsCpuBtn = new Button(new Label("Player vs CPU", 0, 0, 0) { Color = gold }, res.ButtonTexture,
            res.ButtonPressedTexture, 0, 0, menuWidth);
        pvsPBtn = new Button(new Label("Player vs Player", 0, 0, 0) { Color = gold }, res.ButtonTexture,
            res.ButtonPressedTexture, 0, 0, menuWidth);
        settingsBtn = new Button(new Label("Configurações", 0, 0, 0) { Color = gold }, res.ButtonTexture,
            res.ButtonPressedTexture, 0, 0, menuWidth);
        quitBtn = new Button(new Label("Sair", 0, 0, 0) { Color = gold }, res.ButtonTexture, res.ButtonPressedTexture,
            0, 0, menuWidth);

        menuStack = new StackPanel(menuX, menuY, menuWidth) { Spacing = 10 };
        menuStack.AddChild(pvsCpuBtn);
        menuStack.AddChild(pvsPBtn);
        menuStack.AddChild(settingsBtn);

        IsReady = true;
    }

    public void Unload()
    {
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

        if (settingsBtn.IsClicked)
        {
            // Placeholder para cena de configurações
        }
    }

    public void Draw(GameTime gameTime)
    {
        graphicsDevice.Clear(new Color(10, 31, 68, 255));

        sceneManager.SpriteBatch.Begin();
        logoImg.Draw(uiCtx);
        menuStack.Draw(uiCtx);
        sceneManager.SpriteBatch.End();
    }
}