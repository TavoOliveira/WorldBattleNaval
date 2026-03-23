using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WorldBattleNaval.Interfaces;
using WorldBattleNaval.UI;

namespace WorldBattleNaval.Scenes;

public class MainMenuScene : IScene
{
    private GraphicsDevice graphicsDevice;
    private SceneManager sceneManager;
    private SpriteBatch spriteBatch;
    private SpriteFont font;
    private UIContext uiCtx;

    private StackPanel menuStack;
    private Button pvsCpuBtn;
    private Button pvsPBtn;
    private Button settingsBtn;

    public bool IsReady { get; private set; }

    public MainMenuScene(GraphicsDevice graphicsDevice, SceneManager sceneManager)
    {
        this.graphicsDevice = graphicsDevice;
        this.sceneManager = sceneManager;
    }

    public void LoadContent(ContentManager content)
    {
        spriteBatch = new SpriteBatch(graphicsDevice);
        font = content.Load<SpriteFont>("fonts/Font");

        var texture = new Texture2D(graphicsDevice, 1, 1);
        texture.SetData(new[] { Color.White });
        uiCtx = new UIContext(spriteBatch, texture, font);

        int menuWidth = 250;
        int centerX = graphicsDevice.Viewport.Width / 2 - menuWidth / 2;
        int centerY = graphicsDevice.Viewport.Height / 2 - 100;

        pvsCpuBtn = new Button("Player vs CPU", 0, 0, menuWidth);
        pvsPBtn = new Button("Player vs Player", 0, 0, menuWidth);
        settingsBtn = new Button("Configuracoes", 0, 0, menuWidth);

        menuStack = new StackPanel(centerX, centerY, menuWidth) { Spacing = 10 };
        menuStack.Add(new Label("WORLD BATTLE NAVAL", 0, 0, menuWidth) { Color = Color.LightSkyBlue })
                 .Add(pvsCpuBtn)
                 .Add(pvsPBtn)
                 .Add(settingsBtn);

        IsReady = true;
    }

    public void Unload()
    {
        IsReady = false;
    }

    public void Update(GameTime gameTime)
    {
        var mouseState = Mouse.GetState();

        // Atualiza os botões manualmente pois o StackPanel não possui lógica de Update
        pvsCpuBtn.Update(mouseState);
        pvsPBtn.Update(mouseState);
        settingsBtn.Update(mouseState);

        if (pvsCpuBtn.IsClicked || pvsPBtn.IsClicked)
        {
            sceneManager.ChangeScene(new GameScene(graphicsDevice, sceneManager));
        }

        if (settingsBtn.IsClicked)
        {
            // Placeholder para cena de configurações
        }
    }

    public void Draw(GameTime gameTime)
    {
        graphicsDevice.Clear(Color.Black);

        spriteBatch.Begin();
        menuStack.Draw(uiCtx);
        spriteBatch.End();
    }
}