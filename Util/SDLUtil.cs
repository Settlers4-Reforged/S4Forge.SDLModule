using Forge.Logging;

using SDL;

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Forge.SDLBackend.Util {
    internal static class SDLUtil {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Conditional("DEBUG")]
        public static void HandleSDLError(bool success, string? message = null) {
            if (!success) {
                LogSDLError(message);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Conditional("DEBUG")]
        public static void LogSDLError(string? message) {
            string? sdlGetError = SDL3.SDL_GetError();
            if (!string.IsNullOrEmpty(sdlGetError)) {
                Logger.LogError(new InvalidOperationException(message), sdlGetError ?? "SDL Error detected");
                SDL3.SDL_ClearError();
            }
        }
    }
}
