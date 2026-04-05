using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WorldBattleNaval.UI;

public class Button : UIElement
{
    private readonly Label label;
    private readonly Texture2D texture;
    private readonly Texture2D? texturePressed;

    public int Height { get; set; } = 50;

    public bool IsHovered { get; private set; }
    public bool IsPressed { get; private set; }
    public bool IsClicked { get; private set; }

    private Rectangle bounds;

    public Button(Label label, Texture2D texture, Texture2D? texturePressed, int x, int y, int width) : base(x, y, width)
    {
        this.label = label;
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

        var textSize = (label.Font ?? ctx.Font).MeasureString(label.Text);
        label.X = X + (Width  - (int)textSize.X) / 2;
        label.Y = Y + (Height - (int)textSize.Y) / 2;
        label.Draw(ctx);

        return Height;
    }
}
