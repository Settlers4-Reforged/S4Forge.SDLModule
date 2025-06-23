using Forge.SDLBackend.Util;
using Forge.UX.Rendering.Text;

using SDL;

using System;
using System.Numerics;

namespace Forge.SDLBackend.Rendering.Text {
    internal unsafe class SDLText {
        // Font is stable across engine recreations
        private TTF_Font* assignedFont;
        private TTF_Text* text;
        private string stringText = "";

        private TextStyle style;
        private Vector2 textBoxSize;

        private TextRenderer renderer;

        internal SDLText(TextRenderer renderer, TextStyle style, Vector2? textBoxSize = null) {
            this.renderer = renderer;
            this.style = style;
            this.textBoxSize = textBoxSize ?? Vector2.Zero;
            assignedFont = renderer.GetFont(style);
            text = null;

            CreateText();
        }

        internal void CreateText() {
            if (text != null)
                DestroyText();

            text = SDL3_ttf.TTF_CreateText(renderer.engine, assignedFont, stringText, 0);
        }

        internal void DestroyText() {
            if (text == null)
                return;

            SDL3_ttf.TTF_DestroyText(text);
            text = null;
        }

        public void SetText(string text) {
            if (stringText == text) return; // Setting text to same value would be expensive
            stringText = text;

            SDL3_ttf.TTF_SetTextString(this.text, stringText, 0);
            DirtyCheck(true);
        }

        public void SetTextBoxSize(Vector2 size) {
            if (size.Equals(textBoxSize)) return;
            textBoxSize = size;
            CalculateTextSize();
        }


        public void Draw(Vector2 position) {
            if (stringText == "")
                return;

            ApplyStyle();

            float textHeight = TextSize.Y;

            TextStyleAlignment verticalAlignment = style.VerticalAlignment;
            int heightOffset = verticalAlignment switch {
                TextStyleAlignment.Start => 0,
                TextStyleAlignment.Center => (int)(textBoxSize.Y - textHeight) / 2,
                TextStyleAlignment.End => (int)(textBoxSize.Y - textHeight),
                _ => throw new ArgumentOutOfRangeException(nameof(style.VerticalAlignment), verticalAlignment, null)
            };
            float elementPosY = position.Y + heightOffset;
            float elementPosX = position.X;

            if (style.Shadowed) {
                SDL3_ttf.TTF_SetTextColor(text, 0, 0, 0, 255);
                SDLUtil.HandleSDLError(SDL3_ttf.TTF_DrawRendererText(text, elementPosX + 3, elementPosY + 3));
            }


            Vector4 color = style.Color;
            SDL3_ttf.TTF_SetTextColor(text, (byte)color.X, (byte)color.Y, (byte)color.Z, (byte)color.W);
            SDLUtil.HandleSDLError(SDL3_ttf.TTF_DrawRendererText(text, elementPosX, elementPosY));
        }

        private void ApplyStyle() {
            // Text style:
            SDL3_ttf.TTF_SetTextWrapWidth(text, style.Wrapped ? (int)textBoxSize.X : 0);
        }

        private bool DirtyCheck(bool force = false) {
            // Check if the style has changed
            if (!force && assignedFont == renderer.GetFont(style)) {
                return false;
            }

            assignedFont = renderer.GetFont(style);
            SDL3_ttf.TTF_SetTextFont(text, assignedFont);

            CalculateTextSize();

            return true;
        }

        Vector2? textSize = null;
        public Vector2 TextSize {
            get {
                DirtyCheck();
                return textSize ?? Vector2.Zero;
            }
        }

        private Vector2 CalculateTextSize() {
            ApplyStyle();

            int width = 0, height = 0;
            SDLUtil.HandleSDLError(SDL3_ttf.TTF_GetTextSize(text, &width, &height));

            textSize = new(width, height);
            return textSize.Value;
        }

        internal void Destroy() {
            throw new NotImplementedException();
        }
    }
}
