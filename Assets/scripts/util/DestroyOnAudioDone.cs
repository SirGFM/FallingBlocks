using AudioSource = UnityEngine.AudioSource;

public class DestroyOnAudioDone  : UnityEngine.MonoBehaviour {
    private AudioSource src;

    void Start() {
        src = this.gameObject.GetComponent<AudioSource>();
    }

    void Update() {
        if (!src.isPlaying)
            Destroy(this.gameObject);
    }
}
