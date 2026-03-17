using WorldBattleNaval.Enums;

namespace WorldBattleNaval;

public class Cell
{
    public int Row { get; }
    public int Col { get; }
    public CellState State { get; set; }

    public Cell(int row, int col)
    {
        Row = row;
        Col = col;
        State = CellState.EMPTY;
    }
}
