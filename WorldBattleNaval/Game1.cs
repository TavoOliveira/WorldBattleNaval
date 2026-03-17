using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace WorldBattleNaval;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = null!;

    private Board _board = null!;
    private Submarine _submarine = null!;
    private Camera _camera = null!;
    private InputHandler _input = null!;

    private ButtonState _prevLeftButton;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        _input = new InputHandler();
        _camera = new Camera();
        _camera.Initialize(Mouse.GetState());
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        var model = Content.Load<Model>("models/Submarino");
        _board = new Board(GraphicsDevice);
        _submarine = new Submarine(model);
    }

    protected override void Update(GameTime gameTime)
    {
        _input.Update();
        var mouse = Mouse.GetState();

        if (_input.IsDown(Keys.Escape) ||
            GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            Exit();

        _camera.Update(mouse, GraphicsDevice.Viewport.AspectRatio);
        _board.Update(gameTime);
        _submarine.Update(gameTime);

        if (!_submarine.IsPlaced)
            HandlePlacement(mouse);
        else
            HandlePostPlacement();

        _prevLeftButton = mouse.LeftButton;
        base.Update(gameTime);
    }

    private void HandlePlacement(MouseState mouse)
    {
        // Mover cursor via mouse (ray cast contra o plano Y=0)
        if (TryGetBoardCell(mouse, out int row, out int col))
            _board.SetCursor(row, col);

        // Mover cursor com setas (alternativo ao mouse)
        if (_input.IsPressed(Keys.Up))    _board.MoveCursor(-1,  0);
        if (_input.IsPressed(Keys.Down))  _board.MoveCursor( 1,  0);
        if (_input.IsPressed(Keys.Left))  _board.MoveCursor( 0, -1);
        if (_input.IsPressed(Keys.Right)) _board.MoveCursor( 0,  1);

        // Rotacionar com R ou botão direito do mouse
        if (_input.IsPressed(Keys.R)) _submarine.Rotate();

        // Confirmar com clique esquerdo ou Enter/Espaço
        bool leftClicked = mouse.LeftButton == ButtonState.Pressed &&
                           _prevLeftButton  == ButtonState.Released;

        if (leftClicked || _input.IsPressed(Keys.Enter) || _input.IsPressed(Keys.Space))
        {
            var (r, c) = _board.CursorPosition;
            if (_board.CanPlace(r, c, _submarine.Size, _submarine.IsHorizontal))
            {
                _board.Place(r, c, _submarine.Size, _submarine.IsHorizontal);
                _submarine.Place(r, c);
            }
        }
    }

    private void HandlePostPlacement()
    {
        // Desfazer posicionamento com Delete ou Backspace
        if (_input.IsPressed(Keys.Delete) || _input.IsPressed(Keys.Back))
        {
            _board.Clear(_submarine.PlacedRow, _submarine.PlacedCol,
                         _submarine.Size, _submarine.PlacedHorizontal);
            _submarine.Reset();
        }
    }

    /// <summary>
    /// Lança um raio do mouse e retorna a célula do tabuleiro sob o cursor,
    /// intersectando com o plano horizontal Y = 0.
    /// </summary>
    private bool TryGetBoardCell(MouseState mouse, out int row, out int col)
    {
        row = col = 0;

        var viewport = GraphicsDevice.Viewport;
        var mousePos = new Vector3(mouse.X, mouse.Y, 0f);

        var near = viewport.Unproject(mousePos with { Z = 0f },
            _camera.Projection, _camera.View, Matrix.Identity);
        var far  = viewport.Unproject(mousePos with { Z = 1f },
            _camera.Projection, _camera.View, Matrix.Identity);

        var dir = Vector3.Normalize(far - near);

        // Intersecção com Y = 0: near.Y + t * dir.Y = 0
        if (MathF.Abs(dir.Y) < 1e-6f) return false;

        float t = -near.Y / dir.Y;
        if (t < 0f) return false;

        var hit = near + t * dir;

        float half = Board.Size * Board.CellSize / 2f;
        int c = (int)MathF.Floor((hit.X + half) / Board.CellSize);
        int r = (int)MathF.Floor((hit.Z + half) / Board.CellSize);

        if (r < 0 || r >= Board.Size || c < 0 || c >= Board.Size) return false;

        row = r;
        col = c;
        return true;
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(8, 20, 50));

        var view = _camera.View;
        var projection = _camera.Projection;

        // 1. Tabuleiro (água + grade)
        _board.Draw(GraphicsDevice, _submarine, view, projection);

        // 2. Submarino
        if (_submarine.IsPlaced)
        {
            _submarine.Draw(GraphicsDevice, _submarine.PlacedRow, _submarine.PlacedCol,
                            view, projection, alpha: 1f);
        }
        else
        {
            var (row, col) = _board.CursorPosition;
            bool canPlace = _board.CanPlace(row, col, _submarine.Size, _submarine.IsHorizontal);
            _submarine.Draw(GraphicsDevice, row, col, view, projection, alpha: canPlace ? 0.75f : 0.4f);
        }

        base.Draw(gameTime);
    }
}
