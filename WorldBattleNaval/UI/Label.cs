using System.Text;
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

    public override (int Width, int Height) Measure(UIContext ctx)
    {
        var size = (Font ?? ctx.Font).MeasureString(Text);
        return ((int)size.X, (int)size.Y);
    }

    public override int Draw(UIContext ctx)
    {
        if (!Visible) return 0;

        var font = Font ?? ctx.Font;
        var wrapped = WrapText(font, Text, Width);

        ctx.SpriteBatch.DrawString(
            font, 
            wrapped, 
            new Vector2(X + ctx.OffsetX, Y + ctx.OffsetY),
            Color
        );

        return (int)font.MeasureString(Text).Y;
    }

    private string WrapText(SpriteFont font, string text, int width)
    {
        if (width <= 0) return text;

        var result = new StringBuilder();
        var words = text.Split(' ');
        var line = "";

        foreach (var word in words)
        {
            var testLine = line.Length == 0 ? word : line + " " + word;
            var size = font.MeasureString(testLine);

            if (size.X > width)
            {
                result.Append(line + "\n");
                line = word;
            }
            else
            {
                line = testLine;
            }
        }

        result.Append(line);
        return result.ToString();
    }
}
