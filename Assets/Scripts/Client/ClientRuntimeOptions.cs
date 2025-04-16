using Rover656.Survivors.Common.World;

namespace Rover656.Survivors.Client {
    public static class ClientRuntimeOptions {
        public static string RemoteEndpoint = "127.0.0.1:1337";
        public static LevelMode LevelMode = Common.World.LevelMode.StandardPlay;
        public static int? MaxPlayTime = null;

        public static bool RunIntegratedServer =
#if UNITY_EDITOR
            true
#else
            false
#endif
            ;
    }
}