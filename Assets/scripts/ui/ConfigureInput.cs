using CoroutineRet = System.Collections.IEnumerator;
using UiButton = UnityEngine.UI.Button;
using UiText = UnityEngine.UI.Text;

public class ConfigureInput : UnityEngine.MonoBehaviour {
    private UiText label;
    private UnityEngine.Coroutine bgFunc;

    public Input.Actions action;
    public int column;

    private CoroutineRet WaitTimeout() {
        for (int time = 5; time >= 0; time--) {
            this.label.text = $"Waiting input... {time}...";

            for (int i = 0; i < 100; i++) {
                yield return new UnityEngine.WaitForSeconds(0.01f);

                if (!Input.IsWaitingInput()) {
                    /* Done waiting! */
                    string axisName;
                    axisName = Input.AxisName(this.action, this.column);
                    this.label.text = axisName;
                    this.StopCoroutine(this.bgFunc);
                }
            }
        }

        this.label.text = "";
        Input.CancelWaitInput();
        Input.ClearAxis(this.action, this.column);
    }

    private void OnClick() {
        if (Input.IsWaitingInput())
            return;

        this.label.text = "Waiting input...";

        this.bgFunc = this.StartCoroutine(this.WaitTimeout());
        Input.WaitInput(this.gameObject, this.column, this.action);
    }

    void Start() {
        UiButton bt = this.GetComponentInChildren<UiButton>();
        bt.onClick.AddListener(this.OnClick);

        this.label = this.GetComponentInChildren<UiText>();
        this.label.text = Input.AxisName(this.action, this.column);
    }
}
