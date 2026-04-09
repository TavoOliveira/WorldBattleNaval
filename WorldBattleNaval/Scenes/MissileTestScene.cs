using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using WorldBattleNaval.Interfaces;

namespace WorldBattleNaval.Scenes;

public class MissileTestScene : IScene
{
    private readonly GraphicsDevice graphicsDevice;
    private readonly SceneManager sceneManager;

    private Camera camera;
    private Board board;

    // Hit detection
    private readonly bool[,] hasShip = new bool[Board.Size, Board.Size];

    private enum CellResult { None, Hit, Miss }
    private readonly CellResult[,] cellResults = new CellResult[Board.Size, Board.Size];
    private readonly List<(int row, int col, CellResult result)> resultList = new();

    // Missile state machine
    private enum MissilePhase { Idle, Pass, Dive }
    private MissilePhase missilePhase = MissilePhase.Idle;
    private float phaseT;

    private int missileTargetRow, missileTargetCol;
    private Vector3 missileTarget;   // board cell center
    private Vector3 passStart;       // where missile enters screen
    private Vector3 passMid;         // sweep-by point (just past target, still at height)
    private const float PassDuration = 2.5f;  // slow climb past camera
    private const float DiveDuration = 0.5f;  // fast plunge onto target

    // Mouse hover
    private int cursorRow, cursorCol;
    private bool hasCursor;

    public bool IsReady { get; private set; }

    public MissileTestScene(GraphicsDevice graphicsDevice, SceneManager sceneManager)
    {
        this.graphicsDevice = graphicsDevice;
        this.sceneManager = sceneManager;
    }

    public void LoadContent(ContentManager content)
    {
        camera = new Camera();
        board = new Board();

        var rng = new Random(7);
        int placed = 0;
        while (placed < 8)
        {
            int r = rng.Next(Board.Size);
            int c = rng.Next(Board.Size);
            if (!hasShip[r, c]) { hasShip[r, c] = true; placed++; }
        }

        IsReady = true;
    }

    public void Unload() => IsReady = false;

    public void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        switch (missilePhase)
        {
            case MissilePhase.Pass:
                phaseT += dt / PassDuration;
                if (phaseT >= 1f)
                {
                    phaseT = 0f;
                    missilePhase = MissilePhase.Dive;
                }
                return;

            case MissilePhase.Dive:
                phaseT += dt / DiveDuration;
                if (phaseT >= 1f)
                {
                    phaseT = 1f;
                    missilePhase = MissilePhase.Idle;
                    RegisterResult(missileTargetRow, missileTargetCol);
                }
                return;
        }

        // Idle — handle input
        hasCursor = TryGetBoardCell(out cursorRow, out cursorCol);

        if (InputManager.IsLeftClicked && hasCursor && cellResults[cursorRow, cursorCol] == CellResult.None)
            FireAt(cursorRow, cursorCol);
    }

    private void FireAt(int row, int col)
    {
        float half = Board.Size * Board.CellSize / 2f;
        float cx = -half + col * Board.CellSize + Board.CellSize / 2f;
        float cz = -half + row * Board.CellSize + Board.CellSize / 2f;

        missileTargetRow = row;
        missileTargetCol = col;
        missileTarget = new Vector3(cx, 0.18f, cz);

        // Camera is at ~(5, 30, 0.5) looking nearly straight down.
        // "Bottom of screen" = positive Z world (camera up-vector points toward -Z).
        // Missile starts low at positive Z, climbs steeply toward the camera lens,
        // peaks at Y=27 almost directly under the camera (3 units away) — appears HUGE —
        // then dives onto the target cell.
        passStart = new Vector3(5f, 4f, cz + 20f);  // enters from below, off-screen bottom
        passMid   = new Vector3(5f, 27f, 0.4f);     // peaks 3 units below camera — close-up

        phaseT = 0f;
        missilePhase = MissilePhase.Pass;
    }

    private void RegisterResult(int row, int col)
    {
        var result = hasShip[row, col] ? CellResult.Hit : CellResult.Miss;
        cellResults[row, col] = result;
        resultList.Add((row, col, result));
    }

    // Returns current missile world position and forward vector
    private (Vector3 pos, Vector3 forward) GetMissileTransform()
    {
        Vector3 pos, next;

        if (missilePhase == MissilePhase.Pass)
        {
            // Ease-in: slow start, accelerates as it climbs toward camera lens
            float s  = EaseIn(phaseT);
            float s2 = EaseIn(Math.Min(phaseT + 0.015f, 1f));
            pos  = Vector3.Lerp(passStart, passMid, s);
            next = Vector3.Lerp(passStart, passMid, s2);
        }
        else // Dive
        {
            // Falls from passMid back to target — ease-out so it slams in fast then lands
            float s  = EaseOut(phaseT);
            float s2 = EaseOut(Math.Min(phaseT + 0.04f, 1f));
            pos  = Vector3.Lerp(passMid, missileTarget, s);
            next = Vector3.Lerp(passMid, missileTarget, s2);
        }

        var forward = Vector3.Normalize(next - pos);
        if (forward == Vector3.Zero) forward = Vector3.Down;
        return (pos, forward);
    }

    private static float EaseIn(float t)  => t * t * t;           // slow start, fast finish
    private static float EaseOut(float t) => 1f - (1f - t) * (1f - t); // fast start, slows at end


    public void Draw(GameTime gameTime)
    {
        var view = camera.View;
        var projection = camera.Projection;

        graphicsDevice.Clear(new Color(10, 31, 68));

        board.Draw(graphicsDevice, view, projection);
        DrawCellOverlays(view, projection);

        if (hasCursor && missilePhase == MissilePhase.Idle && cellResults[cursorRow, cursorCol] == CellResult.None)
            DrawQuad(cursorRow, cursorCol, new Color(255, 240, 80, 100), view, projection);

        if (missilePhase != MissilePhase.Idle)
            DrawMissile(view, projection);

        sceneManager.SpriteBatch.Begin();
        DrawHUD();
        sceneManager.SpriteBatch.End();
    }

    private void DrawMissile(Matrix view, Matrix projection)
    {
        var model = sceneManager.Resources.Missil;
        var (pos, forward) = GetMissileTransform();

        var up = Math.Abs(Vector3.Dot(forward, Vector3.Up)) > 0.99f ? Vector3.Backward : Vector3.Up;

        float maxRadius = 0f;
        foreach (var mesh in model.Meshes)
            maxRadius = Math.Max(maxRadius, mesh.BoundingSphere.Radius);
        float scale = maxRadius > 0f ? 0.5f / maxRadius : 0.4f;

        var world = Matrix.CreateScale(scale)
                  * Matrix.CreateWorld(pos, forward, up);

        graphicsDevice.DepthStencilState = DepthStencilState.Default;
        graphicsDevice.BlendState = BlendState.Opaque;
        graphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

        foreach (var mesh in model.Meshes)
        {
            foreach (BasicEffect fx in mesh.Effects)
            {
                fx.EnableDefaultLighting();
                fx.View = view;
                fx.Projection = projection;
                fx.World = world;
            }
            mesh.Draw();
        }
    }

    private void DrawCellOverlays(Matrix view, Matrix projection)
    {
        foreach (var (r, c, result) in resultList)
        {
            var color = result == CellResult.Hit
                ? new Color(255, 60, 60, 210)
                : new Color(60, 130, 255, 180);
            DrawQuad(r, c, color, view, projection);
        }
    }

    private void DrawQuad(int row, int col, Color color, Matrix view, Matrix projection)
    {
        float half = Board.Size * Board.CellSize / 2f;
        const float pad = 0.08f;
        const float h = 0.20f;

        float x0 = -half + col * Board.CellSize + pad;
        float x1 = x0 + Board.CellSize - pad * 2f;
        float z0 = -half + row * Board.CellSize + pad;
        float z1 = z0 + Board.CellSize - pad * 2f;

        VertexPositionColor[] verts =
        [
            new(new Vector3(x0, h, z0), color),
            new(new Vector3(x1, h, z0), color),
            new(new Vector3(x1, h, z1), color),
            new(new Vector3(x0, h, z0), color),
            new(new Vector3(x1, h, z1), color),
            new(new Vector3(x0, h, z1), color),
        ];

        var effect = new BasicEffect(graphicsDevice)
        {
            VertexColorEnabled = true,
            LightingEnabled = false,
            View = view,
            Projection = projection,
            World = Matrix.Identity
        };

        graphicsDevice.BlendState = BlendState.AlphaBlend;
        graphicsDevice.DepthStencilState = DepthStencilState.Default;
        graphicsDevice.RasterizerState = RasterizerState.CullNone;

        effect.CurrentTechnique.Passes[0].Apply();
        graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, verts, 0, 2);
    }

    private void DrawHUD()
    {
        var res = sceneManager.Resources;
        var vp = graphicsDevice.Viewport;
        var sb = sceneManager.SpriteBatch;

        int hits = 0, misses = 0;
        foreach (var (_, _, r) in resultList)
        {
            if (r == CellResult.Hit) hits++;
            else misses++;
        }

        string title = "Teste de Ataque - Missel";
        var titleSz = res.Font.MeasureString(title);
        sb.DrawString(res.Font, title,
            new Vector2((vp.Width - titleSz.X) / 2f, 10f), Color.White);

        string status = missilePhase switch
        {
            MissilePhase.Pass => "Missel se aproximando...",
            MissilePhase.Dive => "Atacando!",
            _ => hasCursor && cellResults[cursorRow, cursorCol] == CellResult.None
                    ? "Clique para atacar"
                    : "Celula ja atacada"
        };
        sb.DrawString(res.SmallFont, status,
            new Vector2((vp.Width - res.SmallFont.MeasureString(status).X) / 2f, vp.Height - 40f),
            Color.LightGray);

        string score = $"Acertos: {hits}  |  Erros: {misses}";
        sb.DrawString(res.SmallFont, score, new Vector2(10f, 10f), Color.LightCyan);

        int legendY = vp.Height - 90;
        sb.Draw(res.Pixel, new Rectangle(10, legendY, 16, 16), new Color(255, 60, 60));
        sb.DrawString(res.TinyFont, "Acerto", new Vector2(32f, legendY), Color.White);
        sb.Draw(res.Pixel, new Rectangle(10, legendY + 22, 16, 16), new Color(60, 130, 255));
        sb.DrawString(res.TinyFont, "Erro", new Vector2(32f, legendY + 22), Color.White);
    }

    private bool TryGetBoardCell(out int row, out int col)
    {
        row = col = 0;
        var vp = graphicsDevice.Viewport;
        var mp = InputManager.MousePosition;

        var near = vp.Unproject(new Vector3(mp.X, mp.Y, 0f), camera.Projection, camera.View, Matrix.Identity);
        var far  = vp.Unproject(new Vector3(mp.X, mp.Y, 1f), camera.Projection, camera.View, Matrix.Identity);
        var dir  = Vector3.Normalize(far - near);

        if (MathF.Abs(dir.Y) < 1e-6f) return false;

        float t = -near.Y / dir.Y;
        if (t < 0f) return false;

        var hit  = near + t * dir;
        float half = Board.Size * Board.CellSize / 2f;
        int c = (int)MathF.Floor((hit.X + half) / Board.CellSize);
        int r = (int)MathF.Floor((hit.Z + half) / Board.CellSize);

        if (r < 0 || r >= Board.Size || c < 0 || c >= Board.Size) return false;

        row = r; col = c;
        return true;
    }
}
