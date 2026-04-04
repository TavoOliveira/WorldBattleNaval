using Microsoft.Xna.Framework;

namespace WorldBattleNaval.UI;

public abstract class UIElement(int x, int y, int width)
{
    public int X { get; set; } = x;
    public int Y { get; set; } = y;
    public int Width { get; set; } = width;
    
    public abstract int Draw(UIContext ctx);
}