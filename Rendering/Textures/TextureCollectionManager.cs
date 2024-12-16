using Forge.UX.Rendering.Texture;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forge.SDLBackend.Rendering.Textures {
    internal class TextureCollectionManager : ITextureCollectionManager {
        private readonly Dictionary<string, TextureCollection> collections = new Dictionary<string, TextureCollection>();

        public ITextureCollection GetCollection(string id) {
            if (!collections.ContainsKey(id)) {
                collections[id] = new TextureCollection(id);
            }
            return collections[id];
        }

        public ITexture Get(string col, string id) {
            return GetCollection(col).GetTexture(id);
        }
    }
}
