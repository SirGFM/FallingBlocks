public class DebugGizmo : UnityEngine.MonoBehaviour {
    public UnityEngine.Color color = UnityEngine.Color.white;
    public float radius = 0.5f;

    void OnDrawGizmos() {
        UnityEngine.Gizmos.color = this.color;
        UnityEngine.Gizmos.DrawWireSphere(this.transform.position, this.radius);
    }
}
