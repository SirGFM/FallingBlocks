In Unity3D, there are two main types of containers:

* Game Object: Anything that may be placed in the engine, and that may be specialized by adding components; Can also hold child game objects
* Scene: Container for game objects

Then, it's possible to bring these into the game in one of three ways:

* Adding objects to the active scene (which Unity will automatically load when the scene starts)
* Calling `GameObject.Instantiate()` to spawn a new game object dynamically
* Calling `SceneManager.LoadScene*()` to load a new scene

# Loading scenes dynamically

Depending on the `LoadSceneMode` passed to `LoadScene*()`, the new scene will either be loaded into the current scene (using `LoadSceneMode.Additive`) or it will replace the current scene (using `LoadSceneMode.Single`).

However, there's no way to change a scene's origin. So, if every scene was composed around (0,0) and they are later moved into the correct place, they will **overlap each other in the frame they finish loading**. A simple, but annoying, way to go around this limitation is to have the play are be really far away from the scene's origin, and **message the scene with the proper origin as soon as it finishes loading**.

If the scene has a single root game object, which every other game object is a child of, that is displaced, the following would return the object to the origin as soon as the scene finishes loading:

```C#
using AsyncOp = UnityEngine.AsyncOperation;
using GO = UnityEngine.GameObject;
using Scene = UnityEngine.SceneManagement.Scene;
using SceneMng = UnityEngine.SceneManagement.SceneManager;
using SceneMode = UnityEngine.SceneManagement.LoadSceneMode;

/* Call as a coroutine to load a new scene */
private System.Collections.IEnumerator load(string scene) {
    /* Register a listener when the scene finishes loading */
    SceneMng.sceneLoaded += OnSceneLoaded;

    /* Load the scene in background and wait until it finishes */
    yield return SceneMng.LoadSceneAsync(scenes, SceneMode.Additive);

    /* Remove the listener */
    SceneMng.sceneLoaded -= OnSceneLoaded;
}

void OnSceneLoaded(Scene scene, SceneMode mode) {
    foreach (GO go in scene.GetRootGameObjects()) {
        if (go.name != "root")
            continue;
        UnityEngine.Transform t = go.transform;
        t.Translate(-1 * t.position);
    }   
}   
```

## Physics of the loaded scene

On the first frame, `FixedUpdate()` will get called before any collision detection is done. However, collision detection should have run before `Update()`. This follows the behaviour described in section "Order of Execution for Event Functions" of the manual.

`On*Enter()` seems to be correctly called even if objects where loaded in the scene already touching each other. However, **`*Collision*` events are only called for non-kinematic objects.**
