using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WorldBattleNaval;

public class Board
{
    public const int Size = 10;
    public const float CellSize = 2f;

    private readonly Cell[,] _cells = new Cell[Size, Size];
    private int _cursorRow;
    private int _cursorCol;

    private readonly BasicEffect _effect;
    private VertexPositionColor[] _gridLines = null!;

    public (int row, int col) CursorPosition => (_cursorRow, _cursorCol);

    public Board(GraphicsDevice graphicsDevice)
    {
        for (int r = 0; r < Size; r++)
            for (int c = 0; c < Size; c++)
                _cells[r, c] = new Cell(r, c);

        _effect = new BasicEffect(graphicsDevice)
        {
            VertexColorEnabled = true,
            LightingEnabled = false,
        };

        BuildGridLines();
    }

    private void BuildGridLines()
    {
        var lines = new List<VertexPositionColor>();
        float half = Size * CellSize / 2f;
        var color = new Color(200, 230, 255, 200);

        for (int i = 0; i <= Size; i++)
        {
            float t = -half + i * CellSize;
            // linhas horizontais (ao longo de X)
            lines.Add(new VertexPositionColor(new Vector3(-half, 0.03f, t), color));
            lines.Add(new VertexPositionColor(new Vector3(half,  0.03f, t), color));
            // linhas verticais (ao longo de Z)
            lines.Add(new VertexPositionColor(new Vector3(t, 0.03f, -half), color));
            lines.Add(new VertexPositionColor(new Vector3(t, 0.03f,  half), color));
        }

        _gridLines = lines.ToArray();
    }

    public void MoveCursor(int dRow, int dCol)
    {
        _cursorRow = Math.Clamp(_cursorRow + dRow, 0, Size - 1);
        _cursorCol = Math.Clamp(_cursorCol + dCol, 0, Size - 1);
    }

    public void SetCursor(int row, int col)
    {
        _cursorRow = Math.Clamp(row, 0, Size - 1);
        _cursorCol = Math.Clamp(col, 0, Size - 1);
    }

    public Cell GetCell(int row, int col) => _cells[row, col];

    public bool CanPlace(int row, int col, int shipSize, bool horizontal)
    {
        for (int i = 0; i < shipSize; i++)
        {
            int r = horizontal ? row : row + i;
            int c = horizontal ? col + i : col;
            if (r < 0 || r >= Size || c < 0 || c >= Size) return false;
            if (_cells[r, c].State == CellState.Occupied) return false;
        }
        return true;
    }

    public void Place(int row, int col, int shipSize, bool horizontal)
    {
        for (int i = 0; i < shipSize; i++)
        {
            int r = horizontal ? row : row + i;
            int c = horizontal ? col + i : col;
            _cells[r, c].State = CellState.Occupied;
        }
    }

    public void Clear(int row, int col, int shipSize, bool horizontal)
    {
        for (int i = 0; i < shipSize; i++)
        {
            int r = horizontal ? row : row + i;
            int c = horizontal ? col + i : col;
            _cells[r, c].State = CellState.Empty;
        }
    }

    public void Draw(GraphicsDevice gd, Submarine submarine, Matrix view, Matrix projection)
    {
        bool canPlace = !submarine.IsPlaced &&
                        CanPlace(_cursorRow, _cursorCol, submarine.Size, submarine.IsHorizontal);

        gd.BlendState = BlendState.AlphaBlend;
        gd.DepthStencilState = DepthStencilState.Default;
        gd.RasterizerState = RasterizerState.CullNone;

        _effect.View = view;
        _effect.Projection = projection;
        _effect.World = Matrix.Identity;

        DrawCells(gd, submarine, canPlace);
        DrawGridLines(gd);

        gd.RasterizerState = RasterizerState.CullCounterClockwise;
    }

    private void DrawCells(GraphicsDevice gd, Submarine submarine, bool canPlace)
    {
        for (int r = 0; r < Size; r++)
        {
            for (int c = 0; c < Size; c++)
            {
                Color color;

                if (_cells[r, c].State == CellState.Occupied)
                {
                    color = new Color(20, 60, 140, 200);
                }
                else
                {
                    color = new Color(20, 100, 180, 130);
                }

                // Sobrescreve com cor de preview se necessário
                if (!submarine.IsPlaced && IsInPreview(r, c, _cursorRow, _cursorCol, submarine.Size, submarine.IsHorizontal))
                {
                    color = canPlace
                        ? new Color(0, 200, 80, 190)
                        : new Color(220, 40, 40, 190);
                }

                DrawQuad(gd, r, c, color);
            }
        }
    }

    private bool IsInPreview(int row, int col, int cursorRow, int cursorCol, int shipSize, bool horizontal)
    {
        for (int i = 0; i < shipSize; i++)
        {
            int r = horizontal ? cursorRow : cursorRow + i;
            int c = horizontal ? cursorCol + i : cursorCol;
            if (r == row && c == col) return true;
        }
        return false;
    }

    private void DrawQuad(GraphicsDevice gd, int row, int col, Color color)
    {
        float half = Size * CellSize / 2f;
        const float pad = 0.05f;
        float x0 = -half + col * CellSize + pad;
        float x1 = x0 + CellSize - pad * 2f;
        float z0 = -half + row * CellSize + pad;
        float z1 = z0 + CellSize - pad * 2f;
        const float y = 0.01f;

        var verts = new VertexPositionColor[]
        {
            new(new Vector3(x0, y, z0), color),
            new(new Vector3(x1, y, z0), color),
            new(new Vector3(x0, y, z1), color),
            new(new Vector3(x1, y, z0), color),
            new(new Vector3(x1, y, z1), color),
            new(new Vector3(x0, y, z1), color),
        };

        _effect.CurrentTechnique.Passes[0].Apply();
        gd.DrawUserPrimitives(PrimitiveType.TriangleList, verts, 0, 2);
    }

    private void DrawGridLines(GraphicsDevice gd)
    {
        _effect.CurrentTechnique.Passes[0].Apply();
        gd.DrawUserPrimitives(PrimitiveType.LineList, _gridLines, 0, _gridLines.Length / 2);
    }
}
