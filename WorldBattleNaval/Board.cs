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

    // Water wave mesh
    private const int WaterSub = 50; // subdivisões por eixo
    private readonly VertexPositionColor[] _waterVerts = new VertexPositionColor[WaterSub * WaterSub * 6];
    private float _waterTime = 0f;

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
        UpdateWaterMesh(); // inicializa mesh para não ter lixo no primeiro frame
    }

    // ──────────────────────────────────────────────
    // Update
    // ──────────────────────────────────────────────

    public void Update(GameTime gameTime)
    {
        _waterTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
        UpdateWaterMesh();
    }

    // ──────────────────────────────────────────────
    // Water wave mesh
    // ──────────────────────────────────────────────

    private float WaveY(float x, float z)
    {
        const float amp = 0.07f;
        return amp * (
            MathF.Sin(x * 1.4f + _waterTime * 1.1f) * MathF.Cos(z * 1.0f + _waterTime * 0.8f) +
            MathF.Sin(x * 0.7f - _waterTime * 0.6f + z * 1.2f) * 0.45f +
            MathF.Cos(x * 2.0f + z * 0.9f - _waterTime * 1.5f) * 0.2f
        );
    }

    private Color WaveColor(float y, float cellBaseAlpha)
    {
        // y em [-amp, +amp] ≈ [-0.1, 0.1] após somatório
        float t = Math.Clamp((y + 0.12f) / 0.24f, 0f, 1f);
        var dark  = new Color(8,  55, 140, (int)(cellBaseAlpha * 255));
        var light = new Color(40, 140, 220, (int)(Math.Min(cellBaseAlpha + 0.15f, 1f) * 255));
        return Color.Lerp(dark, light, t);
    }

    private void UpdateWaterMesh()
    {
        float half = Size * CellSize / 2f;
        float step = (Size * CellSize) / WaterSub;
        int idx = 0;

        for (int iz = 0; iz < WaterSub; iz++)
        {
            for (int ix = 0; ix < WaterSub; ix++)
            {
                float x0 = -half + ix * step;
                float x1 = x0 + step;
                float z0 = -half + iz * step;
                float z1 = z0 + step;

                // Determina alpha base da célula que contém este quad
                float alpha = CellAlphaAt(x0 + step * 0.5f, z0 + step * 0.5f);

                float y00 = WaveY(x0, z0);
                float y10 = WaveY(x1, z0);
                float y01 = WaveY(x0, z1);
                float y11 = WaveY(x1, z1);

                _waterVerts[idx++] = new VertexPositionColor(new Vector3(x0, y00, z0), WaveColor(y00, alpha));
                _waterVerts[idx++] = new VertexPositionColor(new Vector3(x1, y10, z0), WaveColor(y10, alpha));
                _waterVerts[idx++] = new VertexPositionColor(new Vector3(x0, y01, z1), WaveColor(y01, alpha));

                _waterVerts[idx++] = new VertexPositionColor(new Vector3(x1, y10, z0), WaveColor(y10, alpha));
                _waterVerts[idx++] = new VertexPositionColor(new Vector3(x1, y11, z1), WaveColor(y11, alpha));
                _waterVerts[idx++] = new VertexPositionColor(new Vector3(x0, y01, z1), WaveColor(y01, alpha));
            }
        }
    }

    /// Retorna alpha base (~opacidade) para a posição de mundo (wx, wz).
    private float CellAlphaAt(float wx, float wz)
    {
        float half = Size * CellSize / 2f;
        int col = (int)MathF.Floor((wx + half) / CellSize);
        int row = (int)MathF.Floor((wz + half) / CellSize);
        if (row < 0 || row >= Size || col < 0 || col >= Size) return 0.5f;

        return _cells[row, col].State == CellState.Occupied ? 0.75f : 0.5f;
    }

    // ──────────────────────────────────────────────
    // Grid lines
    // ──────────────────────────────────────────────

    private void BuildGridLines()
    {
        var lines = new List<VertexPositionColor>();
        float half = Size * CellSize / 2f;
        var color = new Color(200, 230, 255, 200);

        for (int i = 0; i <= Size; i++)
        {
            float t = -half + i * CellSize;
            lines.Add(new VertexPositionColor(new Vector3(-half, 0.18f, t), color));
            lines.Add(new VertexPositionColor(new Vector3(half,  0.18f, t), color));
            lines.Add(new VertexPositionColor(new Vector3(t, 0.18f, -half), color));
            lines.Add(new VertexPositionColor(new Vector3(t, 0.18f,  half), color));
        }

        _gridLines = lines.ToArray();
    }

    // ──────────────────────────────────────────────
    // Cursor
    // ──────────────────────────────────────────────

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

    // ──────────────────────────────────────────────
    // Placement logic
    // ──────────────────────────────────────────────

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

    // ──────────────────────────────────────────────
    // Draw
    // ──────────────────────────────────────────────

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

        DrawWater(gd);
        DrawCellOverlays(gd, submarine, canPlace);
        DrawGridLines(gd);

        gd.RasterizerState = RasterizerState.CullCounterClockwise;
    }

    private void DrawWater(GraphicsDevice gd)
    {
        _effect.CurrentTechnique.Passes[0].Apply();
        gd.DrawUserPrimitives(PrimitiveType.TriangleList, _waterVerts, 0, _waterVerts.Length / 3);
    }

    /// Overlays semitransparentes acima das ondas para mostrar estados (cursor, preview, ocupado).
    private void DrawCellOverlays(GraphicsDevice gd, Submarine submarine, bool canPlace)
    {
        for (int r = 0; r < Size; r++)
        {
            for (int c = 0; c < Size; c++)
            {
                Color? overlay = null;

                if (_cells[r, c].State == CellState.Occupied)
                    overlay = new Color(20, 60, 140, 120);

                if (!submarine.IsPlaced && IsInPreview(r, c, _cursorRow, _cursorCol, submarine.Size, submarine.IsHorizontal))
                    overlay = canPlace ? new Color(0, 200, 80, 160) : new Color(220, 40, 40, 160);

                if (overlay.HasValue)
                    DrawOverlayQuad(gd, r, c, overlay.Value);
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

    private void DrawOverlayQuad(GraphicsDevice gd, int row, int col, Color color)
    {
        float half = Size * CellSize / 2f;
        const float pad = 0.05f;
        float x0 = -half + col * CellSize + pad;
        float x1 = x0 + CellSize - pad * 2f;
        float z0 = -half + row * CellSize + pad;
        float z1 = z0 + CellSize - pad * 2f;
        const float y = 0.15f; // acima do pico das ondas

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
