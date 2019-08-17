using GO = UnityEngine.GameObject;
using Vec3 = UnityEngine.Vector3;
using Math = UnityEngine.Mathf;

public class CameraController : UnityEngine.MonoBehaviour {
    private const string cameraTag = "MainCamera";

    private new UnityEngine.Transform camera;
    private UnityEngine.Transform self;
    private Vec3 globalOffset;

    private float distance = 7.0f;
    private float baseDY = 6.0f;
    private float baseDZ = -17.0f;

    private void findCamera() {
        GO[] cam = GO.FindGameObjectsWithTag(cameraTag);
        if (cam.Length == 1)
            this.camera = cam[0].transform;
    }

    void Start() {
        this.camera = null;
        this.self = this.transform;
        this.findCamera();
    }

    void Update() {
        if (this.camera == null) {
            this.findCamera();
            return;
        }

        Vec3 v = new Vec3();
        v.x = 0f;
        v.y = this.baseDY;
        v.z = this.baseDZ;
        v = v.normalized * this.distance;

        if (this.self.position.y >= this.globalOffset.y)
            this.globalOffset = this.self.position;
        else {
            this.globalOffset.x = this.self.position.x;
            this.globalOffset.z = this.self.position.z;
        }
        this.globalOffset.x *= 1.25f;

        Vec3 localOffset = new Vec3();
        float offY = UnityEngine.Input.GetAxis("CameraY");
        float absOffY = Math.Abs(offY);
        if (offY < 0) {
            /* Moving camera up, looking down */
            localOffset.z = this.baseDZ * absOffY * -0.3f;
            localOffset.y = absOffY * 4.0f;
        }
        else {
            /* Moving camera down, looking up */
            localOffset.z = this.baseDZ * absOffY * -0.35f;
            localOffset.z += absOffY * -5.0f;
            localOffset.y = this.baseDY * -absOffY;
        }
        localOffset.x = UnityEngine.Input.GetAxis("CameraX") * 2.0f;

        this.camera.position = v + this.globalOffset + localOffset;
        this.camera.LookAt(this.self);
    }
}
