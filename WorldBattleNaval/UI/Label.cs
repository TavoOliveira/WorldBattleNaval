using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WorldBattleNaval.UI;

public class Label : UIElement
{
    public string Text { get; set; }
    public Color Color { get; set; } = Color.White;
    public SpriteFont? Font { get; set; }

    public Label(string text, int x, int y, int width) : base(x, y, width)
        => Text = text;

    public override int Draw(UIContext ctx)
    {
        var font = Font ?? ctx.Font;
        ctx.SpriteBatch.DrawString(font, Text, new Vector2(X + ctx.OffsetX, Y + ctx.OffsetY), Color);
        return (int)font.MeasureString(Text).Y;
    }
}