using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WorldBattleNaval;

public class Board
{
    public const int Size = 10;
    public const float CellSize = 2f;

    private readonly Cell[,] cells = new Cell[Size, Size];
    private int cursorRow;
    private int cursorCol;

    private VertexPositionColor[] gridLines;

    public (int row, int col) CursorPosition => (cursorRow, cursorCol);

    public Board()
    {
        for (var row = 0; row < Size; row++)
            for (var col = 0; col < Size; col++)
                cells[row, col] = new Cell(row, col);

        BuildGridLines();
    }

    public void Draw(GraphicsDevice graphicsDevice, Matrix view, Matrix projection)
    {
        graphicsDevice.BlendState = BlendState.AlphaBlend;
        graphicsDevice.DepthStencilState = DepthStencilState.Default;
        graphicsDevice.RasterizerState = RasterizerState.CullNone;

        var effect = new BasicEffect(graphicsDevice)
        {
            VertexColorEnabled = true,
            LightingEnabled = false
        };

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

        Console.WriteLine($"Row: {row}, Col: {col}");
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