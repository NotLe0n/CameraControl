# Camera Control

Camera Control is the perfect tool for making showcase, cinematic or youtube videos!
It allows you to move the camera along a predefined path, lock the camera to a fixed point 
or make the camera follow an NPC, Item or Projectile.

If you have any questions, issues or bugs contact me on my [discord server](https://discord.gg/NX4DVzz2v6")

This mod has been split of from the ["Better Zoom"](https://steamcommunity.com/sharedfiles/filedetails/?id=2562953970) mod. If you're looking for a mod to just zoom in/out more, check it out.

## How to use
To use this mod you need to enter **Editor mode** by pressing the Keybind, which you have to set first.
In **Editor mode**, you can freely move the camera with W, A, S or D and zoom in and out using the mouse wheel.
The "Show View Range" button will draw a box to show what the player will see at 100% zoom.

You'll also see the UI at the bottom of the screen in **Editor mode**.<br>
![](https://i.imgur.com/Vv2IOWd.png)

Currently you are in **Select mode** - to start placing tracks enter **Drawing mode**, by clicking on one of the two "Place curve" buttons.
![draw](https://i.imgur.com/J4JrVo9.png)

You can select between Bézier and (Catmul-Rom) Spline curves.
The difference is that Bézier curves will create a smooth curve, where the points of the curve don't have to hit all control points.
In comparison, a Spline curve will go through all control points.<br>
![comp](https://i.imgur.com/OH6pxNH.png)

After selecting a curve, start drawing by clicking and dragging the mouse to the desired location. 
When you're done placing, go back to **Select mode**, by clicking on the currently selected curve type
and move the control points to create the curve you want.<br>
![control](https://i.imgur.com/BTBQkeU.gif)

You can erase curves by clicking on "Erase curve". You will enter **Eraser mode** where clicking on a curve will remove it.
To remove all curves at once, click on "Delete all curves".

Now we have a track the camera will follow. Press the Play button to start or resume. 
Press it again and the camera will pause at it's current location along the curve.<br>
![play](https://i.imgur.com/LISgnhz.gif)

There are a few settings for what will happen at the end. The default behavior is stopping at the end. 
If you toggle "Repeat", the camera will go back to the start. 
If you toggle "Bounce", the camera will track backwards towards the start and then start again.<br>
![endsettings](https://i.imgur.com/yIjclws.gif)

While the camera is tracking, a progress bar will show you the percentage to competion. 
You can quckly scrub through the curve by holding left click and dragging the scroll bar.<br>
![progress](https://i.imgur.com/VNwCdeQ.gif)

To change the speed, use the Speed up and Speed down buttons. This will double or halve the speed of the previous keyframe. 
This will get us to the Keyframe system.<br>
![changespeed](https://i.imgur.com/BvP8SoJ.png)

### Keyframes
There is always at least one Keyframe present - at the very start. Keyframe are shown on the progress bar as red rectangles.
To create a keyframe right click the progress bar at the desired location. To remove a keyframe, right click the keyframe.<br>
![keyframes](https://i.imgur.com/WWdYLej.gif)

The tracker will change speed when it passes over a keyframe.

### Track Enities
Click on the "Track Entity" button and then select the entity, by clicking on its hitbox (highlighted in green). To stop tracking, hit the button again.
You can also cancel the selection process, by clicking on the button again while the hitboxes are shown.<br>
![trackentities](https://i.imgur.com/NpQpV9B.gif)

### Lock camera
You can lock the camera with the "Lock camera" button. The camera will be locked to the current editor camera location, once you leave **Editor mode**.<br>
![lockcam](https://i.imgur.com/cpV8UcF.gif)

## Hotkeys
There are hotkeys for:
* Play/Pause
* Toggle Repeat
* Toggle Bounce
* Lock Screen
You have to set them to a key to use them.
