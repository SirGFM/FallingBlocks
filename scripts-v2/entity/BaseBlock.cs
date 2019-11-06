using RelPos = RelativeCollision.RelativePosition;

public class BaseBlock : BaseEntity {
    override protected void start() {
        base.start();

        RelPos[] positions = {RelPos.BottomLeft, RelPos.BottomRight, RelPos.FrontBottom, RelPos.BackBottom};
        this.setCollisionDownCallback(positions);
    }
}
