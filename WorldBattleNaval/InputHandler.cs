using Microsoft.Xna.Framework.Input;

namespace WorldBattleNaval;

public class InputHandler
{
    private KeyboardState _prev;
    private KeyboardState _current;

    public void Update()
    {
        _prev = _current;
        _current = Keyboard.GetState();
    }

    /// <summary>Retorna true apenas no frame em que a tecla foi pressionada.</summary>
    public bool IsPressed(Keys key) =>
        _current.IsKeyDown(key) && !_prev.IsKeyDown(key);

    /// <summary>Retorna true enquanto a tecla estiver pressionada.</summary>
    public bool IsDown(Keys key) => _current.IsKeyDown(key);
}
