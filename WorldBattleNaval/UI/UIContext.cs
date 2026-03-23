using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WorldBattleNaval.UI;

public class UIContext
{
    public readonly SpriteBatch SpriteBatch;
    public readonly Texture2D Texture;
    public readonly SpriteFont Font;

    public UIContext(SpriteBatch spriteBatch, Texture2D texture, SpriteFont font)
    {
        SpriteBatch = spriteBatch;
        Texture = texture;
        Font = font;
    }

    public void FillRect(int x, int y, int width, int height, Color color)
        => SpriteBatch.Draw(Texture, new Rectangle(x, y, width, height), color);
}