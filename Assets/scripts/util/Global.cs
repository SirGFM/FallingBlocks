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

            public Sound(string src) {
                this.ctor(src, Variability.None, null);
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

        static private Sound moveMenu = new Sound(",0.3,,0.1307,,0.0713,0.3,0.5174,,,,,,,,,,,,,0.5394,,,,,1,,,0.1,,,,masterVolume",
                                                  Sound.Variability.Low);
        static private Sound enterMenu = new Sound(",0.3,,0.1696,,0.1923,0.3,0.3811,,,,0.1,0.505,,,,,,,,0.4415,,,,,1,,,0.1,,,,masterVolume",
                                                  Sound.Variability.Medium);
        static private Sound cancelMenu = new Sound(",0.3,,0.1696,,0.1923,0.3,0.3811,,-0.1949,,0.25,0.505,,,,,,,,0.4415,,,,,1,,,0.1,,,,masterVolume",
                                                  Sound.Variability.Medium);
        static private Sound pushBlock = new Sound("3,0.3,,0.495,0.7592,0.2271,,0.02,,0.0449,-0.0999,,,,,,,,,,,,,,,1,,,,,,,masterVolume");
        static private Sound longPushBlock = new Sound("3,0.3,,0.69,0.7592,0.305,,0.02,,-0.045,-0.14,,,,,,,,,,,,,,,1,,,,,,,masterVolume");
        static private Sound blockLand = new Sound("3,0.23,,0.12,0.2354,0.305,0.3,0.11,,-0.2099,,,,,,,0.1566,0.6301,,,,,,0.5037,-0.0249,1,,,,,,,masterVolume");
        static private Sound blockShake = new Sound("3,0.3,,0.405,0.7592,0.33,0.045,0.055,,0.1,-0.0999,0.22,0.525,,,,,,,,,,,0.5049,,1,,,,,,,masterVolume");
        static private Sound playerTurning = new Sound(",0.23,,0.1,0.7392,0.21,0.2,0.22,,0.3049,,,,,,,-0.4399,0.6068,,,,,0.3975,,,1,,,,,,,masterVolume");
        static private Sound playerMoving = new Sound(",0.23,,0.12,0.715,0.295,0.2,0.2,,-0.305,,,,,,,-0.4399,0.6068,,,,,0.3975,,,1,,,,,,,masterVolume");
        static private Sound playerClimbBlock = new Sound(",0.23,,0.2,0.7392,0.21,0.2,0.12,,0.3049,,,,,,,-0.4399,0.6068,,,,,0.3975,,,1,,,,,,,masterVolume");
        static private Sound playerWalkDownBlock = new Sound(",0.23,,0.22,0.7392,0.21,0.2,0.255,,-0.175,,,,,,,-0.4399,0.6068,,,,,0.3975,,,1,,,,,,,masterVolume");
        static private Sound playerLand = new Sound("5,0.23,,0.12,0.2354,0.195,0.3,0.11,,-0.2099,,,,,,,0.1566,0.6301,,,,,,0.5037,-0.0249,1,,,,,,,masterVolume");
        static private Sound playerDeath = new Sound("3,0.3,,0.405,,0.305,0.3,0.737,,-0.4705,0.36,,,,,,,,,,,,,,,1,,,,,,,masterVolume");
        static private Sound playerFalling = new Sound("11,0.1,,0.305,0.2188,0.405,0.3,0.745,0.2,0.12,-0.155,,,,,,,,,,0.4018,-0.6769,,,,1,,,0.2596,,,,masterVolume");
        static private Sound playerMoveLedge = new Sound("1,0.2,,0.1565,,0.195,0.3,0.32,,0.1399,0.2299,,,,,,,,,,0.3241,,,,,1,,,0.0421,,,,masterVolume");
        static private Sound playerCantPush = new Sound("1,0.22,,0.415,,0.0142,0.3,0.24,,,,0.1,0.255,,,,,,,,,,,,,1,,,0.1,,,,masterVolume");
        static private Sound crackedBlockCrack = new Sound("3,0.22,,0.1636,0.4881,0.25,0.155,0.1631,,0.2564,,,,,,,0.4452,0.761,,,,,0.7005,0.0847,-0.146,1,,,,,,,masterVolume");
        static private Sound crackedBlockBreak = new Sound("9,0.3,,0.065,0.255,0.58,0.155,0.7245,,-0.3572,,,,,,,,,,,,,0.7329,,,1,,,,,,,masterVolume");
        static private Sound checkpoint = new Sound("2,0.22,,0.0875,,0.4398,0.3,0.4003,,0.1399,,0.5356,0.4473,,,,,,,,0.0057,,,,,1,,,,,,,masterVolume");
        static private Sound victoryStart = new Sound("2,0.22,,0.24,,0.2,0.3,0.35,,0.2299,,0.5356,0.4473,,,,,,,,0.0057,,,,,1,,,,,,,masterVolume");
        static private Sound victory = new Sound("2,0.22,,0.155,,0.525,0.3,0.4003,,0.195,,0.5356,0.4473,,,,,,,,0.0057,,,,,1,,,,,,,masterVolume");
        static private Sound defeatStart = new Sound("2,0.22,,0.24,,0.2,0.3,0.35,,-0.265,0.155,0.5356,0.4473,,,,,,,,0.0057,,,,,1,,,,,,,masterVolume");
        static private Sound defeat = new Sound("2,0.22,,0.155,,0.525,0.3,0.4003,,-0.1949,,0.5356,0.4473,,,,,,,,0.0057,,,,,1,,,,,,,masterVolume");

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
            pushBlock.load();
            longPushBlock.load();
            blockLand.load();
            blockShake.load();
            playerTurning.load();
            playerMoving.load();
            playerClimbBlock.load();
            playerWalkDownBlock.load();
            playerLand.load();
            playerDeath.load();
            playerFalling.load();
            playerMoveLedge.load();
            playerCantPush.load();
            crackedBlockCrack.load();
            crackedBlockBreak.load();
            checkpoint.load();
            victoryStart.load();
            victory.load();
            defeatStart.load();
            defeat.load();
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
        static public void playPushBlock(float delay) {
            if (delay < 1.0f)
                pushBlock.play(globalTarget);
            else
                longPushBlock.play(globalTarget);
        }
        static public void playPullBlock(float delay) {
            if (delay < 1.0f)
                pushBlock.play(globalTarget);
            else
                longPushBlock.play(globalTarget);
        }
        static public void playPlayerTurning() {
            playerTurning.play(globalTarget);
        }
        static public void playPlayerCrushed() {
            playerDeath.play(globalTarget);
        }
        static public void playPlayerMoving() {
            playerMoving.play(globalTarget);
        }
        static public void playPlayerClimbBlock() {
            playerClimbBlock.play(globalTarget);
        }
        static public void playPlayerWalkDownBlock() {
            playerWalkDownBlock.play(globalTarget);
        }
        static public void playPlayerClimbLedge() {
            playerClimbBlock.play(globalTarget);
        }
        static public void playPlayerDropToLedge() {
            playerWalkDownBlock.play(globalTarget);
        }
        static public void playPlayerMoveLedge() {
            playerMoveLedge.play(globalTarget);
        }
        static public void playPlayerFalling() {
            playerFalling.play(globalTarget);
        }
        static public void playPlayerLand() {
            playerLand.play(globalTarget);
        }
        static public void playEnterCrackedBlock() {
            crackedBlockCrack.play(globalTarget);
        }
        static public void playExitCrackedBlock() {
            crackedBlockCrack.play(globalTarget);
        }
        static public void playBreakCrackedBlock() {
            crackedBlockBreak.play(globalTarget);
        }
        static public void playBlockLanded() {
            blockLand.play(globalTarget);
        }
        static public void playBlockShaking() {
            blockShake.play(globalTarget);
        }
        static public void playPlayerCantPush() {
            playerCantPush.play(globalTarget);
        }
        static public void playCheckpoint() {
            checkpoint.play(globalTarget);
        }
        static public void playVictoryOpening() {
            victoryStart.play(globalTarget);
        }
        static public void playVictory() {
            victory.play(globalTarget);
        }
        static public void playDefeatOpening() {
            defeatStart.play(globalTarget);
        }
        static public void playDefeat() {
            defeat.play(globalTarget);
        }
    }
}
