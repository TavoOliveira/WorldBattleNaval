using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WorldBattleNaval.UI;

public class GridAttack : UIElement
{
    private Panel panel;
    private CrosshairGrid crosshairGrid;
    private AnimatedSprite animatedSprite;
    
    public int Height { get; set;  }
    
    public GridAttack(Texture2D texture, int x, int y, int size) : base(x, y, size)
    {
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
    }
    
    public override (int Width, int Height) Measure(UIContext ctx) => (Width, Height);

    public void Update(GameTime gameTime)
    {
        crosshairGrid.Update(gameTime);
        animatedSprite.Update(gameTime);
    }

    public override int Draw(UIContext ctx)
    {
        return panel.Draw(ctx);
    }
}