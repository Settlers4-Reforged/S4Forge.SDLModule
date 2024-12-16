using SDL;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forge.SDLBackend.Rendering {
    internal unsafe interface ISDLRenderer {
        SDL_Renderer* Renderer { get; }
        SDL_Window* Window { get; }

        void AttachToWindow();
        bool CreateRenderer();

        void BeginPresent();
        void EndPresent();
    }
}
