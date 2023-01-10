using NetModAPI;

namespace S4_GFXBridge.S4Hooks {
    internal class S4ModAPI {
        public static NetS4ModApi API { get; }

        static S4ModAPI() {
            API = new NetS4ModApi();
        }

        public S4ModAPI() {
            unsafe {
                API.AddFrameListener((surface, width, reserved) => 0);
            }
        }
    }
}
