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
    }

    override protected void onLeft() {
        int old = PlayerModel.active;
        PlayerModel.active--;
        if (PlayerModel.active < 0)
            PlayerModel.active = this.spriteSelector.Length - 1;
        this.updateSpriteSelector(old, PlayerModel.active);
    }

    override protected void onRight() {
        int old = PlayerModel.active;
        PlayerModel.active++;
        if (PlayerModel.active >= this.spriteSelector.Length)
            PlayerModel.active = 0;
        this.updateSpriteSelector(old, PlayerModel.active);
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

    override protected void start() {
        this.options = this._opts;
        this.CombinedLoadScene("scenes/000-game-controller/bg-scenes/MainMenu");
        base.start();

        this.updateSpriteSelector(0, PlayerModel.active);
        Global.setup();
    }
}
