using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Devices.Sensors;
using Petzold.Phone.Xna;

namespace PingPong
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        const float GRAVITY = 1000;     // pixels per second squared
        const float BOUNCE = 1f;    // fraction of velocity
        const int BALL_RADIUS1 = 16;
        const int BALL_SCALE1 = 16;
        const int BALL_RADIUS2 = 32;
        const int BALL_SCALE2 = 32;
        //static int count = 30;

        const int HIT = 10;
        const int PENALTY = -1;
        Vector2 screenCenter;
        SpriteFont segoe96;
        int score;
        StringBuilder scoreText = new StringBuilder();
        Vector2 scoreCenter;

        //RenderTarget2D tinyTexture;
        //Vector2 tinyTextureCenter, tinyTexturePosition, tinyTextureVelocity = Vector2.Zero;
        //int highlightedSide;

        GraphicsDeviceManager graphics1;//graphics2;
        SpriteBatch spriteBatch1;//spriteBatch2;

        Viewport viewport;
        Texture2D ball1, ball2;
        Vector2 ballCenter1, ballCenter2;
        Vector2 ballPosition1, ballPosition2;
        Vector2 ballVelocity1 = Vector2.Zero;
        Vector2 ballVelocity2 = Vector2.Zero;
        Vector3 oldAcceleration1, acceleration1, oldAcceleration2, acceleration2;
        object accelerationLock1 = new object();
        object accelerationLock2 = new object();

        public Game1()
        {
            graphics1 = new GraphicsDeviceManager(this);
            //graphics2 = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            // Restrict orientation to portrait
            graphics1.SupportedOrientations = DisplayOrientation.Portrait;
            graphics1.PreferredBackBufferWidth = 480;
            graphics1.PreferredBackBufferHeight = 768;

            //graphics2.SupportedOrientations = DisplayOrientation.Portrait;
            //graphics2.PreferredBackBufferWidth = 480;
            //graphics2.PreferredBackBufferHeight = 768;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            Accelerometer accelerometer1 = new Accelerometer();
            accelerometer1.ReadingChanged += OnAccelerometer1ReadingChanged;

            try { accelerometer1.Start(); }
            catch { }

            base.Initialize();
        }

        void OnAccelerometer1ReadingChanged(object sender, AccelerometerReadingEventArgs args)
        {
            lock (accelerationLock1)
            {
                acceleration1 = 0.5f * oldAcceleration1 + 0.5f * new Vector3((float)args.X, (float)args.Y, (float)args.Z);
                oldAcceleration1 = acceleration1;
            }
            lock (accelerationLock2)
            {
                acceleration2 = 0.5f * oldAcceleration2 + 0.5f * new Vector3((float)args.X, (float)args.Y, (float)args.Z);
                oldAcceleration2 = acceleration2;
            }
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch1 = new SpriteBatch(GraphicsDevice);

            viewport = this.GraphicsDevice.Viewport;
            screenCenter = new Vector2(viewport.Width / 2, viewport.Height / 2);
            ball1 = Texture2DExtensions.CreateBall(this.GraphicsDevice, BALL_RADIUS1 * BALL_SCALE1);
            ballCenter1 = new Vector2(ball1.Width / 2, ball1.Height / 2);
            ballPosition1 = new Vector2(viewport.Width / 2, viewport.Height / 2);

            //spriteBatch2 = new SpriteBatch(GraphicsDevice);

            //viewport = this.GraphicsDevice.Viewport;
            ball2 = Texture2DExtensions.CreateBall(this.GraphicsDevice, BALL_RADIUS2 * BALL_SCALE2);
            ballCenter2 = new Vector2(ball2.Width / 2, ball2.Height / 2);
            ballPosition2 = new Vector2(0, viewport.Height);

            //tinyTexture = new Texture2D(this.GraphicsDevice, 50, 100);
            //tinyTexture.SetData<Color>(new Color[] { Color.White});
            //tinyTexturePosition = new Vector2(0, viewport.Height);
            //tinyTextureCenter = new Vector2(tinyTexture.Width / 2, tinyTexture.Height / 2);


            segoe96 = this.Content.Load<SpriteFont>("Segoe96");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed) // count == 0)
                this.Exit();

            // Calculate new velocity and position
            Vector2 acceleration2D = Vector2.Zero;

            lock (accelerationLock1)
            {
                acceleration2D = new Vector2(acceleration1.X, -acceleration1.Y);
            }
            
            lock (accelerationLock2)
            {
                acceleration2D = new Vector2(acceleration2.X, -acceleration2.Y);
            }

            float elapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            ballVelocity1 += GRAVITY * acceleration2D * elapsedSeconds;
            ballPosition1 += ballVelocity1 * elapsedSeconds;

            //float elapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            ballVelocity2 += GRAVITY * acceleration2D * elapsedSeconds;
            ballPosition2 += ballVelocity2 * elapsedSeconds;

            // Check for hitting edge
            if (ballPosition2.X - BALL_RADIUS2 < 0)
            {
                ballPosition2.X = BALL_RADIUS2;
                ballVelocity2.X = 0;
            }
            if (ballPosition2.X + BALL_RADIUS2 > viewport.Width)
            {
                ballPosition2.X = viewport.Width - BALL_RADIUS2;
                ballVelocity2.X = 0;
            }
            if (ballPosition2.Y - BALL_RADIUS2 < 0)
            {
                ballPosition2.Y = BALL_RADIUS2;
                ballVelocity2.Y = 0;
            }
            if (ballPosition2.Y + BALL_RADIUS2 > viewport.Height)
            {
                ballPosition2.Y = viewport.Height - BALL_RADIUS2;
                ballVelocity2.Y = 0;
            }

            // Check for bouncing off edge
            bool needAnotherLoop = false;

            do
            {
                needAnotherLoop = false;

                if (ballPosition1.X - BALL_RADIUS1 < 0)
                {
                    //score += ((ballPosition1.X <= (tinyTexturePosition.X + tinyTexture.Width / 2)) && (ballPosition1.X >= (tinyTexturePosition.X - tinyTexture.Width / 2)) && ballPosition1.Y == tinyTexturePosition.Y) ? HIT : PENALTY;
                    ballPosition1.X = -ballPosition1.X + 2 * BALL_RADIUS1;
                    ballVelocity1.X *= -BOUNCE;
                    needAnotherLoop = true;
                    //count--;
                }
                else if (ballPosition1.X + BALL_RADIUS1 > viewport.Width)
                {
                    //score += ((ballPosition1.X <= (tinyTexturePosition.X + tinyTexture.Width / 2)) && (ballPosition1.X >= (tinyTexturePosition.X - tinyTexture.Width / 2)) && ballPosition1.Y == tinyTexturePosition.Y) ? HIT : PENALTY;
                    ballPosition1.X = -ballPosition1.X - 2 * (BALL_RADIUS1 - viewport.Width);
                    ballVelocity1.X *= -BOUNCE;
                    needAnotherLoop = true;
                    //count--;
                }
                else if (ballPosition1.Y - BALL_RADIUS1 < 0)
                {
                    //score += ((ballPosition1.X <= (tinyTexturePosition.X + tinyTexture.Width / 2)) && (ballPosition1.X >= (tinyTexturePosition.X - tinyTexture.Width / 2)) && ballPosition1.Y == tinyTexturePosition.Y) ? HIT : PENALTY;
                    ballPosition1.Y = -ballPosition1.Y + 2 * BALL_RADIUS1;
                    ballVelocity1.Y *= -BOUNCE;
                    needAnotherLoop = true;
                    //count--;
                }
                else if (ballPosition1.Y + BALL_RADIUS1 > viewport.Height)
                {
                    score += ((ballPosition1.X <= (ballPosition2.X + BALL_RADIUS2)) && (ballPosition1.X >= (ballPosition2.X - BALL_RADIUS2)) && (ballPosition1.Y <= ballPosition2.Y + BALL_RADIUS2) && (ballPosition1.Y >= ballPosition2.Y - BALL_RADIUS2)) ? HIT : PENALTY;                    
                    ballPosition1.Y = -ballPosition1.Y - 2 * (BALL_RADIUS1 - viewport.Height);
                    ballVelocity1.Y *= -BOUNCE;
                    needAnotherLoop = true;
                    //count--;
                }
            }
            while (needAnotherLoop);

            scoreText.Remove(0, scoreText.Length);
            scoreText.Append(score);
            scoreCenter = segoe96.MeasureString(scoreText) / 2;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Aqua);

            spriteBatch1.Begin();
            //spriteBatch2.Begin();
            //Rectangle rect = new Rectangle(0, viewport.Height, 100, 50);
            //spriteBatch1.Draw(tinyTexture, rect, Color.Tomato);
            spriteBatch1.DrawString(segoe96, scoreText, screenCenter,Color.DarkRed,0,scoreCenter, 1, SpriteEffects.None, 0);
            spriteBatch1.Draw(ball1, ballPosition1, null, Color.DarkGreen, 0, ballCenter1, 1f / BALL_SCALE1, SpriteEffects.None, 0);
            spriteBatch1.Draw(ball2, ballPosition2, null, Color.Red, 0, ballCenter2, 1f / BALL_SCALE2, SpriteEffects.None, 0);
            spriteBatch1.End();
            //spriteBatch2.End();

            base.Draw(gameTime);
        }
    }
}


