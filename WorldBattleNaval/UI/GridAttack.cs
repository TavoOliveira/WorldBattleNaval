using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WorldBattleNaval.UI;

public class GridAttack : UIElement
{
    public const int Cells = 10;

    private readonly Panel panel;
    private readonly CrosshairGrid crosshairGrid;
    private readonly AnimatedSprite animatedSprite;
    private readonly TweenGroup scaleTween;

    private readonly int baseSize;
    private readonly int baseX;
    private readonly int baseY;

    private readonly List<(int row, int col, Texture2D tex)> markers = [];
    private readonly bool[,] hasMarker = new bool[Cells, Cells];

    private float scale;

    public int Height { get; set; }

    public bool IsLocked => crosshairGrid.IsLocked;
    public int LockedRow => crosshairGrid.LockedRow;
    public int LockedCol => crosshairGrid.LockedCol;
    public bool IsAnimating => scaleTween.IsPlaying;
    public bool IsShown => !scaleTween.IsPlaying && scale > 0.99f;

    public GridAttack(Texture2D texture, int x, int y, int size) : base(x, y, size)
    {
        baseSize = size;
        baseX = x;
        baseY = y;
        Height = size;

        panel = new Panel(x, y, size, size);
        crosshairGrid = new CrosshairGrid(0, 0, size);
        animatedSprite = new AnimatedSprite(texture,
            256, 256,
            60, 0.05f,
            0, 0,
            size, size);
        animatedSprite.IsLooping = true;

        panel.AddChild(animatedSprite);
        panel.AddChild(crosshairGrid);

        scaleTween = new TweenGroup(0.4f).Add(SetScale, 0f, 1f);
        SetScale(0f);
    }

    public override (int Width, int Height) Measure(UIContext ctx) => (Width, Height);

    public void Show() => scaleTween.PlayForward();
    public void Hide() => scaleTween.PlayReverse();
    public void Unlock() => crosshairGrid.Unlock();

    public bool HasMarker(int row, int col) => hasMarker[row, col];

    public void AddMarker(int row, int col, Texture2D tex)
    {
        markers.Add((row, col, tex));
        hasMarker[row, col] = true;
    }

    public void Update(GameTime gameTime)
    {
        scaleTween.Update(gameTime);
        animatedSprite.Update(gameTime);
        if (IsShown) crosshairGrid.Update(gameTime);
    }

    public override int Draw(UIContext ctx)
    {
        if (Width <= 0) return 0;
        int h = panel.Draw(ctx);
        DrawMarkers(ctx);
        return h;
    }

    private void SetScale(float value)
    {
        scale = MathHelper.Clamp(value, 0f, 1f);
        int size = (int)(baseSize * scale);
        int offset = (baseSize - size) / 2;

        X = baseX + offset;
        Y = baseY + offset;
        Width = size;
        Height = size;

        panel.X = X;
        panel.Y = Y;
        panel.Width = size;
        panel.Height = size;

        crosshairGrid.Width = size;
        crosshairGrid.Height = size;

        animatedSprite.Width = size;
        animatedSprite.Height = size;
    }

    private void DrawMarkers(UIContext ctx)
    {
        if (markers.Count == 0) return;

        float cellSize = Width / (float)Cells;
        int gx = X + ctx.OffsetX;
        int gy = Y + ctx.OffsetY;
        int cs = (int)cellSize;

        foreach (var (row, col, tex) in markers)
        {
            int mx = gx + (int)(col * cellSize);
            int my = gy + (int)(row * cellSize);
            ctx.SpriteBatch.Draw(tex, new Rectangle(mx, my, cs, cs), Color.White);
        }
    }
}
