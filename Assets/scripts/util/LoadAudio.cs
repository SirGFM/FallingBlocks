using AudioClip = UnityEngine.AudioClip;

public class LoadAudio : UnityEngine.MonoBehaviour {
    public AudioClip moveMenu;
    public AudioClip enterMenu;
    public AudioClip cancelMenu;
    public AudioClip pushBlock;
    public AudioClip longPushBlock;
    public AudioClip blockLand;
    public AudioClip blockShake;
    public AudioClip playerTurning;
    public AudioClip playerMoving;
    public AudioClip playerClimbBlock;
    public AudioClip playerWalkDownBlock;
    public AudioClip playerLand;
    public AudioClip playerDeath;
    public AudioClip playerFalling;
    public AudioClip playerMoveLedge;
    public AudioClip playerCantPush;
    public AudioClip crackedBlockCrack;
    public AudioClip crackedBlockBreak;
    public AudioClip checkpoint;
    public AudioClip victoryStart;
    public AudioClip victory;
    public AudioClip defeatStart;
    public AudioClip defeat;

    void Start() {
        Global.Sfx.setup(this);
    }
}
