# Using events

The following options are available to sending messages between game objects:

* `GameObject.SendMessage`: Specifies the method by its name (a string) and pass parameters on a object; Calls the method on every `MonoBehaviour` in the game object (i.e., **do not** propagates up/downward);
* `GameObject.SendMessageUpwards`: Similar to `GameObject.SendMessage`, but **propagates upward** (it does not specify when it stops propagating);
* `GameObject.BroadcastMessage`: Similar to `GameObject.SendMessage`, but **propagates downward** (it does not specify when it stops propagating);
* `ExecuteEvents.Execute`: Executes a `IEventSystemHandler` event on all components of a given game object that can handle it (i.e., **do not** propagates up/downward); **Seems to have been removed after 2019.1 (even though it was never deprecated)...**
* `ExecuteEvents.ExecuteHierarchy`: Similar to `ExecuteEvents.Execute`, but propagates upwards until there's a GameObject that can handle the event; **Seems to have been removed after 2019.1 (even though it was never deprecated)...**

In summary:

* `GameObject.SendMessage`: Only hits **components in the game object itself**
* `GameObject.SendMessageUpwards`: Every **parent (but not its siblings) and the game object itself**
* `GameObject.BroadcastMessage`: Every **child and the game object itself**
* `ExecuteEvents.Execute`: Only hits **components in the game object itself**
* `ExecuteEvents.ExecuteHierarchy`: Hits the **first** receiver between **parents and the game object itself**

# Sending events downwards

When using function within `ExecuteEvents`, there's no way to send messages downwards. Two ways of bypassing this limitations are:

1. Pass the caller to the event, and reply with an event starting on the caller (limited)
2. Set an "event proxy" at the game object's root, which calls `GameObject.BroadcastMessage` whenever it receives an event (ugly, but get the job done)
