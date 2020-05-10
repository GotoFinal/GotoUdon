# GotoUdon
A pack of editor scripts that helps with testing vrchat udon scripts without launching games.  
Currently still in-dev, but basic functionality can be already used:  
- Using player join/leave events.
- Invoking basic interactions events from editor, or clicking on object in game window.
- Basic player api functionality, you can change player walk speed and track object to selected bone and check in editor if its working correctly.
- Player tags.
- Automatically building and uploading a map and then starting predefined list of VRChat profiles.

Unsupported stuff that will be added soon:
- Look ray cast.
- Is grounded. (currently always true)
- Simulating actual pickup interactions between user and object.
- More events that can be called with simple button press.
- Object ownership.
- Operation on server time (implemented, but might return completely different values than in vrchat)
- Tracking data. (currently it just return same data as asking for bones, probably completly invalid)
- Ability to actually move when in play mode like in 3rd person game.

Some of unsupported stuff can be actually already simulated by manually assigning objects to scripts added to objects at runtime, 
but its not really usable and not tested. But each player contains "SimulatedVRCPlayer" component where you can see.  
Other functions might be simulated but not tested if they works similar enough to vrchat, and are just blindly implemented to not crash. 

Plans for future:
- Ability to use VR when testing?
- Basic vrchat physics simulation, at least to not do complete opposite of what vrchat is doing.

## How to use
Go to [releases](https://github.com/GotoFinal/GotoUdon/releases) page and download newest one and just drop it to your assets like any other unity package.  
Then you can access new menus:  
![new menu](https://i.imgur.com/OUv7deJ.png)

If you want to use simulated players its important to provide some avatar prefab and spawn point, if you already have a 
scene descriptor a tool will try to copy a spawn point from there at first launch.  
![menu](https://i.imgur.com/Rb7gZMm.png)  
Any changes to this window are saved, so once you add few player templates you will not need to do it again after launching project again.  
For avatar prefab I recommend using included ybot mini.  

Then you can just add more players or change some setting, later when in play mode this window will change and display currently connected people:  
![runtime menu](https://i.imgur.com/XRZTv1r.png)  
You can use this menu to either editor your players, add new ones, or validate that your scripts changed desired valued,
like walk speed or made player immobile, as all custom properties of players are displayed in these boxes.  
Changes made here are not saved, this includes adding new players.

Additional when you launch play mode this script will attach new component to every object that have udon behaviour components.  
This component allows you to call some basic events with simple click, you can also click on object in game window to
cause interaction event to be executed.  
![debugger](https://i.imgur.com/76BeNMc.png)  
(More events will be added soon, note that some events like player join can be simulated using features described above)

![tool](https://i.imgur.com/T3wrlqu.gif)

Additionaly you can also test your map in vrchat using multiple clients with just one simple click:  
![clients](https://i.imgur.com/iMWdBoO.png)  
Just add few random clients with some profile number (0 is your default one) and then it will start multiple VRChat instances, each one with separate account, you can login to your account and next time you will use this feature all your accounts will be already logged in!

## Limitations
This tool can't simulate networking, so you can't use it to see if other players will see valid state of game.  
Sadly this will probably be never implemented, as its way too complicated to be possible to add this to unity editor tool, 
without actually launching multiple instances of unity.  

## How to help
You can help me by either just contributing to this repository with new features, 
or by sending me example udon maps/scenes I can import in unity and test how the tool acts in unity vs in game.  

## Contact
If you need help you can Join to discord:  
[![Discord](https://img.shields.io/badge/Discord-My%20Discord%20Server-blueviolet?logo=discord)](https://discord.gg/B8hbbax) - For support and bug reports.
