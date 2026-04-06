using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WorldBattleNaval.UI;

public class Image : UIElement
{
    private readonly Texture2D texture;
    
    public int Height { get; private set; }
    
    public Image(Texture2D texture, int x, int y, int? width = null, int? height = null) 
        : base(x, y, width ?? (height.HasValue ? (int)(height.Value * (float)texture.Width / texture.Height) : texture.Width))
    {
        this.texture = texture;
        
        if (height.HasValue)
            Height = height.Value;
        else if (width.HasValue)
            Height = (int)(width.Value * (float)texture.Height / texture.Width);
        else
            Height = texture.Height;
    }

    public override int Draw(UIContext ctx)
    {
        ctx.SpriteBatch.Draw(texture, new Rectangle(X + ctx.OffsetX, Y + ctx.OffsetY, Width, Height), Color.White);
        return Height;
    }
}