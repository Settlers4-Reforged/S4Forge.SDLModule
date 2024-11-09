using Forge.UX.Rendering.Texture;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forge.SDLBackend.Rendering.Textures {
    internal class TextureCollection : ITextureCollection {
        Dictionary<int, Texture> textures = new Dictionary<int, Texture>();

        private int collectionId;

        public TextureCollection(int id) {
            this.collectionId = id;
        }

        public ITexture GetTexture(int id) {
            if (!textures.TryGetValue(id, out Texture? texture)) {
                Texture value = new Texture();
                textures.Add(id, value);
                return value;
                //throw new KeyNotFoundException($"Texture with id {id} in collection {collectionId} not found");
            }

            return texture;
        }
    }
}
