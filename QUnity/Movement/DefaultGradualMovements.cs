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
    /// Defines an elliptical gradual movement integrated into the Questry Gradual Movement System. 
    /// </summary>
    public class QGradualEllipticalMovement : QIGradualMovement
    {

        private Vector3 initial, final, circularPivot;
        private float xCoeff, yCoeff;
        private float movementTime;
        private float currentTime;
        private GameObject movedObject;
        private Rigidbody rigidbody;

        //TODO Ability to transform already created ellipsis for reuse? 

        /// <summary>
        /// Initializes the elliptical movement
        /// </summary>
        /// <param name="gameObject"> The gameobject to be moved. </param>
        /// <param name="startingPosition"> The starting position. If set to  </param>
        /// <param name="finalPosition"></param>
        /// <param name="time"></param>
        /// <param name="stacked"> Defines whether the gradual movement should be stacked along with a series of movements; or only start when a series of movements has finished and define its own series of movements upon which other movements may stack. </param>
        /// <param name="referencePivot"></param>
        /// <param name="degree"> </param>
        public QGradualEllipticalMovement(GameObject gameObject, Vector3 startingPosition, Vector3 finalPosition, Vector3 referencePivot, float degree , float time, bool stacked)
        {

        }

        #region Manager Communication and Manager Related Properties

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
}
