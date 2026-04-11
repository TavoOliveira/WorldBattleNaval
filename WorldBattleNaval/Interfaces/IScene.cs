using System;
using Microsoft.Xna.Framework;

namespace WorldBattleNaval.Interfaces;

public interface IScene
{
    bool IsReady { get; }
    void LoadContent(IServiceProvider services);
    void Unload();
    void Update(GameTime gameTime);
    void Draw(GameTime gameTime);
}
