using Audio = UnityEngine.AudioSource;
using CoroutineRet = System.Collections.IEnumerator;

public class LoopedSong : UnityEngine.MonoBehaviour {
    public Audio FirstPass;
    public Audio Looped;

    static private bool hasSource = false;

    private CoroutineRet stopNonLooped() {
        while (!this.FirstPass.isPlaying)
            yield return null;
        yield return new UnityEngine.WaitForSeconds(1.0f);

        this.Looped.volume = this.FirstPass.volume;
        this.FirstPass.volume = 0.0f;
        yield return null;
        this.FirstPass.Stop();
    }

    void Awake() {
        if (LoopedSong.hasSource) {
            Destroy(this.gameObject);
            return;
        }
        LoopedSong.hasSource = true;

        DontDestroyOnLoad(this.gameObject);

        this.StartCoroutine(this.stopNonLooped());
    }
}
