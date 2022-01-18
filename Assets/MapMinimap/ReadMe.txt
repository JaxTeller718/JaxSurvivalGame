
Thank you for downloading Minimap and Maps!
 
To get started, you may do the following:
1. Look at the demo scenes in the Scenes folder
2. Watch the tutorial videos on Youtube: https://www.youtube.com/channel/UC0LYig0AgPT9T5IN5DCUiqQ
3. Read the PDF doc in this folder.
4. Join the discord to ask questions: https://discord.gg/JpVwUgG
5. If you still need help, you may contact me at: contact@indiemarc.com


Integration with Survival Engine:
1) Import Survival Engine first (v1.10+)
2) Import Map and Minimaps
3) If there are no compile errors, the "Scripting Define Symbol" MAP_MINIMAP should be added automatically to the player settings. If not add it manually. 
4) Make sure the MapManager prefab is in the scene, and that you add the MapIcon script to the PlayerCharacter (Set its type to "Player").
5) Add a MapZone to your scene, and setup the zone to contain the map area, then use MapCapture to generate a template map sprite.
6) Add the MapLevelSettings script to your scene, and set it's properties. Then everything should work!

Integration with InControl:
1) Import both assets
2) If there are no compile errors, the "Scripting Define Symbol" IN_CONTROL should be added automatically to the player settings. If not, add it manually.
3) Connect a USB gamepad such as Xbox One controller, and run the game!

Integration with the New Input System Gamepad
1) Import Map Minimap
2) Install Unity Input System in the package manager
3) In BuildSettings->PlayerSettings, set the Active Input Handling to Both
4) Add to the project the script InputSystemWrap.cs (available on Indie Marc discord)
5) Attach the script to the Manager prefabs (Prefabs/MapManager)

