using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WorldBattleNaval.Enums;

namespace WorldBattleNaval.Entities;

public class Board
{
    public const int Size = 10;
    public const float CellSize = 2f;

    private readonly Cell[,] cells = new Cell[Size, Size];
    private int cursorRow;
    private int cursorCol;

    private VertexPositionColor[] gridLines;
    private BasicEffect effect;

    public (int row, int col) CursorPosition => (cursorRow, cursorCol);

    public Board()
    {
        for (var row = 0; row < Size; row++)
            for (var col = 0; col < Size; col++)
                cells[row, col] = new Cell(row, col);

        BuildGridLines();
    }

    public void InitEffect(GraphicsDevice graphicsDevice)
    {
        effect = new BasicEffect(graphicsDevice)
        {
            VertexColorEnabled = true,
            LightingEnabled = false
        };
    }

    public void Draw(GraphicsDevice graphicsDevice, Matrix view, Matrix projection)
    {
        if (effect == null) InitEffect(graphicsDevice);

        graphicsDevice.BlendState = BlendState.AlphaBlend;
        graphicsDevice.DepthStencilState = DepthStencilState.Default;
        graphicsDevice.RasterizerState = RasterizerState.CullNone;

        effect.View = view;
        effect.Projection = projection;
        effect.World = Matrix.Identity;

        DrawGridLines(graphicsDevice, effect);

        graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
    }

    public void MoveCursor(int row, int col)
    {
        cursorRow = Math.Clamp(cursorRow + row, 0, Size - 1);
        cursorCol = Math.Clamp(cursorCol + col, 0, Size - 1);
    }

    public void SetCursor(int row, int col)
    {
        cursorRow = Math.Clamp(row, 0, Size - 1);
        cursorCol = Math.Clamp(col, 0, Size - 1);
    }
    
    public bool CanPlace(int row, int col, int shipSize, bool horizontal)
    {
        for (int i = 0; i < shipSize; i++)
        {
            int r = horizontal ? row : row + i;
            int c = horizontal ? col + i : col;
            if (r < 0 || r >= Size || c < 0 || c >= Size) return false;
            if (cells[r, c].State == ECellState.OCCUPIED) return false;
        }
        return true;
    }

    public void Place(int row, int col, int shipSize, bool horizontal)
    {
        for (int i = 0; i < shipSize; i++)
        {
            int r = horizontal ? row : row + i;
            int c = horizontal ? col + i : col;
            cells[r, c].State = ECellState.OCCUPIED;
        }
    }

    public void Clear(int row, int col, int shipSize, bool horizontal)
    {
        for (int i = 0; i < shipSize; i++)
        {
            int r = horizontal ? row : row + i;
            int c = horizontal ? col + i : col;
            cells[r, c].State = ECellState.EMPTY;
        }
    }

    public static Board CreateRandom(IReadOnlyList<Ship> ships, Random? rng = null)
    {
        rng ??= Random.Shared;
        var board = new Board();

        foreach (var ship in ships)
        {
            bool placed = false;
            while (!placed)
            {
                bool horizontal = rng.Next(2) == 0;

                if (ship.IsHorizontal != horizontal)
                    ship.Rotate();

                int maxRow = horizontal ? Size : Size - ship.Size;
                int maxCol = horizontal ? Size - ship.Size : Size;

                int row = rng.Next(0, maxRow);
                int col = rng.Next(0, maxCol);

                if (board.CanPlace(row, col, ship.Size, horizontal))
                {
                    board.Place(row, col, ship.Size, horizontal);
                    ship.Place(row, col);
                    placed = true;
                }
            }
        }

        return board;
    }

    private void BuildGridLines()
    {
        var lines = new List<VertexPositionColor>();
        var half = Size * CellSize / 2f;
        var color = new Color(200, 230, 255, 200);
        var height = 0.18f;

        for (var i = 0; i <= Size; i++)
        {
            var currentLine = -half + i * CellSize;
            lines.Add(new VertexPositionColor(new Vector3(-half, height, currentLine), color));
            lines.Add(new VertexPositionColor(new Vector3(half, height, currentLine), color));
            lines.Add(new VertexPositionColor(new Vector3(currentLine, height, -half), color));
            lines.Add(new VertexPositionColor(new Vector3(currentLine, height, half), color));
        }

        gridLines = [.. lines];
    }

    private void DrawGridLines(GraphicsDevice graphicsDevice, BasicEffect effect)
    {
        effect.CurrentTechnique.Passes[0].Apply();
        graphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, gridLines, 0, gridLines.Length / 2);
    }
}