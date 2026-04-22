using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WorldBattleNaval.Interfaces;
using WorldBattleNaval.UI;

namespace WorldBattleNaval;

public class SceneManager
{
    private readonly IServiceProvider services;
    private readonly Game game;

    private IScene currentScene;
    private IScene pendingScene;

    public SpriteBatch SpriteBatch { get; }
    public GameState GameState { get; }
    public ResourceManager Resources { get; }
    public UIContext UIContext { get; }

    public SceneManager(Game game, IServiceProvider services, SpriteBatch spriteBatch, GameState gameState,
        ResourceManager resources, UIContext uiContext)
    {
        this.game = game;
        this.services = services;
        SpriteBatch = spriteBatch;
        GameState = gameState;
        Resources = resources;
        UIContext = uiContext;
    }

    public void ChangeScene(IScene scene)
    {
        pendingScene = scene;
    }

    public void QuitGame()
    {
        game.Exit();
    }

    public void Update(GameTime gameTime)
    {
        if (pendingScene != null)
        {
            if (!pendingScene.IsReady)
                pendingScene.LoadContent(services);

            if (pendingScene.IsReady)
            {
                currentScene?.Unload();
                currentScene = pendingScene;
                pendingScene = null;
            }
        }

        currentScene?.Update(gameTime);
    }

    public void Draw(GameTime gameTime) => currentScene?.Draw(gameTime);
}
