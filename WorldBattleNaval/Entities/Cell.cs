using WorldBattleNaval.Enums;

namespace WorldBattleNaval.Entities;

public class Cell
{
    public int Row { get; }
    public int Col { get; }
    public ECellState State { get; set; }

    public Cell(int row, int col)
    {
        Row = row;
        Col = col;
        State = ECellState.EMPTY;
    }
}
