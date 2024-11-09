using Forge.UX.Rendering.Texture;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forge.SDLBackend.Rendering.Textures {
    internal class TextureCollectionManager : ITextureCollectionManager {
        private readonly Dictionary<int, TextureCollection> collections = new Dictionary<int, TextureCollection>();

        public ITextureCollection GetCollection(int id) {
            if (!collections.ContainsKey(id)) {
                collections[id] = new TextureCollection(id);
            }
            return collections[id];
        }

        public ITexture Get(int col, int id) {
            return GetCollection(col).GetTexture(id);
        }
    }
}
