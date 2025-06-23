using Forge.Config;
using Forge.Engine;
using Forge.Logging;
using Forge.SDLBackend.Util;
using Forge.UX.Rendering.Text;

using SDL;

using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace Forge.SDLBackend.Rendering.Text {
    internal unsafe class TextRenderer {
        SDLRenderer renderer;

        public TTF_TextEngine* engine;

        // <TextStyle, TTF_Font*>
        private Dictionary<TextStyle, IntPtr> fonts = new Dictionary<TextStyle, IntPtr>();

        private List<SDLText> texts = new List<SDLText>();

        public TextRenderer(SDLRenderer renderer) {
            this.renderer = renderer;
            this.renderer.OnUpdateRenderer += RecreateRenderer;
        }

        internal void Initialize() {
            RecreateRenderer();
        }

        public static float TextFontSizeToDIP(TextStyleSize size) {
            switch (size) {
                case TextStyleSize.Small:
                    return 12f;
                default:
                case TextStyleSize.Regular:
                    return 16f;
                case TextStyleSize.Large:
                    return 20f;
            }
        }

        private TTF_Font* CreateFont(TextStyle style) {
            Logger.LogDebug("TextRenderer: Creating font with size {0}", style.Size);

            ModuleEnvironment<UXEngineSDLRenderer> env = DI.Resolve<ModuleEnvironment<UXEngineSDLRenderer>>();
            string fontPath = Path.Join(env.Path, "Fonts", "arial.ttf");
            if (!File.Exists(fontPath)) {
                Logger.LogError(null, "TextRenderer: Font file '{0}' does not exist", fontPath);
                return null;
            }

            TTF_Font* font = SDL3_ttf.TTF_OpenFont(fontPath, TextFontSizeToDIP(style.Size));
            if (font == null) {
                SDLUtil.LogSDLError($"TextRenderer: Failed to load font '{fontPath}'");
                return null;
            }

            SDL3_ttf.TTF_SetFontStyle(font, (int)style.Type);
            SDL3_ttf.TTF_SetFontSize(font, TextFontSizeToDIP(style.Size));
            SDL3_ttf.TTF_SetFontWrapAlignment(font, (TTF_HorizontalAlignment)style.HorizontalAlignment);
            // Vertical alignment is not supported by SDL_ttf, so we ignore it here and handle it in the text rendering logic

            return font;
        }

        internal TTF_Font* GetFont(TextStyle style) {
            if (fonts.TryGetValue(style, out IntPtr fontPtr)) {
                return (TTF_Font*)fontPtr;
            }

            TTF_Font* font = CreateFont(style);
            if (font == null)
                throw new InvalidOperationException($"Failed to create font for style {style}");

            fonts[style] = (IntPtr)font;
            return font;
        }

        private void RecreateRenderer() {
            Logger.LogDebug("TextRenderer: Recreating TTF text engine");

            foreach (var text in texts) {
                text.DestroyText();
            }

            if (engine != null)
                SDL3_ttf.TTF_DestroyRendererTextEngine(engine);

            engine = SDL3_ttf.TTF_CreateRendererTextEngine(renderer.Renderer);
            if (engine == null) {
                SDLUtil.LogSDLError("TextRenderer: Failed to create TTF text engine");
                return;
            }

            foreach (var text in texts) {
                text.CreateText();
            }
        }

        public SDLText CreateText(TextStyle style, Vector2? textBoxSize = null) {
            SDLText text = new SDLText(this, style, textBoxSize);
            texts.Add(text);
            return text;
        }

        public void DestroyText(SDLText text) {
            texts.Remove(text);
            text.Destroy();
        }
    }
}
