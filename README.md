# Shadster's Avatar Tools [Download](https://github.com/Shadsterwolf/ShadsterAvatarTools/releases)
Shadsterwolf's avatar tools, mostly used for personal use and subject to change. <br />
<b>Intended for Unity Editor to support VRChat Avatar models</b>
- Most button functions will be hard changes done to your avatar and files, use at your own risk!!!
- Theres still missing debug/message feedback, but once the button is pressed the code executes. If the console shows no errors, it probably worked.

<p align="center"> 
<img src="https://github.com/Shadsterwolf/ShadstersAvatarTools/blob/master/Preview1_2_0.png?raw=true">
</p>

# Instructions
1. With a project open with VRCSDK (from VRChat Content Creator)
2. Import my provided released package
3. In Unity, on your toolbar, click "ShadsterWolf" > "Shadster Tools"

(Pro-tip, use Alt+S to bring up the toolbar selection, S again to open the tool window)

# Features
- Quick Sceneview and Playmode speed up toggles
- Quick settings of mesh boundaries and anchor probes
- Quick setup for Gogo Loco (after import): https://franadavrc.gumroad.com/l/gogoloco
- Quick export preparation of Prefab backup and clearing Blueprint ID 
- Quick Physbone setup
- Quick Generation of Animation Renders and Shapekeys
- Quick Toggle and BlendTree setup
- Quick Gogo Loco Setup

# Functions
Main:
- Auto-Detect - Get the selected or first vrc avatar in the scene
- Reset-All - Clear Avatar tool references
- Start Play Mode in Scene View - Switch play mode to stay in Scene view without the need to create a seperate window
- Use Experimental Play Mode - Quickly get into play mode faster
- Ignore Physbone Immobile - Temporarily set physbone immobile world values to zero when testing physics
- Test All Avatar Physbones - Temporarily animate the avatar in play mode testing physics

Common:
- Fix Avatar Descriptor - Attempts to fix an issue when resetting rig back to Humanoid and the descriptor loses Face/Body references
- Set All Mesh Bounds to 2.5 - All mesh boundaries will be set to equal 2.5 measure (Fixes mesh culling issues when viewing avatar from certain angles)
- Set All Anchor Probes to Hip - All mesh anchor overrides will be set to hip (Fixing lighting issues when standing between light probes)
- Clear Avatar Blueprint ID - Clears Blueprint ID on the Avatar

Textures:
- Enable/Disable All MipMaps - Set mipmap flag to all textures assigned to active materials being used. (ONLY DISABLE MIPMAPS IF TEXTURES HAVE ISSUES FROM A DISTANCE)
- Set All Max Size 1k/2k/4k - Set texture max size dimentions 1024/1024, 2048/2048, 4096/4096 (Unity default is 2k)
- Set Compression LQ/NQ/HQ - Set texture compression quality Low, Normal, High (Unity default is normal compression)

Bones:
- Move PhysBones from Armature - Moves physbones off Armature, assinging the source object, and to the parent object.
- Move Colliders from Aramature - Moves physbone colliders off Armature, assinging the source object, and to the parent object.
- Set All Grab Movement to 1 - All physbone interact grab movement setting will be set to 1
- Repair Missing Physbone Transforms - Attempt to find the armature name that is the same as the physbone's object name 

Animation: 
- Generate Animation Render Toggles - Find all meshes and generate animation files On/Off by game object, stored in "...\Animations\Generated\Toggles"
- Generate Animation Shapekeys - Find all meshes and their shapekeys, combine objects keys with the same name, generate animation files 0/100, stored in "...\Animations\Generated\Shapekeys". Any shapekeys with "Emote_" will be stored in "...\Animations\Generated\Emotes"
- Generate Animation Physbones - Generate some common physbone control settings
- Generate Emote Override Menu - Generate a menu with emotes with each emote inside "...\Animations\Generated\Emotes"

Gogo Loco:
- Setup Prefab - Copies "Beyond Prefab" to Avatar hierarchy and assigns constraint source to head
- Setup Layers - Updates Avatar Descriptor's Playable layers except FX and unchecks "Force Locomotion animations for 6 point tracking
- Setup Menu - Adds Gogo Loco menu to your main menu
- Setup Params - Adds Gogo's parameters to your VRC parameters (and deletes the old previous version ones too)
- Setup FX - Copies the two fly layers over to your current FX layer (adds the controller parameters too) 

Scene:
(Context - Usually "Assets/Avatar", labels the location that the buttons within Scene will influence)
- Setup Menus/Params - Copy off the VRCSDK3 sample menu and params to the folder, attaches to the avatar
- Setup FX Controller - Copy off the VRCSDK3 sample FX controller to the folder, attaches to the avatar
- Regenerate GUIDs - Creates a new folder with a 1 "Avatar1" re-writing new GUIDs (CAUTION, few reference IDs might be missed and will need to manaully be repointed)
- Cleanup - Reads every scene within context,  delete any leftover prefabs from this tool
- Fix - Applies all fixes within "Common" to all scenes and avatars with vrc descriptor
- Export - Exports everything in the context folder and Gogo Loco to a unitypackage file
- Compile - Takes the Blender Zip path and the exported Unity Package, zips both into one file.

# Credits
Made by ShadsterWolf

Some code inspired by the VRCSDK, PumkinsAvatarTools, and Av3Creator
