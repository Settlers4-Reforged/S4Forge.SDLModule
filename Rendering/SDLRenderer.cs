using S4_UIEngine.Rendering;
using S4_UIEngine.Rendering.Text;
using S4_UIEngine.Rendering.Texture;
using S4_UIEngine.UI.Components;
using S4_UIEngine.UI.Elements;
using System;
using System.Numerics;

namespace S4_GFXBridge.Rendering {
    internal class SDLRenderer : IRenderer {
        public void RenderUIComponent(IUIComponent component, UIElement parent) {
            throw new NotImplementedException();
        }

        public void RenderTeamTexture(ITexture texture, Vector2 position, Vector2 size) {
            throw new NotImplementedException();
        }

        public ITextRenderer GetTextRenderer() {
            throw new NotImplementedException();
        }
    }
}
