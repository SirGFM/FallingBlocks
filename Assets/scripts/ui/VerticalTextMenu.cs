using UiText = UnityEngine.UI.Text;

public class VerticalTextMenu : VerticalMenu {
    public UiText shadow;
    public UiText unselected;
    public UiText selected;

    protected string[] options;

    override protected int getNumberOfOptions() {
        return this.options.Length;
    }

    override protected void updateSelected() {
        string txt = "";

        for (int i = 0; i < this.options.Length; i++) {
            if (i == this.getCurrentOpt())
                txt += $"-- {this.options[i]} --\n";
            else
                txt += "\n";
        }

        selected.text = txt;
    }

    override protected void start() {
        string txt = "";

        foreach (string opt in this.options)
            txt += $"{opt}\n";

        shadow.text = txt;
        unselected.text = txt;

        base.start();
    }
}
