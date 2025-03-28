using Forge.Logging;

using SDL;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forge.SDLBackend.Util {
    internal static class SDLUtil {
        public static bool HandleSDLError(bool success, string? message = null) {
            if (!success) {
                LogSDLError(message);
            }
            return success;
        }

        public static void LogSDLError(string? message) {
            string? sdlGetError = SDL3.SDL_GetError();
            if (!string.IsNullOrEmpty(sdlGetError)) {
                Logger.LogError(new InvalidOperationException(message), sdlGetError ?? "SDL Error detected");
                SDL3.SDL_ClearError();
            }
        }
    }
}
