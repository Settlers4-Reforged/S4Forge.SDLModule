using Forge.UX.UI;

using SDL;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forge.SDLBackend.Rendering.Components {
    internal unsafe class NullComponentRenderer : IUXComponent {
        public void Render(SDL_Renderer* renderer, SceneGraphState sceneGraphState) {
            // Nothing happens here...
            // This is only for unsupported components
        }
    }
}
