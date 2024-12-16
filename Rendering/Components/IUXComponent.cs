using Forge.UX.Rendering;
using Forge.UX.UI;
using Forge.UX.UI.Components;
using Forge.UX.UI.Elements;

using SDL;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forge.SDLBackend.Rendering.Components {
    internal unsafe interface IUXComponent : IElementData {
        void Render(SDL_Renderer* renderer, SceneGraphState sceneGraphState);
    }
}
