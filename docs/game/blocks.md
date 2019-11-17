Most blocks behave essentially the same: the can be pushed, at various speeds and they fall if there's nothing bellow (although it's possible to have floating blocks).

For these, most of the logic is similar and shared, implemented in a few components. However, there are a few special kind of blocks that must be signaled whenever something walks over them.

# Basic block architecture

Blocks have the same basic organization as other entities:

```
Block object
 |_ Model controller
 |   |_ Actual model
 |_ Relative collision parent
     |_ Relative collision 1 (e.g., left)
     |_ Relative collision 2 (e.g., right)
     | ...
     |_ Relative collision N
```

For the relative collision, blocks need to keep track of **neighbouring** objects and objects **bellow**, including objects bellow and on an adjacent square.

The neighbouring objects are used to check if there's anything nearby that will be **pushed** alongside the block, and the get the slowest moving speed (or if it will move at all). Meanwhile, objects bellow are used to halt falling.

To avoid crushing the player when they push a block that's holding the block directly above the player, a block will delay its fall if it detects that its last supporting block was pushed away. However, if the supporting block merely fell (regardless of whether or not its fall was delayed), then the block will fall without any extra delay.

# Top tracking blocks

Some blocks need to keep track of when something stepped on top of it, and react to it in some way.

To do that, these blocks have an extra **relative collider** tracking its **top**. This collider is assigned a custom callback, different for each block.

Currently, these are the blocks that react to objects above it:

* Cracked block
* Goal block
* Minion goal block

# Ice block

Different from other *top tracking blocks*, ice block doesn't actually act any differently than other blocks. However, these have the **Type `IceBlock`**.

Thus, when an entity finishes moving, **the entity itself** checks whether its on top of an ice block and keeps moving forward (if moving in the same direction as facing and if there's nothing in front of it).
