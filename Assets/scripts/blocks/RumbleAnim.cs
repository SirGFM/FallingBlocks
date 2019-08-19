using EvSys = UnityEngine.EventSystems;
using Time = UnityEngine.Time;
using Vec3 = UnityEngine.Vector3;

public interface Rumbler : EvSys.IEventSystemHandler {
    /** Signal the object to start rumbling */
    void StartRumbling();

    /** Signal the object to stop rumbling */
    void StopRumbling();
}

public class RumbleAnim : UnityEngine.MonoBehaviour, Rumbler {
    /** How long the object should move in a single direction */
    private const float step = 0.075f;

    /** Referece to the object's transform */
    private UnityEngine.Transform self;
    /** Whether the object should keep rumbling */
    private bool running;

    void Start() {
        this.self = this.transform;
        this.running = false;
    }

    private System.Collections.IEnumerator rumble() {
        while (this.running) {
            Vec3 nextPos = new Vec3();
            for (int i = 0; i < 3; i++) {
                int rand = (Global.PRNG.fastInt() % 21) - 10;
                nextPos[i] = 0.0125f * (float)rand;
            }

            for (float t = 0; t < RumbleAnim.step; t += Time.fixedDeltaTime) {
                this.self.Translate(nextPos * (t / RumbleAnim.step));
                yield return new UnityEngine.WaitForFixedUpdate();
                /* XXX: Reset to neutral before next rumble */
                this.self.Translate(-nextPos * (t / RumbleAnim.step));
            }
        }
    }

    public void StartRumbling() {
        if (this.running)
            return;
        this.running = true;
        this.StartCoroutine(this.rumble());
    }

    public void StopRumbling() {
        this.running = false;
    }
}
