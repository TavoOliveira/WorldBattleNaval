using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using WorldBattleNaval.Interfaces;
using WorldBattleNaval.UI;

namespace WorldBattleNaval.Scenes;

public class GameResultScene : IScene
{
    private readonly GraphicsDevice graphicsDevice;
    private readonly SceneManager sceneManager;
    private readonly bool playerWon;

    private ContentManager sceneContent;
    private Texture2D resultTexture;
    private Button mainMenuBtn;

    public bool IsReady { get; private set; }

    public GameResultScene(GraphicsDevice graphicsDevice, SceneManager sceneManager, bool playerWon)
    {
        this.graphicsDevice = graphicsDevice;
        this.sceneManager = sceneManager;
        this.playerWon = playerWon;
    }

    public void LoadContent(IServiceProvider services)
    {
        sceneContent = new ContentManager(services) { RootDirectory = "Content" };
        var res = sceneManager.Resources;

        var texturePath = playerWon ? "images/vitoria" : "images/perdeu";
        resultTexture = sceneContent.Load<Texture2D>(texturePath);

        mainMenuBtn = Button.ReturnButton("MENU PRINCIPAL", sceneManager.Resources, sceneManager.UIContext);
        mainMenuBtn.Width = 250;
        mainMenuBtn.X = graphicsDevice.Viewport.Width / 2 - mainMenuBtn.Width / 2;
        mainMenuBtn.Y = graphicsDevice.Viewport.Height - 150;

        IsReady = true;
    }

    public void Unload()
    {
        sceneContent?.Unload();
        sceneContent?.Dispose();
        sceneContent = null;
        resultTexture = null;
        mainMenuBtn = null;
        IsReady = false;
    }

    public void Update(GameTime gameTime)
    {
        mainMenuBtn.Update();

        if (mainMenuBtn.IsClicked)
        {
            sceneManager.ChangeScene(new MainMenuScene(graphicsDevice, sceneManager));
        }
    }

    public void Draw(GameTime gameTime)
    {
        graphicsDevice.Clear(new Color(10, 31, 68, 255));

        var uiCtx = sceneManager.UIContext;
        sceneManager.SpriteBatch.Begin();

        var imgX = graphicsDevice.Viewport.Width / 2 - resultTexture.Width / 2;
        var imgY = graphicsDevice.Viewport.Height / 2 - resultTexture.Height / 2 - 50;
        uiCtx.SpriteBatch.Draw(resultTexture, new Vector2(imgX, imgY), Color.White);

        mainMenuBtn.Draw(uiCtx);

        sceneManager.SpriteBatch.End();
    }
}
