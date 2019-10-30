# Detecting collision with colliders

Collision detection comes in two flavors in Unity3D:

* Trigger-based: Detects that two colliders are overlapping; without **phisically separating the objects**
* Collision-based: Detects that two colliders are touching each other **and push them apart**

If a game object has multiple colliders, however, there's no way (afaik) to automatically tell which of the colliders were involved in the collision. You must manually associate each collider (most likely from its `GetInstanceID()`) to something that identifies the colliders (e.g., an enum).

Another option is to have each collider be in a separated child game object, which sends a custom message upward identifying the collision.

# Moving colliders

Unity3D differentiate between static and dynamic colliders:

* Static collider: collider **without** an attached rigidbody that **won't move at all**
* Rigidbody collider: collider **with** an attached rigidbody that moves only by means of Unity's **built-in physics**
* Kinematic rigidbody collider: collider **with** an attached rigidbody that moves by manually integrating the object position/rotation/etc

It's possible to make an object a **mix of kinematic and non-kinematic**, by modifying the object's rigidbody. Doing so, one could, for example, **move the object manually but have Unity's physics handle the gravity**.

The table bellow (extracted from Unity's Manual) describes which collisions are valid, and which type of message is sent in each case:

|| Static Collider | Rigidbody Collider | Kinematic Rigidbody Collider | Static Trigger Collider | Rigidbody Trigger Collider | Kinematic Rigidbody Trigger Collider |
| --- | --- | --- | --- | --- | --- | --- |
| Static Collider                      |   | C |   |   | T | T |
| Rigidbody Collider                   | C | C | C | T | T | T |
| Kinematic Rigidbody Collider         |   | C |   | T | T | T |
| Static Trigger Collider              |   | T | T |   | T | T |
| Rigidbody Trigger Collider           | T | T | T | T | T | T |
| Kinematic Rigidbody Trigger Collider | T | T | T | T | T | T |

Where:

* `C`: Collision messages (e.g., `OnCollisionEnter()`)
* `T`: Trigger messages (e.g., `OnTriggerEnter()`)

## Children with colliders

Each child that has a collider must have an attached rigidbody as well (otherwise, that child would have a static collider).

By setting that rigidbody as kinematic, the parent may be set to non-kinematic and the **parent's physics will affect the child as well**.
