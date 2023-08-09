using Forge.UX.Rendering;
using Forge.UX.Rendering.Text;
using Forge.UX.Rendering.Texture;
using Forge.UX.UI;
using Forge.UX.UI.Components;
using Forge.UX.UI.Elements;

using System;
using System.Numerics;

namespace S4_GFXBridge.Rendering {
    internal class SDLRenderer : IRenderer {
        public string Name => "SDLRenderer";

        public void RenderUIComponent(IUIComponent component, UIElement parent, SceneGraphState _) {
            throw new NotImplementedException();
        }

        public Vector2 GetScreenSize() {
            throw new NotImplementedException();
        }


    }
}
