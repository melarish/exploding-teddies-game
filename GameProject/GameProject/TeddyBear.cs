using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameProject
{
    /// <summary>
    /// A class for a teddy bear
    /// </summary>
    public class TeddyBear
    {
        #region Fields

        bool active = true;

        // drawing support
        Texture2D sprite;
        Rectangle drawRectangle;

        // velocity information
        Vector2 velocity = new Vector2(0, 0);

        // shooting support
        int elapsedShotTime = 0;
        int firingDelay;

        // sound effects
        SoundEffect bounceSound;
        SoundEffect shootSound;

        #endregion

        #region Constructors

        /// <summary>
        ///  Constructs a teddy bear centered on the given x and y with the
        ///  given velocity
        /// </summary>
        /// <param name="contentManager">the content manager for loading content</param>
        /// <param name="spriteName">the name of the sprite for the teddy bear</param>
        /// <param name="x">the x location of the center of the teddy bear</param>
        /// <param name="y">the y location of the center of the teddy bear</param>
        /// <param name="shootSound">the sound the teddy bear plays when bouncing</param>
        /// <param name="shootSound">the sound the teddy bear plays when shooting</param>
        public TeddyBear(ContentManager contentManager, string spriteName, int x, int y,
            Vector2 velocity, SoundEffect bounceSound, SoundEffect shootSound)
        {
            LoadContent(contentManager,spriteName, x, y);
            this.velocity = velocity;
            this.bounceSound = bounceSound;
            this.shootSound = shootSound;
            firingDelay = GetRandomFiringDelay();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets and sets whether or not the teddy bear is active
        /// </summary>
        public bool Active
        {
            get { return active; }
            set { active = value; }
        }

        /// <summary>
        /// Gets the location of the teddy bear
        /// </summary>
        public Point Location
        {
            get { return drawRectangle.Center; }
        }

        /// <summary>
        /// Sets the x location of the center of the teddy bear
        /// </summary>
        public int X
        {
            set { drawRectangle.X = value - drawRectangle.Width / 2; }
        }

        /// <summary>
        /// Sets the y location of the center of the teddy bear
        /// </summary>
        public int Y
        {
            set { drawRectangle.Y = value - drawRectangle.Height / 2; }
        }

        /// <summary>
        /// Gets the collision rectangle for the teddy bear
        /// </summary>
        public Rectangle CollisionRectangle
        {
            get { return drawRectangle; }
        }

        /// <summary>
        /// Gets and sets the velocity of the teddy bear
        /// </summary>
        public Vector2 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }

        /// <summary>
        /// Gets and sets the draw rectangle for the teddy bear
        /// </summary>
        public Rectangle DrawRectangle
        {
            get { return drawRectangle; }
            set { drawRectangle = value; }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Updates the teddy bear's location, bouncing if necessary. Also has
        /// the teddy bear fire a projectile when it's time to
        /// </summary>
        /// <param name="gameTime">game time</param>
        public void Update(GameTime gameTime)
        {
            // move the teddy bear
            drawRectangle.X += (int)(velocity.X * gameTime.ElapsedGameTime.Milliseconds);
            drawRectangle.Y += (int)(velocity.Y * gameTime.ElapsedGameTime.Milliseconds);
            // bounce as necessary
            BounceTopBottom();
            BounceLeftRight();

            // fire projectile as appropriate
            elapsedShotTime += gameTime.ElapsedGameTime.Milliseconds;
            if (elapsedShotTime > firingDelay)
            {
                elapsedShotTime = 0;
                firingDelay = GetRandomFiringDelay();
                Projectile teddyProjectile = new Projectile(ProjectileType.TeddyBear, Game1.GetProjectileSprite(ProjectileType.TeddyBear),
                    drawRectangle.Center.X, drawRectangle.Center.Y + GameConstants.TEDDY_BEAR_PROJECTILE_OFFSET, GetProjectileYVelocity());
                Game1.AddProjectile(teddyProjectile);
            }
            // timer concept (for animations) introduced in Chapter 7

        }

        /// <summary>
        /// Draws the teddy bear
        /// </summary>
        /// <param name="spriteBatch">the sprite batch to use</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(sprite, drawRectangle, Color.White);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Loads the content for the teddy bear
        /// </summary>
        /// <param name="contentManager">the content manager to use</param>
        /// <param name="spriteName">the name of the sprite for the teddy bear</param>
        /// <param name="x">the x location of the center of the teddy bear</param>
        /// <param name="y">the y location of the center of the teddy bear</param>
        private void LoadContent(ContentManager contentManager, string spriteName, 
            int x, int y)
        {
            // load content and set remainder of draw rectangle
            sprite = contentManager.Load<Texture2D>(spriteName);
            drawRectangle = new Rectangle(x - sprite.Width / 2, 
                y - sprite.Height / 2, sprite.Width,
                sprite.Height);
        }

        /// <summary>
        /// Bounces the teddy bear off the top and bottom window borders if necessary
        /// </summary>
        private void BounceTopBottom()
        {
            if (drawRectangle.Y < 0)
            {
                // bounce off top
                drawRectangle.Y = 0;
                velocity.Y *= -1;
            }
            else if ((drawRectangle.Y + drawRectangle.Height) > GameConstants.WINDOW_HEIGHT)
            {
                // bounce off bottom
                drawRectangle.Y = GameConstants.WINDOW_HEIGHT - drawRectangle.Height;
                velocity.Y *= -1;
            }
        }
        /// <summary>
        /// Bounces the teddy bear off the left and right window borders if necessary
        /// </summary>
        private void BounceLeftRight()
        {
            if (drawRectangle.X < 0)
            {
                // bounc off left
                drawRectangle.X = 0;
                velocity.X *= -1;
            }
            else if ((drawRectangle.X + drawRectangle.Width) > GameConstants.WINDOW_WIDTH)
            {
                // bounce off right
                drawRectangle.X = GameConstants.WINDOW_WIDTH - drawRectangle.Width;
                velocity.X *= -1;
            }
        }

        /// <summary>
        /// Gets a random firing delay between MIN_FIRING_DELAY and
        /// MIN_FIRING_DELY + FIRING_RATE_RANGE
        /// </summary>
        /// <returns>the random firing delay</returns>
        private int GetRandomFiringDelay()
        {
            return GameConstants.BEAR_MIN_FIRING_DELAY + 
                RandomNumberGenerator.Next(GameConstants.BEAR_FIRING_RATE_RANGE);
        }

        /// <summary>
        /// Gets the y velocity for the projectile being fired
        /// </summary>
        /// <returns>the projectile y velocity</returns>
        private float GetProjectileYVelocity()
        {
            if (velocity.Y > 0)
            {
                return velocity.Y + GameConstants.TEDDY_BEAR_PROJECTILE_SPEED;
            }
            else
            {
                return GameConstants.TEDDY_BEAR_PROJECTILE_SPEED;
            }
        }

        #endregion
    }
}
