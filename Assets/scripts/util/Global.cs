using AudioClip = UnityEngine.AudioClip;
using AudioSource = UnityEngine.AudioSource;
using GO = UnityEngine.GameObject;
using Transform = UnityEngine.Transform;

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
        private class SoundClip {
            private AudioClip src;

            public SoundClip(AudioClip _src) {
                this.src = _src;
            }

            public void play(Transform target) {
                AudioSource.PlayClipAtPoint(this.src, target.position);
            }
        }

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

            public void play(Transform target) {
                if (this.loaded) {
                    this.sfx.SetParentTransform(target);
                    this.sfx.PlayMutated(this.mutationAmount, this.mutationsNum);
                }
            }
        }

        static private SoundClip moveMenu;
        static private SoundClip enterMenu;
        static private SoundClip cancelMenu;
        static private SoundClip pushBlock;
        static private SoundClip longPushBlock;
        static private SoundClip blockLand;
        static private SoundClip blockShake;
        static private SoundClip playerTurning;
        static private SoundClip playerMoving;
        static private SoundClip playerClimbBlock;
        static private SoundClip playerWalkDownBlock;
        static private SoundClip playerLand;
        static private SoundClip playerDeath;
        static private SoundClip playerFalling;
        static private SoundClip playerMoveLedge;
        static private SoundClip playerCantPush;
        static private SoundClip crackedBlockCrack;
        static private SoundClip crackedBlockBreak;
        static private SoundClip checkpoint;
        static private SoundClip victoryStart;
        static private SoundClip victory;
        static private SoundClip defeatStart;
        static private SoundClip defeat;

        static private bool init = false;
        static private GO globalTargetObject = null;
        static private Transform globalTarget = null;

        static public void setup(LoadAudio sfx) {
            if (init)
                return;
            init = true;

            globalTargetObject = new GO();
            globalTargetObject.name = "Global Audio Player";
            globalTargetObject.AddComponent<UnityEngine.AudioListener>();
            globalTargetObject.AddComponent<AudioSourcePosition>();
            GO.DontDestroyOnLoad(globalTargetObject);
            globalTarget = globalTargetObject.transform;

            /* Load every sound, sequentially */
            moveMenu = new SoundClip(sfx.moveMenu);
            enterMenu = new SoundClip(sfx.enterMenu);
            cancelMenu = new SoundClip(sfx.cancelMenu);
            pushBlock = new SoundClip(sfx.pushBlock);
            longPushBlock = new SoundClip(sfx.longPushBlock);
            blockLand = new SoundClip(sfx.blockLand);
            blockShake = new SoundClip(sfx.blockShake);
            playerTurning = new SoundClip(sfx.playerTurning);
            playerMoving = new SoundClip(sfx.playerMoving);
            playerClimbBlock = new SoundClip(sfx.playerClimbBlock);
            playerWalkDownBlock = new SoundClip(sfx.playerWalkDownBlock);
            playerLand = new SoundClip(sfx.playerLand);
            playerDeath = new SoundClip(sfx.playerDeath);
            playerFalling = new SoundClip(sfx.playerFalling);
            playerMoveLedge = new SoundClip(sfx.playerMoveLedge);
            playerCantPush = new SoundClip(sfx.playerCantPush);
            crackedBlockCrack = new SoundClip(sfx.crackedBlockCrack);
            crackedBlockBreak = new SoundClip(sfx.crackedBlockBreak);
            checkpoint = new SoundClip(sfx.checkpoint);
            victoryStart = new SoundClip(sfx.victoryStart);
            victory = new SoundClip(sfx.victory);
            defeatStart = new SoundClip(sfx.defeatStart);
            defeat = new SoundClip(sfx.defeat);
        }

        static public void playMoveMenu() {
            if (moveMenu != null)
                moveMenu.play(globalTarget);
        }
        static public void playEnterMenu() {
            if (enterMenu != null)
                enterMenu.play(globalTarget);
        }
        static public void playCancelMenu() {
            if (cancelMenu != null)
                cancelMenu.play(globalTarget);
        }
        static public void playPushBlock(float delay, Transform target) {
            if (target == null)
                target = globalTarget;

            if (delay < 1.0f) {
                if (pushBlock != null)
                    pushBlock.play(target);
            }
            else {
                if (longPushBlock != null)
                    longPushBlock.play(target);
            }
        }
        static public void playPullBlock(float delay, Transform target) {
            if (target == null)
                target = globalTarget;

            if (delay < 1.0f) {
                if (pushBlock != null)
                    pushBlock.play(target);
            }
            else {
                if (longPushBlock != null)
                    longPushBlock.play(target);
            }
        }
        static public void playPlayerTurning(Transform target) {
            if (target == null)
                target = globalTarget;
            if (playerTurning != null)
                playerTurning.play(target);
        }
        static public void playPlayerCrushed(Transform target) {
            if (target == null)
                target = globalTarget;
            if (playerDeath != null)
                playerDeath.play(target);
        }
        static public void playPlayerMoving(Transform target) {
            if (target == null)
                target = globalTarget;
            if (playerMoving != null)
                playerMoving.play(target);
        }
        static public void playPlayerClimbBlock(Transform target) {
            if (target == null)
                target = globalTarget;
            if (playerClimbBlock != null)
                playerClimbBlock.play(target);
        }
        static public void playPlayerWalkDownBlock(Transform target) {
            if (target == null)
                target = globalTarget;
            if (playerWalkDownBlock != null)
                playerWalkDownBlock.play(target);
        }
        static public void playPlayerClimbLedge(Transform target) {
            if (target == null)
                target = globalTarget;
            if (playerClimbBlock != null)
                playerClimbBlock.play(target);
        }
        static public void playPlayerDropToLedge(Transform target) {
            if (target == null)
                target = globalTarget;
            if (playerWalkDownBlock != null)
                playerWalkDownBlock.play(target);
        }
        static public void playPlayerMoveLedge(Transform target) {
            if (target == null)
                target = globalTarget;
            if (playerMoveLedge != null)
                playerMoveLedge.play(target);
        }
        static public void playPlayerFalling(Transform target) {
            if (target == null)
                target = globalTarget;
            if (playerFalling != null)
                playerFalling.play(target);
        }
        static public void playPlayerLand(Transform target) {
            if (target == null)
                target = globalTarget;
            if (playerLand != null)
                playerLand.play(target);
        }
        static public void playEnterCrackedBlock(Transform target) {
            if (target == null)
                target = globalTarget;
            if (crackedBlockCrack != null)
                crackedBlockCrack.play(target);
        }
        static public void playExitCrackedBlock(Transform target) {
            if (target == null)
                target = globalTarget;
            if (crackedBlockCrack != null)
                crackedBlockCrack.play(target);
        }
        static public void playBreakCrackedBlock(Transform target) {
            if (target == null)
                target = globalTarget;
            if (crackedBlockBreak != null)
                crackedBlockBreak.play(target);
        }
        static public void playBlockLanded(Transform target) {
            if (target == null)
                target = globalTarget;
            if (blockLand != null)
                blockLand.play(target);
        }
        static public void playBlockShaking(Transform target) {
            if (target == null)
                target = globalTarget;
            if (blockShake != null)
                blockShake.play(target);
        }
        static public void playPlayerCantPush(Transform target) {
            if (target == null)
                target = globalTarget;
            if (playerCantPush != null)
                playerCantPush.play(target);
        }
        static public void playCheckpoint(Transform target) {
            if (target == null)
                target = globalTarget;
            if (checkpoint != null)
                checkpoint.play(target);
        }
        static public void playVictoryOpening() {
            if (victoryStart != null)
                victoryStart.play(globalTarget);
        }
        static public void playVictory() {
            if (victory != null)
                victory.play(globalTarget);
        }
        static public void playDefeatOpening() {
            if (defeatStart != null)
                defeatStart.play(globalTarget);
        }
        static public void playDefeat() {
            if (defeat != null)
                defeat.play(globalTarget);
        }
    }
}
