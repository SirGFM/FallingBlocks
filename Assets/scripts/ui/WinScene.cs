
public class WinScene : WinLoseScene {
    protected override void onJustPressed() {
        this.rootEvent<LoaderEvents>( (x,y) => x.NextLevel() );
    }

    protected override void playOpeningSfx() {
        Global.Sfx.playVictoryOpening();
    }

    protected override void playSfx() {
        Global.Sfx.playVictory();
    }
}
