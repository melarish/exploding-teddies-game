using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace GameProject
{
    /// <summary>
    /// Information for resolving a collision. Contains the new velocity and 
    /// draw rectangle for each of the objects involved in the collision. Also
    /// tells whether each draw rectangle is out of the game window bounds
    /// </summary>
    public class CollisionResolutionInfo
    {
        #region Fields

        Vector2 firstVelocity;
        Rectangle firstDrawRectangle;
        bool firstOutOfBounds;
        Vector2 secondVelocity;
        Rectangle secondDrawRectangle;
        bool secondOutOfBounds;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="firstVelocity">the first velocity</param>
        /// <param name="firstDrawRectangle">the first draw rectangle</param>
        /// <param name="firstOutOfBounds">whether or not the first draw rectangle is out of bounds</param>
        /// <param name="secondVelocity">the second velocity</param>
        /// <param name="secondDrawRectangle">the second draw rectangle</param>
        /// <param name="secondOutOfBounds">whether or not the second draw rectangle is out of bounds</param>
        public CollisionResolutionInfo(Vector2 firstVelocity,
            Rectangle firstDrawRectangle, bool firstOutOfBounds,
            Vector2 secondVelocity, Rectangle secondDrawRectangle,
            bool secondOutOfBounds)
        {
            this.firstVelocity = firstVelocity;
            this.firstDrawRectangle = firstDrawRectangle;
            this.firstOutOfBounds = firstOutOfBounds;
            this.secondVelocity = secondVelocity;
            this.secondDrawRectangle = secondDrawRectangle;
            this.secondOutOfBounds = secondOutOfBounds;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the first velocity
        /// </summary>
        public Vector2 FirstVelocity
        {
            get { return firstVelocity; }
        }

        /// <summary>
        /// Gets the first draw rectangle
        /// </summary>
        public Rectangle FirstDrawRectangle
        {
            get { return firstDrawRectangle; }
        }

        /// <summary>
        /// Gets and sets whether or not the first draw rectangle is out of bounds
        /// </summary>
        public bool FirstOutOfBounds
        {
            get { return firstOutOfBounds; }
            set { firstOutOfBounds = value; }
        }

        /// <summary>
        /// Gets the second velocity
        /// </summary>
        public Vector2 SecondVelocity
        {
            get { return secondVelocity; }
        }

        /// <summary>
        /// Gets the second draw rectangle
        /// </summary>
        public Rectangle SecondDrawRectangle
        {
            get { return secondDrawRectangle; }
        }

        /// <summary>
        /// Gets and sets whether or not the second draw rectangle is out of bounds
        /// </summary>
        public bool SecondOutOfBounds
        {
            get { return secondOutOfBounds; }
            set { secondOutOfBounds = value; }
        }

        #endregion
    }
}
