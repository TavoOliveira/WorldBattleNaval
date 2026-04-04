using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace WorldBattleNaval;

public class ResourceManager
{
    public Texture2D Pixel { get; }
    public SpriteFont Font { get; }
    public Texture2D ButtonTexture { get; }
    public Texture2D ButtonPressedTexture { get; }
    public Texture2D LogoTexture { get; }
    public Model SubmarineModel { get; }

    public ResourceManager(ContentManager content, GraphicsDevice graphicsDevice)
    {
        Pixel = new Texture2D(graphicsDevice, 1, 1);
        Pixel.SetData(new[] { Color.White });

        Font = content.Load<SpriteFont>("fonts/Font");
        ButtonTexture = content.Load<Texture2D>("images/bg_button");
        ButtonPressedTexture = content.Load<Texture2D>("images/bg_button_pressed");
        LogoTexture = content.Load<Texture2D>("images/logo");
        SubmarineModel = content.Load<Model>("models/Submarine");
    }
}