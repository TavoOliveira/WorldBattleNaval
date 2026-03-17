using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using WorldBattleNaval.Entities;
using WorldBattleNaval.Interfaces;

namespace WorldBattleNaval.Scenes;

public class GameScene : IScene
{
    private readonly GraphicsDevice graphicsDevice;
    private readonly SceneManager sceneManager;

    private Camera camera;
    private Board board;
    private Ship ship;

    public bool IsReady { get; private set; }

    public GameScene(GraphicsDevice graphicsDevice, SceneManager sceneManager)
    {
        this.graphicsDevice = graphicsDevice;
        this.sceneManager = sceneManager;
    }

    public void LoadContent(ContentManager content)
    {
        var model = content.Load<Model>("models/Submarino");

        camera = new Camera();
        board = new Board();
        ship = new Ship(model, 3);

        IsReady = true;
    }

    public void Unload()
    {
        IsReady = false;
    }

    public void Update(GameTime gameTime)
    {
    }

    public void Draw(GameTime gameTime)
    {
        var view = camera.View;
        var projection = camera.Projection;

        board.Draw(graphicsDevice, view, projection);
        ship.Draw(graphicsDevice, 3, 4, view, projection);
    }
}
