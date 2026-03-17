using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using WorldBattleNaval.Interfaces;

namespace WorldBattleNaval.Scenes;

public class MenuScene : IScene
{
    private readonly GraphicsDevice graphicsDevice;
    private readonly SceneManager sceneManager;

    private SpriteBatch spriteBatch;
    private SpriteFont font;

    private const string Title = "WorldBattle Naval";

    public bool IsReady { get; private set; }

    public MenuScene(GraphicsDevice graphicsDevice, SceneManager sceneManager)
    {
        this.graphicsDevice = graphicsDevice;
        this.sceneManager = sceneManager;
    }

    public void LoadContent(ContentManager content)
    {
        spriteBatch = new SpriteBatch(graphicsDevice);
        font = content.Load<SpriteFont>("fonts/Font");

        IsReady = true;
    }

    public void Unload()
    {
        spriteBatch.Dispose();
        IsReady = false;
    }

    public void Update(GameTime gameTime)
    {
    }

    public void Draw(GameTime gameTime)
    {
        var viewport = graphicsDevice.Viewport;
        var titleSize = font.MeasureString(Title);
        var position = new Vector2(
            (viewport.Width - titleSize.X) / 2f,
            (viewport.Height - titleSize.Y) / 2f
        );

        spriteBatch.Begin();
        spriteBatch.DrawString(font, Title, position, Color.White);
        spriteBatch.End();
    }
}
