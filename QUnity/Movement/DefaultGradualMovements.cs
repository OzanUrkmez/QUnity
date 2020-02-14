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
        private GameObject movedObject;

        /// <summary>
        /// Initializes the elliptical movement as being a 90 degree rotation.
        /// </summary>
        /// <param name="gameObject"> The gameobject to be moved. </param>
        /// <param name="startingPosition"> The starting position. If set to  </param>
        /// <param name="finalPosition"></param>
        /// <param name="time"></param>
        public QGradualEllipticalMovement(GameObject gameObject, Vector3 startingPosition, Vector3 finalPosition, float time)
        {

        }

        #region Manager Communication

        public bool AttemptMerge(QIGradualMovement mov)
        {
            throw new NotImplementedException();
        }

        public Vector3 GetTransformation(float time)
        {
            throw new NotImplementedException();
        }

        public bool IsStacked()
        {
            throw new NotImplementedException();
        }

        public void OnMovementFinish(bool premature)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
