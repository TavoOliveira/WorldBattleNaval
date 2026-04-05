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

        foreach (var child in children)
        {
            if (child.Width == 0) child.Width = Width;

            if (IsVertical)
            {
                ctx.PushOffset(X, Y + position);
                int drawn = child.Draw(ctx);
                ctx.PopOffset(X, Y + position);
                position += drawn + Spacing;
            }
            else
            {
                ctx.PushOffset(X + position, Y);
                int drawn = child.Draw(ctx);
                ctx.PopOffset(X + position, Y);
                position += drawn + Spacing;
            }
        }

        return position > 0 ? position - Spacing : 0;
    }
}