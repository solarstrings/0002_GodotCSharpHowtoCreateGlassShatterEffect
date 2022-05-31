# Godot C# How-to #2: How to Create a Glass Shatter Effect

Godot project of how to create a glass shatter effect that works on any sprite.

Usage:
Add a sprite to any scene. 
Make sure the sprite is not centered - or the effect wont work.
   Click on hte sprite.
   In the inspector, Expand Offset
   Find the Centered[] checkbox & make sure it's not checked
   
Attach the ShatterGlass scene to any sprite node, create a script and call the SmashGlass() 
method on the ShatterGlass node, to make the sprite node break like glass.

e.g. A script attached to the parent node of the Sprite:
	GetNode<ShatterGlass>("Sprite/ShatterGlass")?.SmashGlass();
	
e.g. A script attached to the sprite node itself:
	GetNode<ShatterGlass>("ShatterGlass")?.SmashGlass();
	
Note: The sprite the script is attached to will be deleted once the shatter effect has completed.
