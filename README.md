This is a version for Manomotion Lite 0.3 + ARCore sample project.
The repo allows developers to simulate hand input in Manomotion terms in Unity Editor without deploying to device.

__Note__:
- 1) Attach InteractionPointsExample to ManoMotionCanvas to see the pointer gizmo
- 2) Clone `ManoMotionCanvas`.`cursor`, rename it to `HandIcon` and put it inside the `cursor` game object, set scale to 4, 4, 4

Controls:
* Mouse coords for palm center
* Mouse wheel for depth

* LMB down for Pinch state 13 (closed)
* LMB up then for Pinch state 0 (opened)

* LeftCtrl + LMB down for Pinch and CLICK
* LeftAlt + LMB down for Pinch PICK trigger
* LeftAlt + LMB up for Pinch DROP trigger

RMB - is the same for GRAB
MMB - same for POINTER

TODO:
* Extract InteractionPointsExample to a separate script
* Automate the steps from Notes creating cursor and hand icon objects
* Add a branch for Lite 0.3 (w/o ARCore)
