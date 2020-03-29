using GO = UnityEngine.GameObject;
using Vec3 = UnityEngine.Vector3;

public class ScaledCopyTransform : UnityEngine.MonoBehaviour {
    private UnityEngine.Transform pTransform;
    private UnityEngine.Transform selfTransform;
    public GO parent;
    public Vec3 positionOffset = new Vec3(0.0f, 0.0f, 0.0f);
    public Vec3 positionFactor = new Vec3(1.0f, 1.0f, 1.0f);
    public Vec3 rotationFactor = new Vec3(1.0f, 1.0f, 1.0f);

    void Start() {
        this.pTransform = this.parent.GetComponent<UnityEngine.Transform>();
        this.selfTransform = this.GetComponent<UnityEngine.Transform>();
    }

    void Update() {
        Vec3 newPos = new Vec3();
        Vec3 newRot = new Vec3();
        for (int i = 0; i < 3; i++) {
            newPos[i] = this.positionOffset[i];
            newPos[i] += this.pTransform.position[i] * this.positionFactor[i];

            newRot[i] = this.pTransform.eulerAngles[i] * this.rotationFactor[i];
        }

        this.selfTransform.position = newPos;
        this.selfTransform.eulerAngles = newRot;
    }
}
