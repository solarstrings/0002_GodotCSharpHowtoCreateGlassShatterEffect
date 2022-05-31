using Godot;


public class Main : Node2D
{
    private bool _glassIsSmashed = false;
    public override void _Process(float delta)
    {
        if(Input.IsActionJustPressed("ui_accept"))
        {
            if(!_glassIsSmashed)
            {
                _glassIsSmashed = true;
                GetNode<ShatterGlass>("Sprite/ShatterGlass")?.SmashGlass();
                GetNode<AudioStreamPlayer>("AudioStreamGlassBreak").Play();
            }
        }         
    }
}
