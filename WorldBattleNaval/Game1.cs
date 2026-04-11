using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WorldBattleNaval.Scenes;
using WorldBattleNaval.UI;

namespace WorldBattleNaval;

public class Game1 : Game
{
    private GraphicsDeviceManager graphics;

    private SceneManager sceneManager;

    public Game1()
    {
        graphics = new GraphicsDeviceManager(this);
        graphics.PreferredBackBufferWidth = 1280;
        graphics.PreferredBackBufferHeight = 720;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        var spriteBatch = new SpriteBatch(GraphicsDevice);
        var resources   = new ResourceManager(Content, GraphicsDevice);
        var uiContext   = new UIContext(spriteBatch, resources.Pixel, resources.Font);
        sceneManager = new SceneManager(Services, spriteBatch, new GameState(), resources, uiContext);
        sceneManager.ChangeScene(new MainMenuScene(GraphicsDevice, sceneManager));
    }

    protected override void Update(GameTime gameTime)
    {
        InputManager.Update();

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            InputManager.IsKeyDown(Keys.Escape))
            Exit();

        sceneManager.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        sceneManager.Draw(gameTime);

        base.Draw(gameTime);
    }
}
