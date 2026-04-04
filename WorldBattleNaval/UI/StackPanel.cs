using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;

namespace WorldBattleNaval.UI;

public class StackPanel : UIElementWithChildren
{
    public bool IsVertical { get; set; } = true;
    public int Spacing { get; set; } = 4;

    public StackPanel(int x, int y, int width) : base(x, y, width) { }

    public override int Draw(UIContext ctx)
    {
        var position = 0;
        var ox = ctx.OffsetX;
        var oy = ctx.OffsetY;

        foreach (var child in children)
        {
            if (IsVertical)
            {
                child.X = X + ox;
                child.Y = Y + oy + position;
                child.Width = child.Width == 0 ? Width : child.Width;
                
                ctx.PopOffset(ox, oy);
                position += child.Draw(ctx) + Spacing;
                ctx.PushOffset(ox, oy);
            }
            else
            {
                child.X = X + ox + position;
                child.Y = Y + oy;
                
                ctx.PopOffset(ox, oy);
                position += child.Draw(ctx) + Spacing;
                ctx.PushOffset(ox, oy);
            }
        }
        
        return position > 0 ? position - Spacing : 0;
    }
}