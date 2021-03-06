
public class LoseScene : WinLoseScene {
    protected override void onJustPressed() {
        this.rootEvent<Loader>( (x,y) => x.ReloadLevel() );
    }

    protected override void playOpeningSfx() {
        Global.Sfx.playDefeatOpening();
    }

    protected override void playSfx() {
        Global.Sfx.playDefeat();
    }
}
