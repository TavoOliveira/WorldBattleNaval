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

    public (bool hit, bool sunk) ReceiveAttack(int row, int col)
    {
        var ship = Ships.FirstOrDefault(s => s.Covers(row, col));
        if (ship != null)
        {
            ship.TakeDamage();
            Board.MarkHit(row, col);
            return (true, ship.IsSunk);
        }

        Board.MarkMiss(row, col);
        return (false, false);
    }
}
