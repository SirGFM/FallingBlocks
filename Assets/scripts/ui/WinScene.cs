
public class WinScene : WinLoseScene {
    protected override void onJustPressed() {
        this.rootEvent<LoaderEvents>( (x,y) => x.NextLevel() );
    }
}
