
public class LoseScene : WinLoseScene {
    protected override void onJustPressed() {
        this.rootEvent<Loader>( (x,y) => x.ReloadLevel() );
    }
}
