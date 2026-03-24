using System.Collections.Generic;
using System.Linq;

namespace WorldBattleNaval.Entities;

public class Player
{
    public Board Board { get; }
    public List<Ship> Ships { get; } = [];
    public bool IsDefeated => Ships.All(s => s.IsSunk);

    public Player()
    {
        Board = new Board();
    }
}