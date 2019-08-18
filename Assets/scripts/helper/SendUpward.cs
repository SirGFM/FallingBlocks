using SMO = UnityEngine.SendMessageOptions;

/** Component for working around Unity's limitation of not being able to send
 * events upward from an animation */
public class SendUpward : UnityEngine.MonoBehaviour {
    private void send(string msg) {
        this.transform.parent.SendMessageUpwards(msg, SMO.RequireReceiver);
    }

    public void OnAnimationFinished() {
        this.send("OnAnimationFinished");
    }
}
