# Base collision detection

Most of the game logic (and thus its collision detection) is based on checking adjacent objects. Because of this, almost every object has a `TrackSurroundings` (used to receive and track `On*RelativeCollision` events) and a number of objects with a `RelativeCollision` (to report back to the `TrackSurroundings`).

To simplify collision detection, these objects are organized in two different layers:

* **detector**: Layer used by objects that have a `RelativeCollision` component. Can only collide with **tracker** (or semantically similar).
* **tracker**: Layer used by objects that have a `TrackSurroundings` component. Can only collide with **detector** (or semantically similar).

## Entity specific collision

The player and the minions may detect when they are colliding (i.e., overlapping) with specific entities (e.g., a checkpoint). In this case, another layer shall be used, but this layer must be configured at least as the **detector** (but colliding with more layers).

# Base entity architecture

Most entities in the game are organized as:

```
Main game object
 |_ Model controller
 |   |_ Actual model
 |   |_ ...
 |_ Relative collsion parent
     |_ Relative collision 1 (e.g., left)
     |_ Relative collision 2 (e.g., right)
     | ...
     |_ Relative collision N
```

* **Main game object**: Contain most of the game component behaviours
* **Model controller**: Wrapper for the model, with components that control how the model is displayed (e.g., shaking it in place)
* **Actual model**: One or more objects used to render the object's model
* **Relative collsion parent**: Parent to every relative collision in the object. Should have a single `EaseSetRelativeCollision` component, which **copies the object's layer into its children**
* **Relative collision N**: Each of the object's relative collision detector. Should be created mainly by the parent object

