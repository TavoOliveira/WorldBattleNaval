using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace WorldBattleNaval.UI;

public class Button : UIElement
{
    public string Text { get; set; }
    public int Height { get; set; } = 32;

    public bool IsHovered { get; private set; }
    public bool IsPressed { get; private set; }
    public bool IsClicked { get; private set; }

    private ButtonState previewLeft = ButtonState.Released;

    private readonly Color BackgroundColor;
    private readonly Color BackgroundColorHover;
    private readonly Color BackgroundColorPressed;


    public Button(string text, int x, int y, int width) : base(x, y, width)
        => Text = text;

    public void Update(MouseState mouse)
    {
        var bounds = new Rectangle(X, Y, Width, Height);
        IsHovered = bounds.Contains(mouse.X, mouse.Y);
        IsPressed = IsHovered && mouse.LeftButton == ButtonState.Pressed;
        IsClicked = IsHovered && previewLeft == ButtonState.Pressed && mouse.LeftButton == ButtonState.Released;

        previewLeft = mouse.LeftButton;
    }

    public override int Draw(UIContext ctx)
    {
        var bg = IsPressed ? BackgroundColorPressed : IsHovered ? BackgroundColorHover : BackgroundColor;

        ctx.FillRect(X, Y, Width, Height, bg);

        var size = ctx.Font.MeasureString(Text);
        var textX = X + (Width - (int)size.X) / 2;
        var textY = Y + (Height - (int)size.Y) / 2;

        ctx.SpriteBatch.DrawString(ctx.Font, Text, new Vector2(textX, textY), Color.White);

        return Height;
    }
}