using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WorldBattleNaval.UI;

public class AnimatedSprite : UIElement
{
    private readonly Texture2D texture;
    private readonly int frameWidth;
    private readonly int frameHeight;
    private readonly int columns;
    private readonly int totalFrames;
    private readonly float frameDuration;

    private float elapsed;
    private int currentFrame;

    public int Height { get; set; }
    public bool IsLooping { get; set; } = true;
    public bool IsPlaying { get; private set; } = true;
    public float Opacity { get; set; } = 1f;

    public AnimatedSprite(
        Texture2D texture,
        int frameWidth,
        int frameHeight,
        int frameCount,
        float frameDuration,
        int x, int y,
        int? displayWidth = null,
        int? displayHeight = null)
        : base(x, y, displayWidth ?? frameWidth)
    {
        this.texture = texture;
        this.frameWidth = frameWidth;
        this.frameHeight = frameHeight;
        this.frameDuration = frameDuration;
        totalFrames = frameCount;
        columns = texture.Width / frameWidth;
        Height = displayHeight ?? frameHeight;
    }

    public void Update(GameTime gameTime)
    {
        if (!IsPlaying) return;

        elapsed += (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (elapsed >= frameDuration)
        {
            elapsed -= frameDuration;
            currentFrame++;

            if (currentFrame >= totalFrames)
                currentFrame = IsLooping ? 0 : totalFrames - 1;
        }
    }

    public void Play() { IsPlaying = true; }
    public void Pause() { IsPlaying = false; }

    public void Reset()
    {
        currentFrame = 0;
        elapsed = 0f;
        IsPlaying = true;
    }

    public override (int Width, int Height) Measure(UIContext ctx) => (Width, Height);

    public override int Draw(UIContext ctx)
    {
        int col = currentFrame % columns;
        int row = currentFrame / columns;

        var source = new Rectangle(col * frameWidth, row * frameHeight, frameWidth, frameHeight);
        var dest = new Rectangle(X + ctx.OffsetX, Y + ctx.OffsetY, Width, Height);

        ctx.SpriteBatch.Draw(texture, dest, source, Color.White * Opacity);

        return Height;
    }
}
