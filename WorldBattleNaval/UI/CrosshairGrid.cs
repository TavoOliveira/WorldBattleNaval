using Microsoft.Xna.Framework;

namespace WorldBattleNaval.UI;

public class CrosshairGrid : UIElement
{
    private const int Cells = 10;

    private int hoverCol = -1, hoverRow = -1;
    private float blinkTimer;
    private int screenX, screenY;

    public int Height { get; set; }
    public float Opacity { get; set; } = 1f;
    public int LockedCol { get; private set; } = -1;
    public int LockedRow { get; private set; } = -1;
    public bool IsLocked => LockedCol >= 0;

    public CrosshairGrid(int x, int y, int size) : base(x, y, size)
    {
        Height = size;
    }

    public void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        blinkTimer += dt;

        UpdateHover();

        if (InputManager.IsLeftPressed && hoverCol >= 0 && hoverRow >= 0)
        {
            LockedCol = hoverCol;
            LockedRow = hoverRow;
        }
    }

    public void Unlock()
    {
        LockedCol = -1;
        LockedRow = -1;
    }

    public override (int Width, int Height) Measure(UIContext ctx) => (Width, Height);

    public override int Draw(UIContext ctx)
    {
        if (Width <= 0) return Height;

        int gx = screenX = X + ctx.OffsetX;
        int gy = screenY = Y + ctx.OffsetY;
        float cellSize = Width / (float)Cells;
        var lineColor = new Color(0, 255, 0) * Opacity;

        int crossCol = IsLocked ? LockedCol : hoverCol;
        int crossRow = IsLocked ? LockedRow : hoverRow;

        if (crossCol >= 0 && crossRow >= 0)
        {
            var hoverColor = new Color(0, 255, 0, 60) * Opacity;
            int hx = gx + (int)(crossCol * cellSize);
            int hy = gy + (int)(crossRow * cellSize);
            int cs = (int)cellSize;
            ctx.SpriteBatch.Draw(ctx.Texture,
                new Rectangle(hx, hy, cs, cs), hoverColor);

            var crossColor = IsLocked && (int)(blinkTimer * 4) % 2 == 1
                ? Color.Yellow * Opacity
                : Color.Red * Opacity;
            int centerX = hx + cs / 2;
            int centerY = hy + cs / 2;
            int crossThick = 2;

            ctx.SpriteBatch.Draw(ctx.Texture,
                new Rectangle(centerX - crossThick / 2, gy, crossThick, Width), crossColor);
            ctx.SpriteBatch.Draw(ctx.Texture,
                new Rectangle(gx, centerY - crossThick / 2, Width, crossThick), crossColor);
        }

        for (int i = 0; i <= Cells; i++)
        {
            int lx = gx + (int)(i * cellSize);
            ctx.SpriteBatch.Draw(ctx.Texture,
                new Rectangle(lx, gy, 1, Width), lineColor);
        }

        for (int i = 0; i <= Cells; i++)
        {
            int ly = gy + (int)(i * cellSize);
            ctx.SpriteBatch.Draw(ctx.Texture,
                new Rectangle(gx, ly, Width, 1), lineColor);
        }

        return Height;
    }

    private void UpdateHover()
    {
        hoverCol = -1;
        hoverRow = -1;

        if (Width <= 0) return;

        var pos = InputManager.MousePosition;
        int mx = pos.X - screenX;
        int my = pos.Y - screenY;

        if (mx < 0 || mx >= Width || my < 0 || my >= Width) return;

        float cellSize = Width / (float)Cells;
        hoverCol = (int)(mx / cellSize);
        hoverRow = (int)(my / cellSize);
    }
}
