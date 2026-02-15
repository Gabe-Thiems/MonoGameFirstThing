//using System.Numerics;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FirstGame;

public class Game1 : Game
{
    public List<PhysicsGameObject> sceneObjects = new List<PhysicsGameObject>();
    public const int gravityForce = 1;
    public const float playerScale = 2f;
    public const int pixelScale = 64;

    public int userGraphicsHeight = 1080;
    public int userGraphicsWidth = 1920;

    Texture2D computerTexture;
    Texture2D recTexture;
    Vector2 playerPosition = new Vector2();

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    public PhysicsGameObject playerObj;
    public PhysicsGameObject rectangleObject;

    public class PhysicsGameObject
    {
        public Texture2D texture;
        public Vector2 position;
        public int weight;
        public bool isForceAffected;
        public Vector2 velocity;
        //ashdhas
        public float collider_width;
        public float collider_height;
        public bool collider_isSolid;
        public string tag;

        public bool isOnGround;

        public Vector2 topLeftCorner
        {
            get
            {
                return this.position;
            }
            set
            {
                this.position = value;
            }

        }


        public Vector2 bottomRightCorner
        {
            get
            {
                return this.position + new Vector2(collider_width, collider_height);
            }
        }

        public PhysicsGameObject(Texture2D texture, Vector2 position, int weight, bool isForceAffected, Vector2 velocity, float collider_width, float collider_height, bool collider_isSolid, string tag, bool isOnGround )
        {
            this.texture = texture;
            this.position = position;
            this.weight = weight;
            this.isForceAffected = isForceAffected;
            this.velocity = velocity;
            this.collider_width = collider_width;
            this.collider_height = collider_height;
            this.collider_isSolid = collider_isSolid; 
            this.tag = tag;
            this.isOnGround = isOnGround;
        }

        public void ApplyForce(Vector2 direction, float intensity)
        {
            velocity = direction * intensity;
        }

        public PhysicsGameObject checkForCollisions(List<PhysicsGameObject> colliders)
        {
            foreach (PhysicsGameObject gameObject in colliders)
            {
                if (gameObject.tag == tag) {continue;} //Stop hitting yourself (collisions with self ignored)

                //AHhhhhhhhhhhhh
                if (!(bottomRightCorner.X < gameObject.topLeftCorner.X || // You are left of other
                    topLeftCorner.X > gameObject.bottomRightCorner.X || // You are right of other
                    bottomRightCorner.Y < gameObject.topLeftCorner.Y || // You are below
                    topLeftCorner.Y > gameObject.bottomRightCorner.Y))   // You are above
                {
                    return gameObject;
                }

            }
            return null;
        }
        
    }

    
    
    public void Physics(List<PhysicsGameObject> objects)
    {
        float dragForce = 0.9f;
        float threshold = 0.25f;

        foreach (PhysicsGameObject gameObject in objects)
        {
            if (!gameObject.isOnGround)
            {
                gameObject.velocity.Y += gravityForce;
            }

            if (!gameObject.isForceAffected) {gameObject.velocity = Vector2.Zero;}
        }

        foreach (PhysicsGameObject gameObject in objects)
        {
            gameObject.isOnGround = false;
            PhysicsGameObject collided = gameObject.checkForCollisions(objects);
            if (collided != null)
            {
                Vector2 difference = new Vector2((collided.position.X - gameObject.position.X), (collided.position.Y - gameObject.position.Y));
                
                if (-0.1 < difference.Y && difference.Y < 0.1)
                {
                    gameObject.isOnGround = true; //TODO: Instant response COLLISION
                    gameObject.position.Y = collided.position.Y;
                    gameObject.velocity.Y = 0;
                }
                
            }

            
        }
        foreach (PhysicsGameObject gameObject in objects)
        {
            if ((Math.Abs(gameObject.velocity.X) > 0) || (Math.Abs(gameObject.velocity.Y) > 0)) //If has had force applied, move and reduce force
            {
                gameObject.position += gameObject.velocity;
                gameObject.velocity *= dragForce;

                if (Math.Abs(gameObject.velocity.X) < threshold) {gameObject.velocity.X = 0; }
                if (Math.Abs(gameObject.velocity.Y) < threshold) {gameObject.velocity.Y = 0; }
                
            }
        }
    }

    public Game1()
    {

        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here
        _graphics.PreferredBackBufferWidth = userGraphicsWidth;
        _graphics.PreferredBackBufferHeight = userGraphicsHeight;
        //_graphics.IsFullScreen = true;

        _graphics.ApplyChanges();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here
        computerTexture = Content.Load<Texture2D>("ComputerBoy-1");
        recTexture = Content.Load<Texture2D>("Rectangle");

        playerObj = new PhysicsGameObject(
            computerTexture, 
            playerPosition, 
            50, 
            true, 
            Vector2.Zero,
            (pixelScale * playerScale),
            (pixelScale * playerScale),
            true,
            "player",
            false
            );
        
        rectangleObject = new PhysicsGameObject(
            recTexture,
            new Vector2(50, 800),
            0,
            false,
            Vector2.Zero,
            (pixelScale * 1f), //TODO: FIX, WAYYY Too big, use player as reference -- DONE?
            (pixelScale * 0.5f),
            true,
            "rectangle",
            false
            );
        
        sceneObjects.Add(playerObj);
        sceneObjects.Add(rectangleObject);
    }

    protected override void Update(GameTime gameTime)
    {

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)) { Exit(); }
        if (Keyboard.GetState().IsKeyDown(Keys.D)) { playerObj.velocity.X += 1;}
        if (Keyboard.GetState().IsKeyDown(Keys.A)) { playerObj.velocity.X -= 1;}
        if (Keyboard.GetState().IsKeyDown(Keys.S)) { playerObj.velocity.Y += 1;}
        if (Keyboard.GetState().IsKeyDown(Keys.W)) { playerObj.velocity.Y -= 1;}
        if (Keyboard.GetState().IsKeyDown(Keys.Space)) {playerObj.velocity.Y += 5;}

        Console.WriteLine(playerObj.velocity);

        Physics(sceneObjects);
        // TODO: Add your update logic here

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();
        _spriteBatch.Draw(playerObj.texture, playerObj.position, null, Color.White, 0f, Vector2.Zero, playerScale, SpriteEffects.None, 0f);
        _spriteBatch.Draw(recTexture, rectangleObject.position, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        _spriteBatch.End();
       

        // TODO: Add your drawing code here

        base.Draw(gameTime);
    }
}