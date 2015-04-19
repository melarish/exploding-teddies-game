using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace GameProject
{
    /// <summary>
    /// Provides utilities for checking collisions
    /// </summary>
    public static class CollisionUtils
    {
        #region Public methods

        /// <summary>
        /// Checks to see if the area for the given drawRectangle is collision free given
        /// the list of other rectangles
        /// </summary>
        /// <param name="drawRectangle">the draw rectangle to check</param>
        /// <param name="otherRectangles">the other rectangles</param>
        /// <returns>true for collision free, false if at least one collision would occur</returns>
        public static bool IsCollisionFree(Rectangle drawRectangle, List<Rectangle> otherRectangles)
        {
            foreach (Rectangle otherRectangle in otherRectangles)
            {
                if (drawRectangle.Intersects(otherRectangle))
                {
                    return false;
                }
            }

            // if we get here, the area is collision free
            return true;
        }

        /// <summary>
        /// Returns the collision resolution info object associated with a detected collision or null if no collision
        /// is detected
        /// 
        /// Ref: David M. Bourg, Physics for Game Developers, pages 207 and 209
        /// </summary>
        /// <param name="timeStep">the time step, in milliseconds</param>
        /// <param name="windowWidth">the width of the game window</param>
        /// <param name="windowHeight">the height of the game window</param>
        /// <param name="firstVelocity">the velocity of the first object</param>
        /// <param name="firstDrawRectangle">the draw rectangle of the first object</param>
        /// <param name="secondVelocity">the velocity of the second object</param>
        /// <param name="secondDrawRectangle">the draw rectangle of the second object</param>
        /// <returns>the collision resolution info object for a detected collision or null if no collision is detected</returns>
        public static CollisionResolutionInfo CheckCollision(int timeStep, int windowWidth, int windowHeight,
            Vector2 firstVelocity, Rectangle firstDrawRectangle,
            Vector2 secondVelocity, Rectangle secondDrawRectangle)
        {
            Rectangle initialFirstAabr;
            Rectangle initialSecondAabr;
            Rectangle currentFirstAabr;
            Rectangle currentSecondAabr;
            Rectangle collisionRectangle;

            // overlap test
            bool collisionDetected = firstDrawRectangle.Intersects(secondDrawRectangle);
            if (collisionDetected)
            {
                // initialize non-changing properties
                currentFirstAabr.Width = firstDrawRectangle.Width;
                currentFirstAabr.Height = firstDrawRectangle.Height;
                currentSecondAabr.Width = secondDrawRectangle.Width;
                currentSecondAabr.Height = secondDrawRectangle.Height;

                // back up both objects to their locations before the time step
                float firstDx = (firstVelocity.X * timeStep);
                float firstDy = (firstVelocity.Y * timeStep);
                initialFirstAabr.X = (int)(firstDrawRectangle.X - firstDx);
                initialFirstAabr.Y = (int)(firstDrawRectangle.Y - firstDy);
                initialFirstAabr.Width = firstDrawRectangle.Width;
                initialFirstAabr.Height = firstDrawRectangle.Height;

                float secondDx = (secondVelocity.X * timeStep);
                float secondDy = (secondVelocity.Y * timeStep);
                initialSecondAabr.X = (int)(secondDrawRectangle.X - secondDx);
                initialSecondAabr.Y = (int)(secondDrawRectangle.Y - secondDy);
                initialSecondAabr.Width = secondDrawRectangle.Width;
                initialSecondAabr.Height = secondDrawRectangle.Height;

                // at fixed time step of 60 fps, time increment can only be 8, 4, 2, or 1
                int timeIncrement = timeStep / 2;
                int collisionDt = timeStep;    // we know we have a collision or we wouldn't be here
                int dt = timeIncrement;
                while (timeIncrement > 0)
                {
                    // move both objects forward by dt from their initial positions
                    firstDx = firstVelocity.X * dt;
                    firstDy = firstVelocity.Y * dt;
                    secondDx = secondVelocity.X * dt;
                    secondDy = secondVelocity.Y * dt;

                    // update axis-aligned bounding rectangles
                    currentFirstAabr.X = (int)(initialFirstAabr.X + firstDx);
                    currentFirstAabr.Y = (int)(initialFirstAabr.Y + firstDy);
                    currentSecondAabr.X = (int)(initialSecondAabr.X + secondDx);
                    currentSecondAabr.Y = (int)(initialSecondAabr.Y + secondDy);

                    // cut time increment in half as we search for the time of collision
                    timeIncrement /= 2;

                    collisionDetected = currentFirstAabr.Intersects(currentSecondAabr);
                    if (collisionDetected)
                    {
                        // collision detected, so save collision dt and reduce dt to make it earlier
                        collisionDt = dt;
                        dt -= timeIncrement;

                        // save the collision rectangle in case we don't find any other collisions
                        Rectangle.Intersect(ref currentFirstAabr, ref currentSecondAabr, out collisionRectangle);
                    }
                    else
                    {
                        // no collision detected, so increase dt to make it later
                        dt += timeIncrement;
                    }
                }

                // get rectangle locations at start of collision
                int collisionStartTime = collisionDt;
                firstDx = firstVelocity.X * collisionStartTime;
                firstDy = firstVelocity.Y * collisionStartTime;
                secondDx = secondVelocity.X * collisionStartTime;
                secondDy = secondVelocity.Y * collisionStartTime;

                currentFirstAabr.X = (int)(initialFirstAabr.X + firstDx);
                currentFirstAabr.Y = (int)(initialFirstAabr.Y + firstDy);
                currentSecondAabr.X = (int)(initialSecondAabr.X + secondDx);
                currentSecondAabr.Y = (int)(initialSecondAabr.Y + secondDy);
                
                // use square collision normals
                Rectangle intersection = Rectangle.Intersect(currentFirstAabr, currentSecondAabr);
                CollisionSide collisionSide = GetCollisionSide(currentSecondAabr,
                    intersection, firstVelocity, secondVelocity);

                // move objects through complete time step
                int preCollisionDuration = collisionStartTime - 1;
                int postCollisionDuration = timeStep - collisionStartTime + 1;
                CollisionResolutionInfo cri = BounceObjects(firstVelocity, initialFirstAabr,
                    secondVelocity, initialSecondAabr, preCollisionDuration, postCollisionDuration,
                    collisionSide);

                // check out of bounds and return
                cri.FirstOutOfBounds = OutOfBounds(firstDrawRectangle, windowWidth, windowHeight);
                cri.SecondOutOfBounds = OutOfBounds(secondDrawRectangle, windowWidth, windowHeight);
                return cri;
            }
            else
            {
                // no collision
                return null;
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Gets the collision side of the given rectangle and collision rectangle. firstVelocity is for the object colliding with
        /// the object with secondVvelocity
        /// </summary>
        /// <param name="rectangle">the rectangle the object is colliding with</param>
        /// <param name="collisionRectangle">the collision rectangle</param>
        /// <param name="firstVelocity">the velocity for the first object</param>
        /// <param name="secondVelocity">the velocity for the second object</param>
        /// <returns>the collision side</returns>
        public static CollisionSide GetCollisionSide(Rectangle rectangle, Rectangle collisionRectangle,
            Vector2 firstVelocity, Vector2 secondVelocity)
        {
            List<CollisionSide> collisionSides = GetCollisionSides(rectangle, collisionRectangle);
            if (collisionSides.Count == 1)
            {
                return collisionSides[0];
            }
            else if (collisionSides.Count == 2)
            {
                // figure out correct side
                CollisionSide topBottomCollisionSide;
                if (collisionSides.Contains(CollisionSide.Top))
                {
                    topBottomCollisionSide = CollisionSide.Top;
                }
                else
                {
                    topBottomCollisionSide = CollisionSide.Bottom;
                }
                CollisionSide leftRightCollisionSide;
                if (collisionSides.Contains(CollisionSide.Left))
                {
                    leftRightCollisionSide = CollisionSide.Left;
                }
                else
                {
                    leftRightCollisionSide = CollisionSide.Right;
                }

                // first - second is correct for the checks below (we usually do second - first)
                float velocityRelativeX = firstVelocity.X - secondVelocity.X;
                float velocityRelativeY = firstVelocity.Y - secondVelocity.Y;

                // use both collision rectangle and velocities for ratios
                float xRatio = collisionRectangle.Width / Math.Abs(velocityRelativeX);
                float yRatio = collisionRectangle.Height / Math.Abs(velocityRelativeY);
                if (yRatio < xRatio)
                {
                    // check for top or bottom not possible
                    if ((velocityRelativeY < 0 && topBottomCollisionSide == CollisionSide.Top) ||
                        (velocityRelativeY > 0 && topBottomCollisionSide == CollisionSide.Bottom))
                    {
                        // must have been collision on left or right
                        return leftRightCollisionSide;
                    }
                    else
                    {
                        // top or bottom possible, y velocity greater than x velocity
                        return topBottomCollisionSide;
                    }
                }
                else
                {
                    // check for left or right not possible
                    if ((velocityRelativeX < 0 && leftRightCollisionSide == CollisionSide.Left) ||
                        (velocityRelativeX > 0 && leftRightCollisionSide == CollisionSide.Right))
                    {
                        // must have been collision on top or bottom
                        return topBottomCollisionSide;
                    }
                    else
                    {
                        // left or right possible, x velocity greater than y velocity
                        return leftRightCollisionSide;
                    }
                }
            }
            else
            {
                // 3 collision sides can occur if a taller object hits a shorter object from the side (for example)
                if (collisionSides.Contains(CollisionSide.Top) &&
                    collisionSides.Contains(CollisionSide.Bottom))
                {
                    if (collisionSides.Contains(CollisionSide.Left))
                    {
                        return CollisionSide.Left;
                    }
                    else
                    {
                        return CollisionSide.Right;
                    }
                }
                else
                {
                    // must be colliding with both left and right
                    if (collisionSides.Contains(CollisionSide.Top))
                    {
                        return CollisionSide.Top;
                    }
                    else
                    {
                        return CollisionSide.Bottom;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the list of possible collision sides for the given rectangle and collision rectangle
        /// </summary>
        /// <param name="rectangle">the rectangle</param>
        /// <param name="collisionRectangle">the collision rectangle</param>
        /// <returns>the list of possible collision sides</returns>
        private static List<CollisionSide> GetCollisionSides(Rectangle rectangle, Rectangle collisionRectangle)
        {
            List<CollisionSide> collisionSides = new List<CollisionSide>();
            if (collisionRectangle.Left == rectangle.Left)
            {
                collisionSides.Add(CollisionSide.Left);
            }
            if (collisionRectangle.Right == rectangle.Right)
            {
                collisionSides.Add(CollisionSide.Right);
            }
            if (collisionRectangle.Top == rectangle.Top)
            {
                collisionSides.Add(CollisionSide.Top);
            }
            if (collisionRectangle.Bottom == rectangle.Bottom)
            {
                collisionSides.Add(CollisionSide.Bottom);
            }
            return collisionSides;
        }

        /// <summary>
        /// Gets the collision normal for the given collision side of an object
        /// </summary>
        /// <param name="side">the collision side</param>
        /// <returns>the collision normal</returns>
        private static Vector2 GetSideCollisionNormal(CollisionSide side)
        {
            switch (side)
            {
                case CollisionSide.Top: return new Vector2(0, -1);
                case CollisionSide.Bottom: return Vector2.UnitY;
                case CollisionSide.Left: return Vector2.UnitX;
                case CollisionSide.Right: return new Vector2(-1, 0);
                default: return Vector2.Zero;   // should never get here
            }
        }

        /// <summary>
        /// Bounces the two objects off each other. This method makes sure the speed
        /// of each object after the bounce is set to the given speed values
        /// </summary>
        /// <param name="firstVelocity">the velocity of the first object</param>
        /// <param name="firstDrawRectangle">the draw rectangle of the first object</param>
        /// <param name="secondVelocity">the velocity of the second object</param>
        /// <param name="secondDrawRectangle">the draw rectangle of the second object</param>
        /// <param name="preCollisionDuration">the duration before the collision</param>
        /// <param name="postCollisionDuration">the duration after the collision</param>
        /// <param name="collisionSide">the collision side</param>
        /// <returns>the collision resolution info object</returns>
        private static CollisionResolutionInfo BounceObjects(Vector2 firstVelocity, Rectangle firstDrawRectangle,
            Vector2 secondVelocity, Rectangle secondDrawRectangle, int preCollisionDuration,
            int postCollisionDuration, CollisionSide collisionSide)
        {
            // save speeds for later
            float firstSpeed = firstVelocity.Length();
            float secondSpeed = secondVelocity.Length();

            // move forward up to collision
            Rectangle newFirstDrawRectangle = MoveForward(firstVelocity, firstDrawRectangle,
                preCollisionDuration);
            Rectangle newSecondDrawRectangle = MoveForward(secondVelocity, secondDrawRectangle,
                 preCollisionDuration);

            // change velocities as appropriate
            Vector2 newFirstVelocity;
            Vector2 newSecondVelocity;
            GetNewVelocities(firstVelocity, secondVelocity, collisionSide,
                out newFirstVelocity, out newSecondVelocity);

            // move objects forward after collision
            MoveForward(newFirstVelocity, newFirstDrawRectangle,
                postCollisionDuration);
            MoveForward(newSecondVelocity, newSecondDrawRectangle,
                postCollisionDuration);


            // may still need to move objects apart if they're still colliding
            MoveCollidingObjectsApart(newFirstVelocity, newFirstDrawRectangle,
                newSecondVelocity, newSecondDrawRectangle,
                out newFirstDrawRectangle, out newSecondDrawRectangle);

            return new CollisionResolutionInfo(newFirstVelocity, newFirstDrawRectangle,
                false, newSecondVelocity, newSecondDrawRectangle, false);
        }

        /// <summary>
        /// Moves the object forward along its velocity for the given duration
        /// </summary>
        /// <param name="velocity">the object velocity</param>
        /// <param name="drawRectangle">the object draw rectangle</param>
        /// <param name="duration">the duration</param>
        /// <returns>the new draw rectangle</returns>
        private static Rectangle MoveForward(Vector2 velocity, Rectangle drawRectangle,
            int duration)
        {
            Rectangle newRectangle = new Rectangle(
                drawRectangle.X, drawRectangle.Y,
                drawRectangle.Width, drawRectangle.Height);
            newRectangle.X = (int)(newRectangle.X + velocity.X * duration);
            newRectangle.Y = (int)(newRectangle.Y + velocity.Y * duration);
            return newRectangle;
        }

        /// <summary>
        /// Gets the new velocity vectors for the collider (first object) and
        /// collidee (second object) in a collision. Need to be careful if
        /// the collider caught up to the collidee when both were going in
        /// the same direction
        /// </summary>
        /// <param name="firstVelocity">velocity of the first object</param>
        /// <param name="secondVelocity">velocity of the second object</param>
        /// <param name="collisionSide">the collision side</param>
        /// <param name="newFirstVelocity">the new first object velocity</param>
        /// <param name="newSecondVelocity">the new second object velocity</param>
        private static void GetNewVelocities(Vector2 firstVelocity, Vector2 secondVelocity,
            CollisionSide collisionSide, out Vector2 newFirstVelocity, out Vector2 newSecondVelocity)
        {
            switch (collisionSide)
            {
                case CollisionSide.Top:
                    if (firstVelocity.Y > 0 && secondVelocity.Y > 0)
                    {
                        // first object caught up to second object, only change first y velocity
                        newFirstVelocity = new Vector2(firstVelocity.X, -1 * firstVelocity.Y);
                        newSecondVelocity = secondVelocity;
                    }
                    else if (firstVelocity.Y < 0 && secondVelocity.Y < 0)
                    {
                        // second object caught up to first object, only change second y velocity
                        newFirstVelocity = firstVelocity;
                        newSecondVelocity = new Vector2(secondVelocity.X, -1 * secondVelocity.Y);
                    }
                    else
                    {
                        // normal top collision, change both y velocities
                        newFirstVelocity = new Vector2(firstVelocity.X, -1 * firstVelocity.Y);
                        newSecondVelocity = new Vector2(secondVelocity.X, -1 * secondVelocity.Y);
                    }
                    break;
                case CollisionSide.Bottom:
                    if (firstVelocity.Y > 0 && secondVelocity.Y > 0)
                    {
                        // second object caught up to first object, only change second y velocity
                        newFirstVelocity = firstVelocity;
                        newSecondVelocity = new Vector2(secondVelocity.X, -1 * secondVelocity.Y);
                    }
                    else if (firstVelocity.Y < 0 && secondVelocity.Y < 0)
                    {
                        // first object caught up to second object, only change first y velocity
                        newFirstVelocity = new Vector2(firstVelocity.X, -1 * firstVelocity.Y);
                        newSecondVelocity = secondVelocity;
                    }
                    else
                    {
                        // normal bottom collision, change both y velocities
                        newFirstVelocity = new Vector2(firstVelocity.X, -1 * firstVelocity.Y);
                        newSecondVelocity = new Vector2(secondVelocity.X, -1 * secondVelocity.Y);
                    }
                    break;
                case CollisionSide.Left:
                    if (firstVelocity.X > 0 && secondVelocity.X > 0)
                    {
                        // first object caught up to second object, only change first x velocity
                        newFirstVelocity = new Vector2(-1 * firstVelocity.X, firstVelocity.Y);
                        newSecondVelocity = secondVelocity;
                    }
                    else if (firstVelocity.X < 0 && secondVelocity.X < 0)
                    {
                        // second object caught up to first object, only change second x velocity
                        newFirstVelocity = firstVelocity;
                        newSecondVelocity = new Vector2(-1 * secondVelocity.X, secondVelocity.Y);
                    }
                    else
                    {
                        // normal left collision, change both x velocities
                        newFirstVelocity = new Vector2(-1 * firstVelocity.X, firstVelocity.Y);
                        newSecondVelocity = new Vector2(-1 * secondVelocity.X, secondVelocity.Y);
                    }
                    break;
                case CollisionSide.Right:
                    if (firstVelocity.X > 0 && secondVelocity.X > 0)
                    {
                        // second object caught up to first object, only change second x velocity
                        newFirstVelocity = firstVelocity;
                        newSecondVelocity = new Vector2(-1 * secondVelocity.X, secondVelocity.Y);
                    }
                    else if (firstVelocity.X < 0 && secondVelocity.X < 0)
                    {
                        // first object caught up to second object, only change first x velocity
                        newFirstVelocity = new Vector2(-1 * firstVelocity.X, firstVelocity.Y);
                        newSecondVelocity = secondVelocity;
                    }
                    else
                    {
                        // normal right collision, change both x velocities
                        newFirstVelocity = new Vector2(-1 * firstVelocity.X, firstVelocity.Y);
                        newSecondVelocity = new Vector2(-1 * secondVelocity.X, secondVelocity.Y);
                    }
                    break;
                default:
                    // should never get here
                    newFirstVelocity = firstVelocity;
                    newSecondVelocity = secondVelocity;
                    break;
            }
        }

        /// <summary>
        /// Checks whether or not the given draw rectangle is partially or fully
        /// out of bounds for the given window dimensions
        /// </summary>
        /// <param name="drawRectangle">the draw rectangle</param>
        /// <param name="windowWidth">the window width</param>
        /// <param name="windowHeight">the window height</param>
        /// <returns>true for out of bound,s false otherwise</returns>
        private static bool OutOfBounds(Rectangle drawRectangle,
            int windowWidth, int windowHeight)
        {
            return drawRectangle.Left < 0 ||
                drawRectangle.Right > windowWidth ||
                drawRectangle.Top < 0 ||
                drawRectangle.Bottom > windowHeight;
        }


        /// <summary>
        /// Moves the two colliding objects apart. The objects have already had their velocities changed, so
        /// we simply move the objects along their velocity vectors so their axis-aligned bounding rectangles 
        /// no longer intersect
        /// </summary>
        /// <param name="firstVelocity">the velocity of the first object</param>
        /// <param name="firstDrawRectangle">the draw rectangle for the first object</param>
        /// <param name="secondVelocity">the velocity of the second object</param>
        /// <param name="secondDrawRectangle">the draw rectangle for the second object</param>
        /// <param name="newFirstDrawRectangle">the new draw rectangle for the first object</param>
        /// <param name="newSecondDrawRectangle">the new draw rectangle for the second object</param>
        private static void MoveCollidingObjectsApart(Vector2 firstVelocity,
            Rectangle firstDrawRectangle, Vector2 secondVelocity,
            Rectangle secondDrawRectangle, out Rectangle newFirstDrawRectangle,
            out Rectangle newSecondDrawRectangle)
        {
            // calculate relative ratios for moving each object
            float firstObjectSpeedSquared = firstVelocity.LengthSquared();
            float secondObjectSpeedSquared = secondVelocity.LengthSquared();
            float firstMultiplier = firstObjectSpeedSquared / (firstObjectSpeedSquared + secondObjectSpeedSquared);
            float secondMultiplier = 1 - firstMultiplier;

            // collision intersection
            Rectangle intersection = Rectangle.Intersect(firstDrawRectangle, secondDrawRectangle);

            // ### below is klugey, fix it

            // max distance we need to move apart is the width plus height of the intersection (this isn't
            // perfectly accurate, but it should be larger than we really need to move)
            int distance = intersection.Width + intersection.Height;
            newFirstDrawRectangle = MoveObject(firstVelocity, firstDrawRectangle, distance * firstMultiplier);
            newSecondDrawRectangle = MoveObject(secondVelocity, secondDrawRectangle, distance * secondMultiplier);
        }

        /// <summary>
        /// Moves the given object the given distance along the object's velocity vector
        /// </summary>
        /// <param name="velocity">the object velocity</param>
        /// <param name="drawRectangle">the object draw rectangle</param>
        /// <param name="distance">the distance to move the object</param>
        /// <returns>the new draw rectangle</returns>
        private static Rectangle MoveObject(Vector2 velocity, Rectangle drawRectangle,
            float distance)
        {
            return new Rectangle(
                (int)(drawRectangle.X + distance * velocity.X),
                (int)(drawRectangle.Y + distance * velocity.Y),
                drawRectangle.Width, drawRectangle.Height);
        }

        #endregion
    }
}
