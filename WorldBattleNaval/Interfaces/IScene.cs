using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace WorldBattleNaval.Interfaces;

public interface IScene
{
    bool IsReady { get; }
    void LoadContent(ContentManager content);
    void Unload();
    void Update(GameTime gameTime);
    void Draw(GameTime gameTime);
}
