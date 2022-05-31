using Godot;
using System.Collections.Generic;

public class ShatterGlass : Node2D
{
    [Export(PropertyHint.Range, "0,50")]
    public int NumGlassShardsPoints = 10;       // Number of glass shard points
    [Export]
    public float TriangleEdgeThreshold = 10.0f; // Threshold for creating triangles close to the sprite edges
    [Export]
    public float MinShatterForce = 150.0f;      // The minimum glass shatter force
    [Export]
    public float MaxShatterForce = 800.0f;      // The maximum glass shatter force
    [Export]
    public float ForceMultiplier = 10.0f;       // Force multiplier
    [Export]
    public float GlassShardLifetime = 3.0f;     // The glass shards lifetime (in seconds)
    [Export]
    public float GlassShardOverlap = -5;        // How much the glass shards can overlap
    private PackedScene _glassShardScene;       // The glass shard scene

    // Triangle list - Used to create the glass shards
    private List<Vector2> _triangleList = new List<Vector2>();

    // Glass shard list
    private List<RigidBody2D> _glassShardList = new List<RigidBody2D>();
    
    public override void _Ready()
    {
        // Load the glass shard scene
        _glassShardScene = GD.Load<PackedScene>("res://Effects/ShatterGlass/GlassShard.tscn");   
        if(_glassShardScene == null)
        {
            GD.Print("Hello");
        }
        SetupShatterGlassEffect();     
    }
    private void SetupShatterGlassEffect()
    {
		List<Vector2> points = new List<Vector2>();
        if(GetParent() is Sprite)
        {
            var parentNode = GetParent() as Sprite;     // Get the parent node    
            var rect = parentNode.GetRect();            // Get the parent node rectangle
            AddSpriteCorners(points,rect);              // Add the sprite corner points         
            AddRandomBreakPoints(points,rect);          // Add the break points
            CalculateGlassShardTriangles(points);       // Calculate triangles
            CreateRigidBody2DShards(parentNode);        // Create RigidBody2D shards
            CallDeferred("AddGlassShards");             // Add the glass shards
        }
    }
    private void AddGlassShards()
    {
        // Loop throgh the list of glass shards
	    foreach(var glassShard in _glassShardList)
        {
            // Add the glass shard to the parent node
		    GetParent().AddChild(glassShard);
        }
    }

    private void ResizeGlassShardTriangles(RigidBody2D glassShard, int triangleListIndex)
    {
        // create the polygon from the triangle list
        Vector2[] polygon = new Vector2[3];
        polygon[0] = _triangleList[triangleListIndex];
        polygon[1] = _triangleList[triangleListIndex+1];
        polygon[2] = _triangleList[triangleListIndex+2];

        // Setup the polygon
        glassShard.GetNode<Polygon2D>("Polygon2D").Polygon = polygon;
        
        // Resize the glass shards so they won't overlap
        var resizedTriangles = Geometry.OffsetPolygon2d(polygon, GlassShardOverlap);
        if(resizedTriangles.Count > 0)
        {
            glassShard.GetNode<CollisionPolygon2D>("CollisionPolygon2D").Polygon = (Vector2[])resizedTriangles[0];
        }
        else
        {
            glassShard.GetNode<CollisionPolygon2D>("CollisionPolygon2D").Polygon = polygon;
        }
    }
    private Vector2 GetGlassShardTriangleCenter(int triangleListIndex)
    {
        int t = triangleListIndex;  // Create a shorter variable for the triangle list index (less typing)
        // Get the x and y center point
        double xVal = (_triangleList[t].x + _triangleList[t+1].x + _triangleList[t+2].x)/3.0;
        double yVal = (_triangleList[t].y + _triangleList[t+1].y + _triangleList[t+2].y)/3.0;
        // Return the center point
        return new Vector2((float)xVal,(float)yVal);
    }    
    private void CreateRigidBody2DShards(Sprite parentNode)
    {
        // Loop through each triangle in the triangle list
        for(int t=0; t < _triangleList.Count;t+=3)
        {
            var glassShard = _glassShardScene.Instance() as RigidBody2D;     // Create a new instance of the glass shard scene
            ResizeGlassShardTriangles(glassShard, t);                        // Resize the glass shards
            glassShard.Position = GetGlassShardTriangleCenter(t);            // Set shard position to the glass shard center
            glassShard.Hide();                                               // Hide the glass shard
            _glassShardList.Add(glassShard);                                 // Add the glass shard to the glass shard list

            // Set the position for the polygon and the collision polygon
            glassShard.GetNode<Polygon2D>("Polygon2D").Position = -GetGlassShardTriangleCenter(t);
            glassShard.GetNode<CollisionPolygon2D>("CollisionPolygon2D").Position = -GetGlassShardTriangleCenter(t);

            // Set the texture of the polygon to the same as the parent node texture
            glassShard.GetNode<Polygon2D>("Polygon2D").Texture = parentNode.Texture;            
        }        
    }    
    private void CalculateGlassShardTriangles(List<Vector2> points)
    {
        // Trianglulate the point array
        var array = Geometry.TriangulateDelaunay2d(points.ToArray());

        // loop through the array
        for(int i = 0; i < array.Length; i += 3)
        {
            // Add the three triangle points to the triangle list
            _triangleList.Add(points[array[i + 2]]);
            _triangleList.Add(points[array[i + 1]]);
            _triangleList.Add(points[array[i]]);
        }
    }    
    private void AddRandomBreakPoints(List<Vector2> points,Rect2 spriteRect)
    {
        // Create a random number generator
        RandomNumberGenerator rnd = new RandomNumberGenerator();
        rnd.Randomize();
        
        for(int i=0; i < NumGlassShardsPoints; ++i)
        {
            // Randomize a new break point
            var breakPoint = spriteRect.Position +  new Vector2(rnd.RandfRange(0, spriteRect.Size.x), 
                                                                rnd.RandfRange(0, spriteRect.Size.y));
            
            // The code below snaps the point to and edge if it's within the triangle edge threshold
            if(breakPoint.x < spriteRect.Position.x + TriangleEdgeThreshold)
            {
                breakPoint.x = spriteRect.Position.x;
            }
            else if(breakPoint.x > spriteRect.End.x - TriangleEdgeThreshold)
            {
                breakPoint.x = spriteRect.End.x;
            }
            if(breakPoint.y < spriteRect.Position.y + TriangleEdgeThreshold)
            {
                breakPoint.y = spriteRect.Position.y;
            }
            else if(breakPoint.y > spriteRect.End.y - TriangleEdgeThreshold)
            {
                breakPoint.y = spriteRect.End.y;
            }
            // Add the point
            points.Add(breakPoint);
        }
    }    
    private void AddSpriteCorners(List<Vector2> points, Rect2 spriteRect)
    {
        // Add sprite corners to the points list
        points.Add(spriteRect.Position);
        points.Add(spriteRect.End);        
        points.Add(spriteRect.Position + new Vector2(spriteRect.Size.x, 0));
        points.Add(spriteRect.Position + new Vector2(0, spriteRect.Size.y));
    }    

    private void HideParentSprite()
    {
        // Get the parent node
        var parentNode = GetParent() as Sprite;
        // Get the parents self modulate color
        Color newColor = parentNode.SelfModulate;
        // Set alpha to 0 (invisible)
        newColor.a = 0.0f;
        // Update the parent node modulate with the alpha set to 0
        parentNode.SelfModulate = newColor;
    }    
    private void FadeOutGlasShard(RigidBody2D glassShard)
    {
        var tween = glassShard.GetNode<Tween>("Tween");     // Get the tween node
        Color targetColor = glassShard.Modulate;            // Get the current modulate color
        targetColor.a = 0.0f;                               // Set the target color alpha to 0 (transparent)

        // Interpolate the modulate alpha property value down to zero at the speed of the glass shard life time
        tween.InterpolateProperty(glassShard,"modulate",glassShard.Modulate,targetColor,GlassShardLifetime,Tween.TransitionType.Linear,Tween.EaseType.In);
        tween.Start();
    }    
    private void OnLifeTimerTimeout()
    {
        // When the life timer hit zero, free the parent node from memory
        GetParent().QueueFree();        
    }
    public void SmashGlass()
    {
        // Create a new random number generator
        RandomNumberGenerator rnd = new RandomNumberGenerator();
        rnd.Randomize();

        // Hide the parent sprite
        HideParentSprite();

        // Loop through all the glass shards in the list
        foreach(var glassShard in _glassShardList)
        {
            var impulseDirection = Vector2.Up.Rotated(rnd.RandfRange(0, 2.0f * Mathf.Pi));          // Set the direction of the impulse
            var shatterForce = rnd.RandfRange(MinShatterForce, MaxShatterForce);                    // Randomize a shatter force for the glass shard
            glassShard.ApplyCentralImpulse((impulseDirection * shatterForce) * ForceMultiplier);    // Apply central impulse 
            glassShard.GetNode<CollisionPolygon2D>("CollisionPolygon2D").Disabled = false;          // Enable collision for the glass shard
            glassShard.Show();                                                                      // Show the glass shard
            FadeOutGlasShard(glassShard);                                                           // Fade out the glass shard
        }
        // Start the life timer for the glass shards
        GetNode<Timer>("LifeTimer").Start(GlassShardLifetime);
    }    
//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
