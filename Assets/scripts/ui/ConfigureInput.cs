using UiButton = UnityEngine.UI.Button;
using UiText = UnityEngine.UI.Text;

public class ConfigureInput : UnityEngine.MonoBehaviour {
    private UiText label;
    private bool waiting;

    public Input.Actions action;
    public int column;

    private void OnClick() {
        if (Input.IsWaitingInput())
            return;

        this.label.text = "Waiting input...";

        this.waiting = true;
        Input.WaitInput(this.gameObject, this.column, this.action);
    }

    void Start() {
        UiButton bt = this.GetComponentInChildren<UiButton>();
        bt.onClick.AddListener(this.OnClick);

        this.label = this.GetComponentInChildren<UiText>();
        this.label.text = Input.AxisName(this.action, this.column);
        this.waiting = false;
    }

    void Update() {
        if (!this.waiting)
            return;

        if (!Input.IsWaitingInput()) {
            /* Done waiting! */
            this.label.text = Input.AxisName(this.action, this.column);
        }
    }
}
