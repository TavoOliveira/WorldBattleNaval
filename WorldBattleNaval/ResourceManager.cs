using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace WorldBattleNaval;

public class ResourceManager
{
    public Texture2D Pixel { get; }
    public SpriteFont Font { get; }
    public SpriteFont SmallFont { get; }
    public SpriteFont TinyFont { get; }
    public Texture2D ButtonTexture { get; }
    public Texture2D ButtonPressedTexture { get; }

    public Model CarrierModel { get; }
    public Model BattleshipModel { get; }
    public Model SubmarineModel { get; }
    public Model CruiserModel { get; }
    public Model DestroyerModel { get; }

    public ResourceManager(ContentManager content, GraphicsDevice graphicsDevice)
    {
        Pixel = new Texture2D(graphicsDevice, 1, 1);
        Pixel.SetData(new[] { Color.White });

        Font = content.Load<SpriteFont>("fonts/Font");
        SmallFont = content.Load<SpriteFont>("fonts/SmallFont");
        TinyFont = content.Load<SpriteFont>("fonts/TinyFont");
        ButtonTexture = content.Load<Texture2D>("images/bg_button");
        ButtonPressedTexture = content.Load<Texture2D>("images/bg_button_pressed");

        CarrierModel = content.Load<Model>("models/aircraft_carrier");
        BattleshipModel = content.Load<Model>("models/battleship");
        SubmarineModel = content.Load<Model>("models/Submarine");
        CruiserModel = content.Load<Model>("models/cruiser");
        DestroyerModel = content.Load<Model>("models/destroyer");
    }
}