using ICities;
using CSUtil.Commons;
using System.Reflection;

namespace TMPE_EVE
{
    public class Eve : IUserMod
    {
        public string Name => "TM:PE Emergency Vehicle Extension";

        public string Description => "Extends TM:PE's emergency vehicles functions.";

        public void OnEnabled() {
            Log.Info($"TM:PE Emergency Vehicle Extension enabled. Build {Assembly.GetExecutingAssembly().GetName().Version}");
        }

        public void OnDisabled() {
            Log.Info($"TM:PE Emergency Vehicle Extension disabled.");
        }
    }
}
