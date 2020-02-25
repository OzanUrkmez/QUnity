using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace QUnity.Movement
{
    //event systems etc as well. speed coefficient marks.
    /// <summary>
    /// Defines a semi-circular gradual movement integrated into the Questry Gradual Movement System. 
    /// </summary>
    public class QGradualCircularMovement : QIGradualMovement
    {

        /// <summary>
        /// Defines all the parameters for a general gradual circular movement.
        /// </summary>
        [Serializable]
        public struct CircularMovementArgs
        {
            /// <summary>
            /// The starting position. If set to any other position than that of the gameobject, the gameobject will be moved to that position when the movement starts.
            /// </summary>
            public Vector3 startingPosition;
            /// <summary>
            /// The position the gameobject will be moving towards. This may not be the same as the starting point.
            /// </summary>
            public Vector3 finalPosition;
            /// <summary>
            /// The time it shall take for the movement to finish.
            /// </summary>
            public float time;
            /// <summary>
            /// Defines whether the gradual movement should be stacked along with a series of movements; or only start when a series of movements has finished and define its own series of movements upon which other movements may stack.
            /// </summary>
            public bool stacked;
            /// <summary>
            /// The reference point. The center of the circle will be between the line formed by the start and end points and a plane defined by this point. THIS POINT MAY NOT BE COLINEAR WITH THE START AND END POINTS
            /// </summary>
            public Vector3 referencePivot;
            /// <summary>
            /// how many degrees of the constructed circle the movement will comprise. This value must be between 10 and 180
            /// </summary>
            public float degree;
            /// <summary>
            /// whether this movement should be tried to be merged with other movements.
            /// </summary>
            public bool attemptMerge;
            /// <summary>
            ///  A function that is called when the movement finished, with the boolean indicating whether the movement finished or not. This function will not be called in scene changes.
            /// </summary>
            public Action<bool> onMovementFinish;

            /// <summary>
            ///  Defines whether the transformations influenced by this movement is to be done in world space or local space. Very different movements may thus be achieved. 
            /// </summary>
            public bool isWorldSpace;

            /// <summary>
            /// A function that is called each frame a movement takes place. The rigidbody passed in will be null if the object is not a rigidbody.
            /// </summary>
            public Action<GameObject, Rigidbody, Vector3> onFramePass;

            /// <summary>
            /// Defines a circular movement.
            /// </summary>
            /// <param name="startingPosition"> The starting position. If set to any other position than that of the gameobject, the gameobject will be moved to that position when the movement starts. </param>
            /// <param name="finalPosition"> The position the gameobject will be moving towards. This may not be the same as the starting point. </param>
            /// <param name="time"> The time it shall take for the movement to finish. </param>
            /// <param name="stacked"> Defines whether the gradual movement should be stacked along with a series of movements; or only start when a series of movements has finished and define its own series of movements upon which other movements may stack. </param>
            /// <param name="referencePivot"> The reference point. The center of the circle will be between the line formed by the start and end points and a plane defined by this point. THIS POINT MAY NOT BE COLINEAR WITH THE START AND END POINTS </param>
            /// <param name="degree"> how many degrees of the constructed circle the movement will comprise. This value must be between 10 and 180 </param>
            /// <param name="attemptMerge"> whether this movement should be tried to be merged with other movements. </param>
            /// <param name="isWorldSpace">  Defines whether the transformations influenced by this movement is to be done in world space or local space. Very different movements may thus be achieved. </param>
            /// <param name="onMovementFinish"> A function that is called when the movement finished, with the boolean indicating whether the movement finished or not. This function will not be called in scene changes. </param>
            /// <param name="onFramePass"> A function that is called each frame a movement takes place. The rigidbody passed in will be null if the object is not a rigidbody.</param>
            public CircularMovementArgs(Vector3 startingPosition, Vector3 finalPosition, Vector3 referencePivot, float degree, float time, bool isWorldSpace = true, bool stacked = false, bool attemptMerge = false,
                Action<bool> onMovementFinish = null, Action<GameObject, Rigidbody, Vector3> onFramePass = null)
            {
                this.startingPosition = startingPosition;
                this.finalPosition = finalPosition;
                this.referencePivot = referencePivot;
                this.time = time;
                this.degree = degree;
                this.stacked = stacked;
                this.attemptMerge = attemptMerge;
                this.onMovementFinish = onMovementFinish;
                this.isWorldSpace = isWorldSpace;
                this.onFramePass = onFramePass;
            }
        }

        private Vector3 initial, final, pivot;
        private float degree; //we make circles nao
        private float movementTime;
        private bool movementStacked;
        private float currentTime = -1;
        private GameObject movedObject;
        private Rigidbody rigidbody;
        private bool isRigidBodyMovement = false;
        private bool attemptMerge = false;
        private Action<bool> onMovementFinish;
        private bool isWorldSpace;
        private Action<GameObject, Rigidbody, Vector3> onFramePass;

        //TODO Ability to transform already created ellipsis for reuse? 

        #region Construction

        /// <summary>
        /// Initializes the circular movement
        /// </summary>
        /// <param name="gameObject"> The gameobject to be moved. </param>
        /// <param name="startingPosition"> The starting position. If set to any other position than that of the gameobject, the gameobject will be moved to that position when the movement starts. </param>
        /// <param name="finalPosition"> The position the gameobject will be moving towards. This may not be the same as the starting point. </param>
        /// <param name="time"> The time it shall take for the movement to finish. </param>
        /// <param name="stacked"> Defines whether the gradual movement should be stacked along with a series of movements; or only start when a series of movements has finished and define its own series of movements upon which other movements may stack. </param>
        /// <param name="referencePivot"> The reference point. The center of the circle will be between the line formed by the start and end points and a plane defined by this point. THIS POINT MAY NOT BE COLINEAR WITH THE START AND END POINTS </param>
        /// <param name="degree"> how many degrees of the constructed circle the movement will comprise. This value must be between 10 and 180 </param>
        /// <param name="attemptMerge"> whether this movement should be tried to be merged with other movements. </param>
        /// <param name="isWorldSpace"> Defines whether the transformations influenced by this movement is to be done in world space or local space. Very different movements may thus be achieved. </param>
        /// <param name="onMovementFinish"> A function that is called when the movement finished, with the boolean indicating whether the movement finished or not. This function will not be called in scene changes. </param>
        /// <param name="onFramePass"> A function that is called each frame a movement takes place. The rigidbody passed in will be null if the object is not a rigidbody.</param>
        public QGradualCircularMovement(GameObject gameObject, Vector3 startingPosition, Vector3 finalPosition, Vector3 referencePivot, float degree, float time, bool isWorldSpace = true, bool stacked = false, bool attemptMerge = false,
            Action<bool> onMovementFinish = null, Action<GameObject, Rigidbody, Vector3> onFramePass = null)
        {
            if (degree < 10 || degree > 180)
                throw new Exception("The degree input into a gradual circular movement must be between 10 and 180.");
            else if (degree == 180)
                degree = 179.99f;
            if (QVectorCalculations.Vector3MathfApproximately(startingPosition, finalPosition))
                throw new Exception("The input starting and ending vectors cannot be the same!");
            if (QVectorCalculations.Vector3Colinear(startingPosition, finalPosition, referencePivot))
                throw new Exception("The reference pivot, starting position, and the final position cannot be colinear!");
            movedObject = gameObject;
            initial = startingPosition;
            final = finalPosition;
            pivot = referencePivot;
            movementTime = time;
            this.degree = degree * Mathf.Deg2Rad;
            movementStacked = stacked;
            this.attemptMerge = attemptMerge;
            this.onMovementFinish = onMovementFinish;
            this.isWorldSpace = isWorldSpace;
            this.onFramePass = onFramePass;
        }

        /// <summary>
        /// Initializes the circular movement.
        /// </summary>
        /// <param name="gameObject">The gameobject to be moved.</param>
        /// <param name="args">the arguments that define the movement.</param>
        public QGradualCircularMovement(GameObject gameObject, CircularMovementArgs args)
        {
            movedObject = gameObject;
            initial = args.startingPosition;
            final = args.finalPosition;
            pivot = args.referencePivot;
            movementTime = args.time;
            this.degree = args.degree;
            movementStacked = args.stacked;
            this.attemptMerge = args.attemptMerge;
            this.onMovementFinish = args.onMovementFinish;
            if (degree < 10 || degree > 180)
                throw new Exception("The degree input into a gradual circular movement must be between 10 and 180.");
            else if (degree == 180)
                degree = 179.99f;
            if (QVectorCalculations.Vector3MathfApproximately(initial, final))
                throw new Exception("The input starting and ending vectors cannot be the same!");
            if (QVectorCalculations.Vector3Colinear(initial, initial, pivot))
                throw new Exception("The reference pivot, starting position, and the final position cannot be colinear!");
            this.degree = degree * Mathf.Deg2Rad;
            this.isWorldSpace = args.isWorldSpace;
            this.onFramePass = args.onFramePass;
        }

        /// <summary>
        /// Initializes the circular movement
        /// </summary>
        /// <param name="rigidbody"> The rigidbody to be moved. </param>
        /// <param name="startingPosition"> The starting position. If set to any other position than that of the gameobject, the gameobject will be moved to that position when the movement starts. </param>
        /// <param name="finalPosition"> The position the gameobject will be moving towards. This may not be the same as the starting point. </param>
        /// <param name="time"> The time it shall take for the movement to finish. </param>
        /// <param name="stacked"> Defines whether the gradual movement should be stacked along with a series of movements; or only start when a series of movements has finished and define its own series of movements upon which other movements may stack. </param>
        /// <param name="referencePivot"> The reference point. The center of the circle will be between the line formed by the start and end points and a plane defined by this point. THIS POINT MAY NOT BE COLINEAR WITH THE START AND END POINTS </param>
        /// <param name="degree"> how many degrees of the constructed circle the movement will comprise. This value must be between 10 and 180 </param>
        /// <param name="attemptMerge"> whether this movement should be tried to be merged with other movements. </param>
        /// <param name="isWorldSpace">  Defines whether the transformations influenced by this movement is to be done in world space or local space. Very different movements may thus be achieved. </param>
        /// <param name="onMovementFinish"> A function that is called when the movement finished, with the boolean indicating whether the movement finished or not. This function will not be called in scene changes. </param>
        /// <param name="onFramePass"> A function that is called each frame a movement takes place. The rigidbody passed in will be null if the object is not a rigidbody.</param>
        public QGradualCircularMovement(Rigidbody rigidbody, Vector3 startingPosition, Vector3 finalPosition, Vector3 referencePivot, float degree, float time, bool isWorldSpace = true, bool stacked = false, bool attemptMerge = false,
            Action<bool> onMovementFinish = null, Action<GameObject, Rigidbody, Vector3> onFramePass = null)
        {
            if (degree < 10 || degree > 180)
                throw new Exception("The degree input into a gradual circular movement must be between 10 and 180.");
            else if (degree == 180)
                degree = 179.99f;
            if (QVectorCalculations.Vector3MathfApproximately(startingPosition, finalPosition))
                throw new Exception("The input starting and ending vectors cannot be the same!");
            if (QVectorCalculations.Vector3Colinear(startingPosition, finalPosition, referencePivot))
                throw new Exception("The reference pivot, starting position, and the final position cannot be colinear!");
            isRigidBodyMovement = true;
            this.rigidbody = rigidbody;
            initial = startingPosition;
            final = finalPosition;
            pivot = referencePivot;
            movementTime = time;
            this.degree = degree * Mathf.Deg2Rad;
            movementStacked = stacked;
            this.attemptMerge = attemptMerge;
            this.onMovementFinish = onMovementFinish;
            this.isWorldSpace = isWorldSpace;
            movedObject = rigidbody.gameObject;
            this.onFramePass = onFramePass;
        }

        /// <summary>
        /// Initializes the circular movement.
        /// </summary>
        /// <param name="rigidbody">The rigidbody to be moved.</param>
        /// <param name="args">the arguments that define the movement.</param>
        public QGradualCircularMovement(Rigidbody rigidbody, CircularMovementArgs args)
        {
            this.rigidbody = rigidbody;
            isRigidBodyMovement = true;
            initial = args.startingPosition;
            final = args.finalPosition;
            pivot = args.referencePivot;
            movementTime = args.time;
            this.degree = args.degree;
            movementStacked = args.stacked;
            this.attemptMerge = args.attemptMerge;
            this.onMovementFinish = args.onMovementFinish;
            if (degree < 10 || degree > 180)
                throw new Exception("The degree input into a gradual circular movement must be between 10 and 180.");
            else if (args.degree == 180)
                degree = 179.99f;
            if (QVectorCalculations.Vector3MathfApproximately(initial, final))
                throw new Exception("The input starting and ending vectors cannot be the same!");
            if (QVectorCalculations.Vector3Colinear(initial, initial, pivot))
                throw new Exception("The reference pivot, starting position, and the final position cannot be colinear!");
            this.degree = degree * Mathf.Deg2Rad;
            this.isWorldSpace = args.isWorldSpace;
            movedObject = rigidbody.gameObject;
            this.onFramePass = args.onFramePass;
        }


        #endregion


        #region Manager Communication and Manager Related Properties

        Vector3 v1, v2;

        /// <summary>
        /// Starts the defined movement if it has not already been started.
        /// </summary>
        public void StartMovement()
        {
            if (currentTime > 0)
                return;
            //construct the real pivot.
            movedObject.transform.position = initial;
            Vector3 middle = QVectorCalculations.GetVector3Middle(initial, final);
            Vector3 nearest = QVectorCalculations.GetNearestPointOnLine(middle, (final - initial), pivot);
            Vector3 dir = (pivot - nearest).normalized;
            Vector3 realPivot = (dir * (final - initial).magnitude / 2 * Mathf.Pow(Mathf.Tan(degree / 2), -1)) + initial + (final - initial) / 2;

            //construct circle.
            v1 = (initial - realPivot).normalized * (initial - realPivot).magnitude;
            v2 = (middle - QVectorCalculations.GetNearestPointOnLine(realPivot, v1, middle)).normalized * (initial - realPivot).magnitude;
            pivot = realPivot;

            currentTime = movementTime;

            //do the movement
            if (isRigidBodyMovement)
            {
                //rigidbody movement
                QGradualMovementManager.AddGradualMovement(rigidbody, this, movementStacked, attemptMerge);
            }
            else
            {
                //regular movement.
                QGradualMovementManager.AddGradualMovement(movedObject, this, movementStacked, attemptMerge);
            }
        }

        /// <summary>
        /// whether the gradual movement should be stacked along with a series of movements; or only start when a series of movements has finished and define its own series of movements upon which other movements may stack.
        /// </summary>
        public bool MovementIsStacked { get; set; }

        /// <summary>
        /// Attempts to merge the input movement into this current movement. These are often implemented such that similar types of movement, and especially linear ones over similar times, can be handled as a single movement instead to increase performance.
        /// </summary>
        /// <param name="mov"> The movement to be merged. </param>
        /// <returns> whether the input movement has been integrated into this movement. If true, this class is now responsible for adding the displacement of the other movement into itself. If false, the movement that has been attempted to be merged will be queued normally.</returns>
        public bool AttemptMerge(QIGradualMovement mov)
        {
            return false; //TODO AT LEAST IMPLEMENT SOME MERGES!
        }

        /// <summary>
        /// Based on the time calls the manager has made, returns the amount of time left before the movement is done and the GetApplyTransformation function returns negative infinity.
        /// </summary>
        /// <returns>the amount of time left before the movement is done and the GetApplyTransformation function returns negative infinity. Returns a negative value if there is no movement taking place currently</returns>
        public float GetTimeLeft()
        {
            return currentTime;
        }

        /// <summary>
        /// USED CHIEFLY BY THE MOVEMENT MANAGER. Returns the displacement of the object as affected by this gradual movement, while also keeping track of the time passed. If the gradual movement is done, returns negative infinity to signal so.
        /// </summary>
        /// <param name="time"> the time that has passed since the last frame. </param>
        /// <returns> the displacement to take place within the frame. Negative infinity if the movement is done. </returns>
        public Vector3 GetApplyTransformation(float time)
        {
            if (currentTime <= 0)
                return Vector3.negativeInfinity;
            float timePass = currentTime - time < 0 ? 0 : currentTime - time;
            Vector3 returned = (v1 * Mathf.Cos((movementTime - timePass) / movementTime * degree)) + (v2 * Mathf.Sin((movementTime - timePass) / movementTime * degree)) -
                ((v1 * Mathf.Cos((movementTime - currentTime) / movementTime * degree)) + (v2 * Mathf.Sin((movementTime - currentTime) / movementTime * degree)));
            currentTime -= time;
            onFramePass?.Invoke(movedObject, rigidbody, returned);
            return returned;
        }

        /// <summary>
        /// Returns whether the gradual movement should be stacked along with a series of movements; or only start when a series of movements has finished and define its own series of movements upon which other movements may stack.
        /// </summary>
        /// <returns>true for stacked, false for not stacked.</returns>
        public bool IsStacked()
        {
            return MovementIsStacked;
        }

        /// <summary>
        /// The function called by the manager when the movement has finished and been removed from the current movements of the assigned rigidbody/gameobject. The function will not be called when changing scenes, though the movements and their objects will nonetheless be removed.
        /// </summary>
        /// <param name="premature"> this will be true if the movement was marked as finished before the movement was completed, such as when the object is removed from movement. </param>
        public void OnMovementFinish(bool premature)
        {
            if (onMovementFinish != null)
                onMovementFinish(premature);
        }

        /// <summary>
        /// Defines whether the transformations influenced by this movement is to be done in world space or local space.
        /// </summary>
        /// <returns> true if world space, false if local space </returns>
        public bool IsWorldTranslation()
        {
            return isWorldSpace;
        }

        #endregion

    }

    /// <summary>
    /// Does an entire circular movement, starting at a point and ending at it as well, but passing through a given point that comes 180 degrees after itself.
    /// </summary>
    public class QGradualFullCircularMovement : QIGradualMovement
    {
        /// <summary>
        /// Defines all the parameters for a full gradual circular movement.
        /// </summary>
        [Serializable]
        public struct CircularMovementArgs
        {
            /// <summary>
            /// The starting position. If set to any other position than that of the gameobject, the gameobject will be moved to that position when the movement starts.
            /// </summary>
            public Vector3 startingPosition;
            /// <summary>
            /// The position the gameobject will be moving towards. This may not be the same as the starting point.
            /// </summary>
            public Vector3 finalPosition;
            /// <summary>
            /// The time it shall take for the movement to finish.
            /// </summary>
            public float time;
            /// <summary>
            /// Defines whether the gradual movement should be stacked along with a series of movements; or only start when a series of movements has finished and define its own series of movements upon which other movements may stack.
            /// </summary>
            public bool stacked;
            /// <summary>
            /// The reference point. The center of the circle will be between the line formed by the start and end points and a plane defined by this point. THIS POINT MAY NOT BE COLINEAR WITH THE START AND END POINTS
            /// </summary>
            public Vector3 referencePivot;
            /// <summary>
            /// whether this movement should be tried to be merged with other movements.
            /// </summary>
            public bool attemptMerge;
            /// <summary>
            ///  A function that is called when the movement finished, with the boolean indicating whether the movement finished or not. This function will not be called in scene changes.
            /// </summary>
            public Action<bool> onMovementFinish;

            /// <summary>
            ///  Defines whether the transformations influenced by this movement is to be done in world space or local space. Very different movements may thus be achieved. 
            /// </summary>
            public bool isWorldSpace;

            /// <summary>
            /// A function that is called each frame a movement takes place. The rigidbody passed in will be null if the object is not a rigidbody.
            /// </summary>
            public Action<GameObject, Rigidbody, Vector3> onFramePass;

            /// <summary>
            /// Defines a full circular movement.
            /// </summary>
            /// <param name="startingPosition"> The starting position. If set to any other position than that of the gameobject, the gameobject will be moved to that position when the movement starts. </param>
            /// <param name="finalPosition"> The position the gameobject will be moving towards. This may not be the same as the starting point. </param>
            /// <param name="time"> The time it shall take for the movement to finish. </param>
            /// <param name="stacked"> Defines whether the gradual movement should be stacked along with a series of movements; or only start when a series of movements has finished and define its own series of movements upon which other movements may stack. </param>
            /// <param name="referencePivot"> The reference point. The center of the circle will be between the line formed by the start and end points and a plane defined by this point. THIS POINT MAY NOT BE COLINEAR WITH THE START AND END POINTS </param>
            /// <param name="attemptMerge"> whether this movement should be tried to be merged with other movements. </param>
            /// <param name="isWorldSpace">  Defines whether the transformations influenced by this movement is to be done in world space or local space. Very different movements may thus be achieved. </param>
            /// <param name="onMovementFinish"> A function that is called when the movement finished, with the boolean indicating whether the movement finished or not. This function will not be called in scene changes. </param>
            /// <param name="onFramePass"> A function that is called each frame a movement takes place. The rigidbody passed in will be null if the object is not a rigidbody.</param>
            public CircularMovementArgs(Vector3 startingPosition, Vector3 finalPosition, Vector3 referencePivot, float time, bool isWorldSpace = true, bool stacked = false, bool attemptMerge = false,
                Action<bool> onMovementFinish = null, Action<GameObject, Rigidbody, Vector3> onFramePass = null)
            {
                this.startingPosition = startingPosition;
                this.finalPosition = finalPosition;
                this.referencePivot = referencePivot;
                this.time = time;
                this.stacked = stacked;
                this.attemptMerge = attemptMerge;
                this.onMovementFinish = onMovementFinish;
                this.isWorldSpace = isWorldSpace;
                this.onFramePass = onFramePass;
            }
        }

        private Vector3 initial, final, pivot;
        private float movementTime;
        private bool movementStacked;
        private float currentTime = -1;
        private GameObject movedObject;
        private Rigidbody rigidbody;
        private bool isRigidBodyMovement = false;
        private bool attemptMerge = false;
        private Action<bool> onMovementFinish;
        private bool isWorldSpace;
        private Action<GameObject, Rigidbody, Vector3> onFramePass;

        //TODO Ability to transform already created ellipsis for reuse? 

        #region Construction

        /// <summary>
        /// Initializes the circular movement
        /// </summary>
        /// <param name="gameObject"> The gameobject to be moved. </param>
        /// <param name="startingPosition"> The starting position. If set to any other position than that of the gameobject, the gameobject will be moved to that position when the movement starts. </param>
        /// <param name="finalPosition"> The position the gameobject will be moving towards. This may not be the same as the starting point. </param>
        /// <param name="time"> The time it shall take for the movement to finish. </param>
        /// <param name="stacked"> Defines whether the gradual movement should be stacked along with a series of movements; or only start when a series of movements has finished and define its own series of movements upon which other movements may stack. </param>
        /// <param name="referencePivot"> The reference point. The center of the circle will be between the line formed by the start and end points and a plane defined by this point. THIS POINT MAY NOT BE COLINEAR WITH THE START AND END POINTS </param>
        /// <param name="attemptMerge"> whether this movement should be tried to be merged with other movements. </param>
        /// <param name="isWorldSpace"> Defines whether the transformations influenced by this movement is to be done in world space or local space. Very different movements may thus be achieved. </param>
        /// <param name="onMovementFinish"> A function that is called when the movement finished, with the boolean indicating whether the movement finished or not. This function will not be called in scene changes. </param>
        /// <param name="onFramePass"> A function that is called each frame a movement takes place. The rigidbody passed in will be null if the object is not a rigidbody.</param>
        public QGradualFullCircularMovement(GameObject gameObject, Vector3 startingPosition, Vector3 finalPosition, Vector3 referencePivot, float time, bool isWorldSpace = true, bool stacked = false, bool attemptMerge = false,
            Action<bool> onMovementFinish = null, Action<GameObject, Rigidbody, Vector3> onFramePass = null)
        {
            if (QVectorCalculations.Vector3MathfApproximately(startingPosition, finalPosition))
                throw new Exception("The input starting and ending vectors cannot be the same!");
            if (QVectorCalculations.Vector3Colinear(startingPosition, finalPosition, referencePivot))
                throw new Exception("The reference pivot, starting position, and the final position cannot be colinear!");
            movedObject = gameObject;
            initial = startingPosition;
            final = finalPosition;
            pivot = referencePivot;
            movementTime = time;
            movementStacked = stacked;
            this.attemptMerge = attemptMerge;
            this.onMovementFinish = onMovementFinish;
            this.isWorldSpace = isWorldSpace;
            this.onFramePass = onFramePass;
        }

        /// <summary>
        /// Initializes the circular movement.
        /// </summary>
        /// <param name="gameObject">The gameobject to be moved.</param>
        /// <param name="args">the arguments that define the movement.</param>
        public QGradualFullCircularMovement(GameObject gameObject, CircularMovementArgs args)
        {
            movedObject = gameObject;
            initial = args.startingPosition;
            final = args.finalPosition;
            pivot = args.referencePivot;
            movementTime = args.time;
            movementStacked = args.stacked;
            this.attemptMerge = args.attemptMerge;
            this.onMovementFinish = args.onMovementFinish;
            if (QVectorCalculations.Vector3MathfApproximately(initial, final))
                throw new Exception("The input starting and ending vectors cannot be the same!");
            if (QVectorCalculations.Vector3Colinear(initial, initial, pivot))
                throw new Exception("The reference pivot, starting position, and the final position cannot be colinear!");
            this.isWorldSpace = args.isWorldSpace;
            this.onFramePass = args.onFramePass;
        }

        /// <summary>
        /// Initializes the circular movement
        /// </summary>
        /// <param name="rigidbody"> The rigidbody to be moved. </param>
        /// <param name="startingPosition"> The starting position. If set to any other position than that of the gameobject, the gameobject will be moved to that position when the movement starts. </param>
        /// <param name="finalPosition"> The position the gameobject will be moving towards. This may not be the same as the starting point. </param>
        /// <param name="time"> The time it shall take for the movement to finish. </param>
        /// <param name="stacked"> Defines whether the gradual movement should be stacked along with a series of movements; or only start when a series of movements has finished and define its own series of movements upon which other movements may stack. </param>
        /// <param name="referencePivot"> The reference point. The center of the circle will be between the line formed by the start and end points and a plane defined by this point. THIS POINT MAY NOT BE COLINEAR WITH THE START AND END POINTS </param>
        /// <param name="attemptMerge"> whether this movement should be tried to be merged with other movements. </param>
        /// <param name="isWorldSpace">  Defines whether the transformations influenced by this movement is to be done in world space or local space. Very different movements may thus be achieved. </param>
        /// <param name="onMovementFinish"> A function that is called when the movement finished, with the boolean indicating whether the movement finished or not. This function will not be called in scene changes. </param>
        /// <param name="onFramePass"> A function that is called each frame a movement takes place. The rigidbody passed in will be null if the object is not a rigidbody.</param>
        public QGradualFullCircularMovement(Rigidbody rigidbody, Vector3 startingPosition, Vector3 finalPosition, Vector3 referencePivot, float time, bool isWorldSpace = true, bool stacked = false, bool attemptMerge = false,
            Action<bool> onMovementFinish = null, Action<GameObject, Rigidbody, Vector3> onFramePass = null)
        {
            if (QVectorCalculations.Vector3MathfApproximately(startingPosition, finalPosition))
                throw new Exception("The input starting and ending vectors cannot be the same!");
            if (QVectorCalculations.Vector3Colinear(startingPosition, finalPosition, referencePivot))
                throw new Exception("The reference pivot, starting position, and the final position cannot be colinear!");
            isRigidBodyMovement = true;
            this.rigidbody = rigidbody;
            initial = startingPosition;
            final = finalPosition;
            pivot = referencePivot;
            movementTime = time;
            movementStacked = stacked;
            this.attemptMerge = attemptMerge;
            this.onMovementFinish = onMovementFinish;
            this.isWorldSpace = isWorldSpace;
            movedObject = rigidbody.gameObject;
            this.onFramePass = onFramePass;
        }

        /// <summary>
        /// Initializes the circular movement.
        /// </summary>
        /// <param name="rigidbody">The rigidbody to be moved.</param>
        /// <param name="args">the arguments that define the movement.</param>
        public QGradualFullCircularMovement(Rigidbody rigidbody, CircularMovementArgs args)
        {
            this.rigidbody = rigidbody;
            isRigidBodyMovement = true;
            initial = args.startingPosition;
            final = args.finalPosition;
            pivot = args.referencePivot;
            movementTime = args.time;
            movementStacked = args.stacked;
            this.attemptMerge = args.attemptMerge;
            this.onMovementFinish = args.onMovementFinish;
            if (QVectorCalculations.Vector3MathfApproximately(initial, final))
                throw new Exception("The input starting and ending vectors cannot be the same!");
            if (QVectorCalculations.Vector3Colinear(initial, initial, pivot))
                throw new Exception("The reference pivot, starting position, and the final position cannot be colinear!");
            this.isWorldSpace = args.isWorldSpace;
            movedObject = rigidbody.gameObject;
            this.onFramePass = args.onFramePass;
        }


        #endregion

        #region Manager Communication and Manager Related Properties

        Vector3 v1, v2;

        /// <summary>
        /// Starts the defined movement if it has not already been started.
        /// </summary>
        public void StartMovement()
        {
            if (currentTime > 0)
                return;
            //construct the real pivot.
            movedObject.transform.position = initial;
            Vector3 middle = QVectorCalculations.GetVector3Middle(initial, final);
            Vector3 nearest = QVectorCalculations.GetNearestPointOnLine(middle, (final - initial), pivot);
            Vector3 dir = (pivot - nearest).normalized;
            Vector3 realPivot = middle;

            //construct circle.
            v1 = (initial - realPivot).normalized * (initial - realPivot).magnitude;
            v2 = dir * (initial - realPivot).magnitude;
            pivot = realPivot;

            currentTime = movementTime;

            //do the movement
            if (isRigidBodyMovement)
            {
                //rigidbody movement
                QGradualMovementManager.AddGradualMovement(rigidbody, this, movementStacked, attemptMerge);
            }
            else
            {
                //regular movement.
                QGradualMovementManager.AddGradualMovement(movedObject, this, movementStacked, attemptMerge);
            }
        }

        /// <summary>
        /// whether the gradual movement should be stacked along with a series of movements; or only start when a series of movements has finished and define its own series of movements upon which other movements may stack.
        /// </summary>
        public bool MovementIsStacked { get; set; }

        /// <summary>
        /// Attempts to merge the input movement into this current movement. These are often implemented such that similar types of movement, and especially linear ones over similar times, can be handled as a single movement instead to increase performance.
        /// </summary>
        /// <param name="mov"> The movement to be merged. </param>
        /// <returns> whether the input movement has been integrated into this movement. If true, this class is now responsible for adding the displacement of the other movement into itself. If false, the movement that has been attempted to be merged will be queued normally.</returns>
        public bool AttemptMerge(QIGradualMovement mov)
        {
            return false; //TODO AT LEAST IMPLEMENT SOME MERGES!
        }

        /// <summary>
        /// Based on the time calls the manager has made, returns the amount of time left before the movement is done and the GetApplyTransformation function returns negative infinity.
        /// </summary>
        /// <returns>the amount of time left before the movement is done and the GetApplyTransformation function returns negative infinity. Returns a negative value if there is no movement taking place currently</returns>
        public float GetTimeLeft()
        {
            return currentTime;
        }

        /// <summary>
        /// USED CHIEFLY BY THE MOVEMENT MANAGER. Returns the displacement of the object as affected by this gradual movement, while also keeping track of the time passed. If the gradual movement is done, returns negative infinity to signal so.
        /// </summary>
        /// <param name="time"> the time that has passed since the last frame. </param>
        /// <returns> the displacement to take place within the frame. Negative infinity if the movement is done. </returns>
        public Vector3 GetApplyTransformation(float time)
        {
            if (currentTime <= 0)
                return Vector3.negativeInfinity;
            float timePass = currentTime - time < 0 ? 0 : currentTime - time;
            Vector3 returned = (v1 * Mathf.Cos((movementTime - timePass) / movementTime * 2 * Mathf.PI)) + (v2 * Mathf.Sin((movementTime - timePass) / movementTime * 2 * Mathf.PI)) -
                ((v1 * Mathf.Cos((movementTime - currentTime) / movementTime * 2 * Mathf.PI)) + (v2 * Mathf.Sin((movementTime - currentTime) / movementTime * 2 * Mathf.PI)));
            currentTime -= time;
            onFramePass?.Invoke(movedObject, rigidbody, returned);
            return returned;
        }

        /// <summary>
        /// Returns whether the gradual movement should be stacked along with a series of movements; or only start when a series of movements has finished and define its own series of movements upon which other movements may stack.
        /// </summary>
        /// <returns>true for stacked, false for not stacked.</returns>
        public bool IsStacked()
        {
            return MovementIsStacked;
        }

        /// <summary>
        /// The function called by the manager when the movement has finished and been removed from the current movements of the assigned rigidbody/gameobject. The function will not be called when changing scenes, though the movements and their objects will nonetheless be removed.
        /// </summary>
        /// <param name="premature"> this will be true if the movement was marked as finished before the movement was completed, such as when the object is removed from movement. </param>
        public void OnMovementFinish(bool premature)
        {
            if (onMovementFinish != null)
                onMovementFinish(premature);
        }

        /// <summary>
        /// Defines whether the transformations influenced by this movement is to be done in world space or local space.
        /// </summary>
        /// <returns> true if world space, false if local space </returns>
        public bool IsWorldTranslation()
        {
            return isWorldSpace;
        }

        #endregion
    }

}
