using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using WorldBattleNaval.Interfaces;

namespace WorldBattleNaval;

public class SceneManager
{
    private readonly ContentManager content;

    private IScene currentScene;
    private IScene pendingScene;

    public SceneManager(ContentManager content)
    {
        this.content = content;
    }

    public void ChangeScene(IScene scene)
    {
        pendingScene = scene;
    }

    public void Update(GameTime gameTime)
    {
        if (pendingScene != null)
        {
            if (!pendingScene.IsReady)
                pendingScene.LoadContent(content);

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
