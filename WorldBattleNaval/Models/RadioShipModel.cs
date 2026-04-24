using Microsoft.Xna.Framework.Graphics;
using WorldBattleNaval.Entities;

namespace WorldBattleNaval.Models;

public class RadioShipModel
{
    public bool IsSelected { get; set; }
    public Ship Ship { get; set; }
    public Texture2D Screenshot { get; set; }
}
