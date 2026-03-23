using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WorldBattleNaval.UI;

public class StackPanel : UIElement
{
    public bool IsVertical { get; set; } = true;
    public int Spacing { get; set; } = 4;

    private readonly List<UIElement> children = [];

    public StackPanel(int x, int y, int width) : base(x, y, width)
    {

    }

    public StackPanel Add(UIElement child)
    {
        children.Add(child);
        return this;
    }

    public override int Draw(UIContext ctx)
    {
        var position = 0;

        foreach (var child in children)
        {
            if (IsVertical)
            {
                child.X = X;
                child.Y = Y + position;
                child.Width = Width;
                position += child.Draw(ctx) + Spacing;
            }
            else
            {
                child.X = X + position;
                child.Y = Y;
                position += child.Draw(ctx) + Spacing;
            }
        }

        return position > 0 ? position - Spacing : 0;
    }
}