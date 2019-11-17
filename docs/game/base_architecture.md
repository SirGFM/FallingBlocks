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
 |_ Relative collision parent
     |_ Relative collision 1 (e.g., left)
     |_ Relative collision 2 (e.g., right)
     | ...
     |_ Relative collision N
```

* **Main game object**: Contain most of the game component behaviours
* **Model controller**: Wrapper for the model, with components that control how the model is displayed (e.g., shaking it in place)
* **Actual model**: One or more objects used to render the object's model
* **Relative collision parent**: Parent to every relative collision in the object. Should have a single `EaseSetRelativeCollision` component, which **copies the object's layer into its children**
* **Relative collision N**: Each of the object's relative collision detector. Should be created mainly by the parent object

## Inter-Component Communication

All components communicate with each other through messages. In most cases, simply sending the message to the current object and all of its parents will suffice. However, in a few cases, the main component will need to send messages downward, to specific components.

The solution in these cases was to add a `Get<ComponentName>Component(out GameObject obj)` to these components (that must receive a downward message), and send this message using `GameObject.BroadcastMessage`. After acquiring a reference to the object, all other messages are sending by calling `issueEvent` and passing that reference.


# Scripts organization

The scripts are organized into a few directories, each for a specific type of components:

## `base`

Scripts that deal with the game's custom collision detection and messaging system.

These got their own separated directory since they are the base for the entire game.

## `movement`

Event-based scripts that controls entities in some way. Most of these scripts define two interfaces:

* `*Controller`: Interface **implemented** by the script itself. Contains the events used to signal the script to do some action.
* `*Detector`: Interface **used** by the script to report the current state of its action.

Most of these components should be placed directly into the main game object. However, some specific components (like `Shake` and `Turn`) may be placed in a sub-object.

## `entity`

Scripts that control entities. To simplify implementing similar components, there are three basic components:

* `BaseEntity`: Most generic entity implementation, should be inherited by every entity. Implements moving, turning and falling.
* `BaseBlock`: Basic block; inherits `BaseEntity`. Customize falling (so it's delayed).
* `BaseAnimatedEntity`: Basic mobile entity; inherits `BaseEntity`. Implements various functions to enable moving the entity around blocks.

## `level`

Scripts related to level logic. All levels are loaded by the single component `Loader`, which keeps track of many events: initial player position, the various checkpoints thorough the level, how to load the next level etc.
