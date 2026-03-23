using Microsoft.Xna.Framework;

namespace WorldBattleNaval.UI;

public class Panel : UIElement
{
    public int Height { get; set; }
    public Color Background { get; set; } = Color.Red;
    public Color? BorderColor { get; set; }
    public int BorderThickness { get; set; } = 1;

    public Panel(int x, int y, int width, int height) : base(x, y, width)
        => Height = height;

    public override int Draw(UIContext ctx)
    {
        ctx.FillRect(X, Y, Width, Height, Background);

        if (BorderColor.HasValue)
        {
            var t = BorderThickness;
            var c = BorderColor.Value;
            ctx.FillRect(X, Y, Width, t, c);
            ctx.FillRect(X, Y + Height - t, Width, t, c);
            ctx.FillRect(X, Y, t, Height, c);
            ctx.FillRect(X + Width - t, Y, t, Height, c);
        }

        return Height;
    }
}