Adds a Time.timeScale slider and a reset button to the Unity main toolbar.

Requires Unity 6.3 or newer, which is where the UnityEditor.Toolbars API was introduced.

Configure it by right-clicking the slider or the reset button:

  Forced Override    Toolbar value takes priority over game logic
  Max Scale          Upper bound of the slider (2, 5, 10, 100)

Unity supplies its own entry in that same menu for hiding the element.

While Forced Override is off, changes made to the timeScale from the game's logic take
priority and the slider follows them.

Based on TimeScale Toolbar by Paul Berne. See LICENSE.
