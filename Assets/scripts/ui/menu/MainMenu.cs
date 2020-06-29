using App = UnityEngine.Application;
using Color = UnityEngine.Color;
using Image = UnityEngine.UI.Image;

public class MainMenu : VerticalTextMenu {
    private string[] _opts = {
        "New game",
        "Level select",
        "Options",
        "Quit"
    };

    public Image[] spriteSelector;

    private void updateSpriteSelector(int old, int _new) {
        this.spriteSelector[old].color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
        this.spriteSelector[_new].color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        Config.setPlayerModel(_new);
    }

    override protected void onLeft() {
        int old = Config.getPlayerModel();
        int _new = old - 1;
        if (_new < 0)
            _new = this.spriteSelector.Length - 1;
        this.updateSpriteSelector(old, _new);
    }

    override protected void onRight() {
        int old = Config.getPlayerModel();
        int _new = old + 1;
        if (_new >= this.spriteSelector.Length)
            _new = 0;
        this.updateSpriteSelector(old, _new);
    }

    override protected void onSelect() {
        switch (this.getCurrentOpt()) {
        case 0:
            this.LoadLevel(1);
            break;
        case 1:
            this.LoadScene("scenes/000-game-controller/LevelSelect");
            break;
        case 2:
            this.LoadScene("scenes/000-game-controller/Options");
            break;
        case 3:
            App.Quit();
            break;
        }
    }

    static private bool first = true;
    override protected void start() {
        if (first) {
            Config.load();
            first = false;
        }

        this.options = this._opts;
        this.CombinedLoadScene("scenes/000-game-controller/bg-scenes/MainMenu");
        base.start();

        this.updateSpriteSelector(0, PlayerModel.active);
        Global.setup();
    }
}
