using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WorldBattleNaval.UI;

public class Image : UIElement
{
    private readonly Texture2D texture;
    
    public int Height { get; private set; }
    
    public Image(Texture2D texture, int x, int y, int? width = null, int? height = null) : base(x, y, width ?? texture.Width)
    {
        this.texture = texture;
        Height = height ?? texture.Height;
    }

    public override int Draw(UIContext ctx)
    {
        ctx.SpriteBatch.Draw(texture, new Rectangle(X + ctx.OffsetX, Y + ctx.OffsetY, Width, Height), Color.White);
        return Height;
    }
}