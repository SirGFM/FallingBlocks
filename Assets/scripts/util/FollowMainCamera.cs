using GO = UnityEngine.GameObject;

public class FollowMainCamera : FollowObject {
    override protected GO getFollowed() {
        UnityEngine.Camera mc = UnityEngine.Camera.main;
        if (mc == null)
            return null;
        else
            return mc.gameObject;
    }
}
