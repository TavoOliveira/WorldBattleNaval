using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WorldBattleNaval.Interfaces;
using WorldBattleNaval.UI;

namespace WorldBattleNaval.Scenes;

public class GameScene : IScene
{
    private readonly GraphicsDevice graphicsDevice;
    private readonly SceneManager sceneManager;

    private Camera camera;

    private GridAttack gridAttack;
    private TweenGroup gridTween;

    public bool IsReady { get; private set; }

    public GameScene(GraphicsDevice graphicsDevice, SceneManager sceneManager)
    {
        this.graphicsDevice = graphicsDevice;
        this.sceneManager = sceneManager;
    }

    public void LoadContent(ContentManager content)
    {
        camera = new Camera();

        var screenWidth = graphicsDevice.Viewport.Width;
        var screenHeight = graphicsDevice.Viewport.Height;

        var size = 600;

        var ssRadar = content.Load<Texture2D>("images/radar_spritesheet");

        gridAttack = new GridAttack(ssRadar, (screenWidth - size) / 2 , (screenHeight - size) / 2, size);

        IsReady = true;
    }

    public void Unload()
    {
        IsReady = false;
    }

    public void Update(GameTime gameTime)
    {
        gridAttack.Update(gameTime);
    }

    public void Draw(GameTime gameTime)
    {
        graphicsDevice.Clear(new Color(10, 31, 68, 255));

        sceneManager.SpriteBatch.Begin();
        gridAttack.Draw(sceneManager.UIContext);
        sceneManager.SpriteBatch.End();
    }

}