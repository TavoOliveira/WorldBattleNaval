using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WorldBattleNaval.Scenes;

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
        sceneManager = new SceneManager(Content);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        sceneManager.ChangeScene(new GameScene(GraphicsDevice, sceneManager));
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
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
