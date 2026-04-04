using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace WorldBattleNaval;

public static class InputManager
{
    private static MouseState _current;
    private static MouseState _previous;
    private static KeyboardState _currentKeys;
    private static KeyboardState _previousKeys;

    public static Point MousePosition => new(_current.X, _current.Y);

    // Mouse - held down
    public static bool IsLeftDown    => _current.LeftButton   == ButtonState.Pressed;
    public static bool IsRightDown   => _current.RightButton  == ButtonState.Pressed;
    public static bool IsMiddleDown  => _current.MiddleButton == ButtonState.Pressed;

    // Mouse - just pressed this frame
    public static bool IsLeftPressed   => _current.LeftButton   == ButtonState.Pressed  && _previous.LeftButton   == ButtonState.Released;
    public static bool IsRightPressed  => _current.RightButton  == ButtonState.Pressed  && _previous.RightButton  == ButtonState.Released;
    public static bool IsMiddlePressed => _current.MiddleButton == ButtonState.Pressed  && _previous.MiddleButton == ButtonState.Released;

    // Mouse - just released this frame (click)
    public static bool IsLeftClicked   => _current.LeftButton   == ButtonState.Released && _previous.LeftButton   == ButtonState.Pressed;
    public static bool IsRightClicked  => _current.RightButton  == ButtonState.Released && _previous.RightButton  == ButtonState.Pressed;
    public static bool IsMiddleClicked => _current.MiddleButton == ButtonState.Released && _previous.MiddleButton == ButtonState.Pressed;

    // Keyboard
    public static bool IsKeyDown(Keys key)     => _currentKeys.IsKeyDown(key);
    public static bool IsKeyPressed(Keys key)  => _currentKeys.IsKeyDown(key) && !_previousKeys.IsKeyDown(key);
    public static bool IsKeyReleased(Keys key) => !_currentKeys.IsKeyDown(key) && _previousKeys.IsKeyDown(key);

    public static void Update()
    {
        _previous     = _current;
        _previousKeys = _currentKeys;
        _current      = Mouse.GetState();
        _currentKeys  = Keyboard.GetState();
    }
}
