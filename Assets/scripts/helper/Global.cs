static public class Global {
    static public bool isReady = false;

    static public int curCheckpoint = 0;
    static public MinionGoalBlock sceneMinionGoal = null;

    static public void setup() {
        if (Global.isReady)
            return;

        PRNG.setup();
        Global.isReady = true;
        Global.curCheckpoint = 0;
        Global.sceneMinionGoal = null;
    }

    /** Implements a simple (Box-Muller?) PRNG */
    static public class PRNG {
        static private uint seed;
        static private uint rngA = 0x0019660d;
        static private uint rngC = 0x3c6ef35f;

        static public void setup() {
            System.Security.Cryptography.RNGCryptoServiceProvider rng;
            rng = new System.Security.Cryptography.RNGCryptoServiceProvider();

            byte[] randSeed = new byte[4];
            rng.GetBytes(randSeed);

            rng.Dispose();

            PRNG.seed = 0;
            foreach (byte b in randSeed) {
                PRNG.seed = (PRNG.seed << 8) | (0xff & (uint)b);
            }
        }

        static public uint fastUint() {
            uint seed = PRNG.seed;
            PRNG.seed = PRNG.rngA * PRNG.seed + PRNG.rngC;
            return seed;
        }

        static public int fastInt() {
            return (int)(PRNG.fastUint() & 0x7fffffff);
        }

        static public int fastRange(int min, int max) {
            int delta = PRNG.fastInt() % (1 + max - min);
            return min + delta;
        }
    }
}
