
public class VerticalMenu : Menu {
    private int curOpt;

    virtual protected int getNumberOfOptions() {
        return 0;
    }

    protected int getCurrentOpt() {
        return this.curOpt;
    }

    virtual protected void updateSelected() {
    }

    override protected void onDown() {
        this.curOpt++;
        if (this.curOpt >= this.getNumberOfOptions())
            this.curOpt = 0;
        this.updateSelected();
    }

    override protected void onUp() {
        this.curOpt--;
        if (this.curOpt < 0)
            this.curOpt = this.getNumberOfOptions() - 1;
        this.updateSelected();
    }

    override protected void start() {
        this.curOpt = 0;
        this.updateSelected();
    }
}
