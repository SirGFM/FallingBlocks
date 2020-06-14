using GO = UnityEngine.GameObject;

static public class Global {
    public enum ParticleQuality {
        Off = 0,
        Low,
        Mid,
        High,
    };

    static public bool isReady = false;

    static public float camX = 1.0f;
    static public float camY = 1.0f;

    static public ParticleQuality particleQuality = ParticleQuality.High;

    static public void setup() {
        if (Global.isReady)
            return;

        PRNG.setup();
        Sfx.setup();
        Global.isReady = true;
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

    /* Store SFX objects through scenes */
    static public class Sfx {
        private class Sound {
            private string src;
            private Sound next;

            private SfxrSynth sfx;
            private bool loaded;
            private uint mutationsNum;
            private float mutationAmount;

            public enum Variability {
                None,
                Low,
                Medium,
                High
            };

            private void ctor(string src, Variability var, Sound next) {
                this.src = src;
                this.next = next;
                this.sfx = null;
                this.loaded = false;
                switch (var) {
                case Variability.None:
                    this.mutationsNum = 1;
                    this.mutationAmount = 0.0f;
                    break;
                case Variability.Low:
                    this.mutationsNum = 5;
                    this.mutationAmount = 0.001f;
                    break;
                case Variability.Medium:
                    this.mutationsNum = 10;
                    this.mutationAmount = 0.01f;
                    break;
                case Variability.High:
                    this.mutationsNum = 15;
                    this.mutationAmount = 0.05f;
                    break;
                }
            }

            public Sound(string src, Variability var, Sound next) {
                this.ctor(src, var, next);
            }

            public Sound(string src, Variability var) {
                this.ctor(src, var, null);
            }

            public void load() {
                if (this.sfx == null) {
                    this.sfx = new SfxrSynth();
                    this.sfx.parameters.SetSettingsString(this.src);
                    this.sfx.CacheMutations(this.mutationsNum,
                                            this.mutationAmount);
/*
                                            () => {
                                                this.loaded = true;
                                                if (this.next != null)
                                                    this.next.load();
                                            });
*/
                    this.loaded = true;
                }
            }

            public void play(UnityEngine.Transform target) {
                if (this.loaded) {
                    this.sfx.SetParentTransform(target);
                    this.sfx.PlayMutated(this.mutationAmount, this.mutationsNum);
                }
            }
        }

        static private Sound moveMenu = new Sound(",0.5,,0.1307,,0.0713,0.3,0.5174,,,,,,,,,,,,,0.5394,,,,,1,,,0.1,,,,masterVolume",
                                                  Sound.Variability.Low);
        static private Sound enterMenu = new Sound(",0.5,,0.1696,,0.1923,0.3,0.3811,,,,0.1,0.505,,,,,,,,0.4415,,,,,1,,,0.1,,,,masterVolume",
                                                  Sound.Variability.Medium);
        static private Sound cancelMenu = new Sound(",0.5,,0.1696,,0.1923,0.3,0.3811,,-0.1949,,0.25,0.505,,,,,,,,0.4415,,,,,1,,,0.1,,,,masterVolume",
                                                  Sound.Variability.Medium);

        static private bool init = false;
        static private GO globalTargetObject = null;
        static private UnityEngine.Transform globalTarget = null;

        static public void setup() {
            if (init)
                return;
            init = true;

            globalTargetObject = new GO();
            globalTargetObject.name = "Global Audio Player";
            GO.DontDestroyOnLoad(globalTargetObject);
            globalTarget = globalTargetObject.transform;

            /* Load every sound, sequentially */
            moveMenu.load();
            enterMenu.load();
            cancelMenu.load();
        }

        static public void playMoveMenu() {
            moveMenu.play(globalTarget);
        }
        static public void playEnterMenu() {
            enterMenu.play(globalTarget);
        }
        static public void playCancelMenu() {
            cancelMenu.play(globalTarget);
        }
    }
}
