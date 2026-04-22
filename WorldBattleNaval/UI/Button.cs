using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WorldBattleNaval.UI;

public class Button : UIElement
{
    private readonly UIElement child;
    private readonly Texture2D texture;
    private readonly Texture2D? texturePressed;

    public int Height { get; set; } = 50;

    public bool IsHovered { get; private set; }
    public bool IsPressed { get; private set; }
    public bool IsClicked { get; private set; }

    private Rectangle bounds;

    public Button(UIElement child, Texture2D texture, Texture2D? texturePressed, int x, int y, int width) : base(x, y, width)
    {
        this.child = child;
        this.texture = texture;
        this.texturePressed = texturePressed;
    }

    public void Update()
    {
        IsHovered = bounds.Contains(InputManager.MousePosition);
        IsPressed = IsHovered && InputManager.IsLeftDown;
        IsClicked = IsHovered && InputManager.IsLeftClicked;
    }

    public override int Draw(UIContext ctx)
    {
        bounds = new Rectangle(X + ctx.OffsetX, Y + ctx.OffsetY, Width, Height);

        if (texturePressed != null && IsPressed)
            ctx.SpriteBatch.Draw(texturePressed, bounds, Color.White);
        else
            ctx.SpriteBatch.Draw(texture, bounds, Color.White);

        ctx.PushOffset(X, Y);
        child.Width = child.Width == 0 ? Width : child.Width;
        child.Draw(ctx);
        ctx.PopOffset(X, Y);

        return Height;
    }
}
