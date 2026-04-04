using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using WorldBattleNaval.Interfaces;

namespace WorldBattleNaval.Scenes;

public class GameScene : IScene
{
    private readonly GraphicsDevice graphicsDevice;
    private readonly SceneManager sceneManager;

    private Camera camera;

    public bool IsReady { get; private set; }

    public GameScene(GraphicsDevice graphicsDevice, SceneManager sceneManager)
    {
        this.graphicsDevice = graphicsDevice;
        this.sceneManager   = sceneManager;
    }

    public void LoadContent(ContentManager content)
    {
        camera = new Camera();
        IsReady = true;
    }

    public void Unload()
    {
        IsReady = false;
    }

    public void Update(GameTime gameTime) { }

    public void Draw(GameTime gameTime)
    {
        var state      = sceneManager.GameState;
        var view       = camera.View;
        var projection = camera.Projection;

        state.Player.Board.Draw(graphicsDevice, view, projection);
        state.Cpu.Board.Draw(graphicsDevice, view, projection);

        foreach (var ship in state.Player.Ships)
            ship.Draw(graphicsDevice, ship.PlacedRow, ship.PlacedCol, view, projection);

        foreach (var ship in state.Cpu.Ships)
            ship.Draw(graphicsDevice, ship.PlacedRow, ship.PlacedCol, view, projection);
    }


}
