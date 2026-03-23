using Microsoft.Xna.Framework;

namespace WorldBattleNaval.UI;

public class Label : UIElement
{
    public string Text { get; set; }
    public Color Color { get; set; } = Color.White;

    public Label(string text, int x, int y, int width) : base(x, y, width)
        =>  Text = text;

    public override int Draw(UIContext ctx)
    {
        var font = ctx.Font;
        ctx.SpriteBatch.DrawString(font, Text, new Vector2(X, Y), Color);
        return (int)font.MeasureString(Text).Y;
    }
}