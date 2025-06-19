using DryIoc.ImTools;

using Forge.Config;
using Forge.Engine;
using Forge.Logging;
using Forge.SDLBackend.Rendering.Text;
using Forge.SDLBackend.Util;
using Forge.UX.Rendering;
using Forge.UX.Rendering.Text;
using Forge.UX.UI;
using Forge.UX.UI.Components;
using Forge.UX.UI.Elements;

using SDL;

using System;
using System.IO;
using System.Numerics;
using System.Text;


namespace Forge.SDLBackend.Rendering.Components;

internal unsafe class TextComponentRenderer : IUXComponent {
    private UIElement Element { get; set; }
    private TextComponent Component { get; set; }

    private TextRenderer engine;
    private SDLText text;

    public TextComponentRenderer(TextComponent component, UIElement element) {
        Component = component;
        Element = element;

        engine = DI.Resolve<TextRenderer>();
        text = engine.CreateText(Component.Style);
    }


    public void Render(SDL_Renderer* renderer, SceneGraphState sceneGraphState) {
        (Vector2 elementPos, Vector2 elementSize) = sceneGraphState.TranslateComponent(Element, Component);
        text.SetTextBoxSize(elementSize);
        text.SetText(Component.Text);

        Component.CalculatedSize = text.TextSize;
        text.Draw(elementPos);
    }
}
