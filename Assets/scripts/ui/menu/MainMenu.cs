using App = UnityEngine.Application;

public class MainMenu : VerticalTextMenu {
    private string[] _opts = {
        "New game",
        "Level select",
        "Options",
        "Quit"
    };

    override protected void onSelect() {
        switch (this.getCurrentOpt()) {
        case 0:
            this.LoadLevel(1);
            break;
        case 1:
            this.CombinedLoadScene("scenes/000-game-controller/LevelSelect");
            break;
        case 2:
            this.CombinedLoadScene("scenes/000-game-controller/Options");
            break;
        case 3:
            App.Quit();
            break;
        }
    }

    override protected void start() {
        this.options = this._opts;
        base.start();
    }
}
