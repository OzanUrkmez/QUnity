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

        private Vector3 initial, final, pivot;
        private float degree, xCoeff, yCoeff; //we make circles nao
        private float movementTime, lastInputTime;
        private bool movementStacked;
        private float currentTime = -1;
        private GameObject movedObject;
        private Rigidbody rigidbody;
        private bool isRigidBodyMovement = false;

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
        /// <param name="degree"> how many degrees of the circle the movement will comprise. This value must be 10 and 180 </param>
        public QGradualCircularMovement(GameObject gameObject, Vector3 startingPosition, Vector3 finalPosition, Vector3 referencePivot, float degree , float time, bool stacked)
        {
            if (degree < 10 || degree > 180)
                throw new Exception("The degree input into a gradual circular movement must be between 10 and 180.");
            if (QVectorCalculations.Vector3MathfApproximately(startingPosition, finalPosition))
                throw new Exception("The input starting and ending vectors cannot be the same!");
            if (QVectorCalculations.Vector3Colinear(startingPosition, finalPosition, referencePivot))
                throw new Exception("The reference pivot, starting position, and the final position cannot be colinear!");
            movedObject = gameObject;
            initial = startingPosition;
            final = finalPosition;
            pivot = referencePivot;
            movementTime = time;
            lastInputTime = time;
            this.degree = degree;
            movementStacked = stacked;
        }

        /// <summary>
        /// Initializes the circular movement
        /// </summary>
        /// <param name="rigidbody"> The gameobject to be moved. </param>
        /// <param name="startingPosition"> The starting position. If set to any other position than that of the gameobject, the gameobject will be moved to that position when the movement starts. </param>
        /// <param name="finalPosition"> The position the gameobject will be moving towards. This may not be the same as the starting point. </param>
        /// <param name="time"> The time it shall take for the movement to finish. </param>
        /// <param name="stacked"> Defines whether the gradual movement should be stacked along with a series of movements; or only start when a series of movements has finished and define its own series of movements upon which other movements may stack. </param>
        /// <param name="referencePivot"> The reference point. The center of the circle will be between the line formed by the start and end points and a plane defined by this point. THIS POINT MAY NOT BE COLINEAR WITH THE START AND END POINTS </param>
        /// <param name="degree"> how many degrees of the circle the movement will comprise. This value must be 10 and 180 </param>
        public QGradualCircularMovement(Rigidbody rigidbody, Vector3 startingPosition, Vector3 finalPosition, Vector3 referencePivot, float degree, float time, bool stacked)
        {
            if (degree < 10 || degree > 180)
                throw new Exception("The degree input into a gradual circular movement must be between 10 and 180.");
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
            lastInputTime = time;
            this.degree = degree;
            movementStacked = stacked;
        }

        #endregion


        #region Manager Communication and Manager Related Properties

        /// <summary>
        /// Starts the defined movement if it has not already been started.
        /// </summary>
        public void StartMovement()
        {
            if (currentTime > 0)
                return;
            if (isRigidBodyMovement)
            {
                //rigidbody movement
            }
            else
            {
                //regular movement.
                movedObject.transform.position = initial;
                Vector3 middle = QVectorCalculations.GetVector3Middle(initial, final);
                Vector3 dir = pivot - QVectorCalculations.GetNearestPointOnLine(middle, (final - initial), pivot);
                Vector3 realPivot = dir * (final - initial).magnitude * Mathf.Pow(Mathf.Tan(Mathf.Deg2Rad * degree / 2), -1);

            }
        }

        /// <summary>
        /// whether the gradual movement should be stacked along with a series of movements; or only start when a series of movements has finished and define its own series of movements upon which other movements may stack.
        /// </summary>
        public bool MovementIsStacked { get; set; }

        public bool AttemptMerge(QIGradualMovement mov)
        {
            throw new NotImplementedException();
        }

        public float GetTimeLeft()
        {
            throw new NotImplementedException();
        }

        public Vector3 GetTransformation(float time)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns whether the gradual movement should be stacked along with a series of movements; or only start when a series of movements has finished and define its own series of movements upon which other movements may stack.
        /// </summary>
        /// <returns>true for stacked, false for not stacked.</returns>
        public bool IsStacked()
        {
            return MovementIsStacked;
        }

        public void OnMovementFinish(bool premature)
        {
            throw new NotImplementedException();
        }

        #endregion

    }

    /// <summary>
    /// Does an entire circular movement, starting at a point and ending at it as well, but passing through a given point that comes 180 degrees after itself.
    /// </summary>
    public class QGradualFullCircularMovement
    {

    }

}
