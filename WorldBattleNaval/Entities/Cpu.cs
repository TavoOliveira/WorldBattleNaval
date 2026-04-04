using System.Collections.Generic;
using System.Linq;

namespace WorldBattleNaval.Entities;

public class Cpu
{
    public Board Board { get; private set; }
    public List<Ship> Ships { get; } = [];
    public bool IsDefeated => Ships.All(s => s.IsSunk);

    public void Setup(List<Ship> ships)
    {
        Ships.AddRange(ships);
        Board = Board.CreateRandom(ships);
    }
}
