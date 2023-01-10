using S4_UIEngine;
using SDL2;
using System.Numerics;

namespace S4_GFXBridge {
    internal class InputManager : IInputManager {
        public bool IsMouseInRectangle(Vector4 rect) {
            SDL.SDL_GetMouseState(out int x, out int y);


            return x >= rect.X && y >= rect.Y && x <= rect.Z && y <= rect.W;
        }
    }
}
