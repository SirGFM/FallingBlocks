using EvSys = UnityEngine.EventSystems;
using Time = UnityEngine.Time;
using Vec3 = UnityEngine.Vector3;

public interface ShakeController : EvSys.IEventSystemHandler {
    /** Signal the object to start shaking */
    void StartShaking();

    /** Signal the object to stop shaking */
    void StopShaking();

    /** Retrieve the ShakeController receiver */
    void GetShakeComponent(GetComponentControllerParam param);
}

public class Shake : BaseRemoteAction, ShakeController {
    /** How long the object should move in a single direction */
    private const float step = 0.075f;

    /** Referece to the object's transform */
    private UnityEngine.Transform self;
    /** Whether the object should keep shaking */
    private bool running;

    void Start() {
        this.self = this.transform;
        this.running = false;
    }

    private System.Collections.IEnumerator shake() {
        while (this.running) {
            Vec3 nextPos = new Vec3();
            for (int i = 0; i < 3; i++) {
                int rand = (Global.PRNG.fastInt() % 21) - 10;
                nextPos[i] = 0.0125f * (float)rand;
            }

            for (float t = 0; t < Shake.step; t += Time.fixedDeltaTime) {
                this.self.Translate(nextPos * (t / Shake.step));
                yield return new UnityEngine.WaitForFixedUpdate();
                /* XXX: Reset to neutral before next shake */
                this.self.Translate(-nextPos * (t / Shake.step));
            }
        }
    }

    public void StartShaking() {
        if (this.running)
            return;
        this.running = true;
        this.StartCoroutine(this.shake());
    }

    public void StopShaking() {
        this.running = false;
    }

    public void GetShakeComponent(GetComponentControllerParam param) {
        param.obj = this.gameObject;
    }
}
