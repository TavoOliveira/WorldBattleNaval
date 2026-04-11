using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace WorldBattleNaval;

public static class InputManager
{
    private static MouseState current;
    private static MouseState previous;
    private static KeyboardState currentKeys;
    private static KeyboardState previousKeys;

    public static Point MousePosition => new(current.X, current.Y);

    // Mouse - held down
    public static bool IsLeftDown => current.LeftButton == ButtonState.Pressed;
    public static bool IsRightDown => current.RightButton == ButtonState.Pressed;
    public static bool IsMiddleDown => current.MiddleButton == ButtonState.Pressed;

    // Mouse - just pressed this frame
    public static bool IsLeftPressed =>
        current.LeftButton == ButtonState.Pressed && previous.LeftButton == ButtonState.Released;

    public static bool IsRightPressed =>
        current.RightButton == ButtonState.Pressed && previous.RightButton == ButtonState.Released;

    public static bool IsMiddlePressed =>
        current.MiddleButton == ButtonState.Pressed && previous.MiddleButton == ButtonState.Released;

    // Mouse - just released this frame (click)
    public static bool IsLeftClicked =>
        current.LeftButton == ButtonState.Released && previous.LeftButton == ButtonState.Pressed;

    public static bool IsRightClicked =>
        current.RightButton == ButtonState.Released && previous.RightButton == ButtonState.Pressed;

    public static bool IsMiddleClicked =>
        current.MiddleButton == ButtonState.Released && previous.MiddleButton == ButtonState.Pressed;

    // Keyboard
    public static bool IsKeyDown(Keys key) => currentKeys.IsKeyDown(key);
    public static bool IsKeyPressed(Keys key) => currentKeys.IsKeyDown(key) && !previousKeys.IsKeyDown(key);
    public static bool IsKeyReleased(Keys key) => !currentKeys.IsKeyDown(key) && previousKeys.IsKeyDown(key);

    public static void Update()
    {
        previous = current;
        previousKeys = currentKeys;
        current = Mouse.GetState();
        currentKeys = Keyboard.GetState();
    }
}