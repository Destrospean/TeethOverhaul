using Sims3.SimIFace;

namespace Sims3.Gameplay.TeethOverhaul
{
    public class Settings
    {
        [Tunable]
        public static bool kAutoRandomizeTeethOnSimAgeTransition;

        [Tunable]
        public static bool kAutoRandomizeTeethOnSimInstantiated;

        [Tunable]
        public static bool kShowCheatInteractions;
    }
}
