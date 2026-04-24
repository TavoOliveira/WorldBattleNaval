using System.Reflection.Metadata.Ecma335;
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
        if (!Enabled || !Visible)
        {
            IsHovered = false;
            IsPressed = false;
            IsClicked = false;
            return;
        }

        IsHovered = bounds.Contains(InputManager.MousePosition);
        IsPressed = IsHovered && InputManager.IsLeftDown;
        IsClicked = IsHovered && InputManager.IsLeftClicked;
    }

    public override int Draw(UIContext ctx)
    {
        if (!Visible) return 0;

        bounds = new Rectangle(X + ctx.OffsetX, Y + ctx.OffsetY, Width, Height);

        var color = Enabled ? Color.White : Color.Gray;

        if (texturePressed != null && IsPressed)
            ctx.SpriteBatch.Draw(texturePressed, bounds, color);
        else
            ctx.SpriteBatch.Draw(texture, bounds, color);

        ctx.PushOffset(X, Y);
        child.Width = child.Width == 0 ? Width : child.Width;
        child.Draw(ctx);
        ctx.PopOffset(X, Y);

        return Height;
    }

    public static Button ReturnButton(string text, ResourceManager res, UIContext ctx)
    {
        const int buttonWidth  = 250;
        const int buttonHeight = 50;
        var gold = new Color(220, 160, 20);

        var textSize = ctx.Font.MeasureString(text);
        var labelX = (int)((buttonWidth  - textSize.X) / 2);
        var labelY = (int)((buttonHeight - textSize.Y) / 2);

        var label = new Label(text, labelX, labelY, 0) { Color = gold };
        return new Button(label, res.ButtonTexture, res.ButtonPressedTexture, 0, 0, buttonWidth);
    }
}
