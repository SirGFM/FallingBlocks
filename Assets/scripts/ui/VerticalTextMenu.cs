using UiText = UnityEngine.UI.Text;

public class VerticalTextMenu : Menu {
    public UiText shadow;
    public UiText unselected;
    public UiText selected;

    protected string[] options;

    private int curOpt;

    protected int getCurrentOpt() {
        return this.curOpt;
    }

    private void updateSelected() {
        string txt = "";

        for (int i = 0; i < this.options.Length; i++) {
            if (i == this.curOpt)
                txt += $"-- {this.options[i]} --\n";
            else
                txt += "\n";
        }

        selected.text = txt;
    }

    override protected void onDown() {
        this.curOpt++;
        if (this.curOpt >= this.options.Length)
            this.curOpt = 0;
        this.updateSelected();
    }

    override protected void onUp() {
        this.curOpt--;
        if (this.curOpt < 0)
            this.curOpt = this.options.Length - 1;
        this.updateSelected();
    }

    override protected void start() {
        string txt = "";

        this.curOpt = 0;

        foreach (string opt in this.options)
            txt += $"{opt}\n";

        shadow.text = txt;
        unselected.text = txt;

        this.updateSelected();
    }
}
