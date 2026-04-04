using System.Collections.Generic;

namespace WorldBattleNaval.UI;

public abstract class UIElementWithChildren : UIElement
{
    protected readonly List<UIElement> children = [];
    
    public UIElementWithChildren(int x, int y, int width) : base(x, y, width) {}

    public void AddChild(UIElement child) =>
        children.Add(child);

    public void RemoveChild(UIElement child) =>
        children.Remove(child);
}