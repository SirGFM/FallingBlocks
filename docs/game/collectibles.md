Although there's still no collectible in this game, the game's architecture is generic enough to account for that.

The way checkpoints were implement could be used as a base for other things the player (or other entities) may interact with.

# Checkpoints

Checkpoints are quite similar to other objects:

```
Checkpoint object
 |_ Model controller
 |   |_ Actual model
 |_ Relative collision parent
     |_ Bottom Relative collision
     |_ Center Relative collision
```

However, different from other objects, the *main object* **doesn't have a collider** of its own. Therefore, the checkpoint may detect when other objects pass through it (since it has a **center** relative collision detector), but other objects ignore it completely.

The **bottom** relative collision detector is used only during object instantiation. As soon as it detects another object, it parents the checkpoint to that other object, essentially making the checkpoint follow whatever is exactly bellow it.
