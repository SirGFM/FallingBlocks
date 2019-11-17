public class CaptureOnAnimationFinished : BaseRemoteAction {
    public void OnAnimationFinished() {
        UnityEngine.GameObject parent = this.transform.parent.gameObject;

        this.issueEvent<Goal>( (x,y) => x.AnimationFinished(), parent );
    }
}
