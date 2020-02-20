using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace QUnity.Movement
{
    /// <summary>
    /// The main manager of all gradual movements that follows a singleton structure. All gradual movements and objects that are gradually moved reference this manager. If implementing your own gradual movement, you should make it work with this class.
    /// </summary>
    public class QGradualMovementManager : MonoBehaviour
    {
        #region Unity and Instantiation

        [SerializeField]
        private QGradualMovementProperties defaultProperties;

        private static QGradualMovementManager singleton;

        private void Start()
        {

            if (singleton != null)
            {
                Destroy(gameObject);
                return;
            }
            singleton = this;
            currentProperties = defaultProperties;
            if (currentProperties.dontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);
            SceneManager.activeSceneChanged += OnSceneChanged;
            EnableMovementEnumeration();
        }

        private void OnDestroy()
        {
            if (singleton == this)
            {
                DisableMovementEnumeration();
                gradualMovements = new Dictionary<GameObject, Queue<QIGradualMovement>>();
                rigidGradualMovements = new Dictionary<Rigidbody, Queue<QIGradualMovement>>();
                currentGradualMovements = new Dictionary<GameObject, AggregateGradualMovement>();
                currentRigidGradualMovements = new Dictionary<Rigidbody, AggregateGradualMovement>();
                singleton = null;
            }
        }

        private void OnSceneChanged(Scene oldScene, Scene newScene)
        {
            gradualMovements = new Dictionary<GameObject, Queue<QIGradualMovement>>();
            rigidGradualMovements = new Dictionary<Rigidbody, Queue<QIGradualMovement>>();
            currentGradualMovements = new Dictionary<GameObject, AggregateGradualMovement>();
            currentRigidGradualMovements = new Dictionary<Rigidbody, AggregateGradualMovement>();
        }

        #endregion

        #region 

        #endregion

        #region Enumeration and Activeness

        private bool enumerationActive = false;

        /// <summary>
        /// Disables the movement enumerations, essentially pausing all gradual movements.
        /// </summary>
        public static void DisableMovementEnumeration()
        {
            singleton.enumerationActive = false;
        }

        /// <summary>
        /// Enables the movement enumerations, essentially unpausing all gradual movements if the manager had been paused.
        /// </summary>
        public static void EnableMovementEnumeration()
        {
            singleton.enumerationActive = true;
        }
        List<QIGradualMovement> toBeRemoved = new List<QIGradualMovement>();
        List<GameObject> objectsToBeRemoved = new List<GameObject>();
        List<Rigidbody> rigidbodiesToBeRemoved = new List<Rigidbody>();
        private IEnumerator MovementEnumeration()
        {
            while (enumerationActive)
            {
                float time = 0f;
                //waiting happens here.
                if (currentProperties.useFixedUpdate)
                {
                    yield return new WaitForFixedUpdate();
                    time = Time.fixedDeltaTime;
                }
                else
                {
                    yield return new WaitForEndOfFrame();
                    time = Time.deltaTime;
                }
                //code goes here.
                Vector3 currentAggregateTransformation;
                toBeRemoved.Clear();
                objectsToBeRemoved.Clear();
                rigidbodiesToBeRemoved.Clear();
                //handle regular
                foreach (GameObject g in currentGradualMovements.Keys)
                {
                    AggregateGradualMovement mov = currentGradualMovements[g];
                    if (mov.paused)
                        continue;
                    currentAggregateTransformation = Vector3.zero;
                    toBeRemoved.Clear();
                    foreach (QIGradualMovement qmov in mov.movements)
                    {
                        Vector3 trans = qmov.GetApplyTransformation(time);
                        if(trans == Vector3.negativeInfinity)
                        {
                            //the movement is done! remove it handle logic and then continue.
                            toBeRemoved.Add(qmov);
                            continue;
                        }
                        currentAggregateTransformation += trans;
                    }
                    //do the transformation
                    g.transform.Translate(currentAggregateTransformation);
                    foreach (QIGradualMovement qmov in toBeRemoved)
                    {
                        if (!mov.movements.Contains(qmov))
                            continue;
                        if(qmov.IsStacked())
                        {
                            //just a stacked movement that has finished.
                            qmov.OnMovementFinish(false);
                            mov.movements.Remove(qmov);
                        }
                        else
                        {
                            //the non stacked movement has finished!
                            qmov.OnMovementFinish(false);
                            mov.RemoveStaticMovement();
                            OnCurrentMovementEnd(g);
                        }
                    }
                    if(mov.movements.Count == 0)
                    {
                        //movement of this object has completely finished.
                        objectsToBeRemoved.Add(g);
                    }
                }
                foreach(GameObject g in objectsToBeRemoved)
                {
                    gradualMovements.Remove(g);
                    currentGradualMovements.Remove(g);
                }
                //handle rigid
                foreach (Rigidbody rb in currentRigidGradualMovements.Keys)
                {
                    AggregateGradualMovement mov = currentRigidGradualMovements[rb];
                    if (mov.paused)
                        continue;
                    currentAggregateTransformation = Vector3.zero;
                    toBeRemoved.Clear();
                    foreach (QIGradualMovement qmov in mov.movements)
                    {
                        Vector3 trans = qmov.GetApplyTransformation(time);
                        if (trans == Vector3.negativeInfinity)
                        {
                            //the movement is done! remove it handle logic and then continue.
                            toBeRemoved.Add(qmov);
                            continue;
                        }
                        currentAggregateTransformation += trans;
                    }
                    //do the transformation
                    if (currentProperties.useRigidBodyMovement)
                        rb.MovePosition(rb.transform.position + currentAggregateTransformation);
                    else
                        rb.position = rb.transform.position + currentAggregateTransformation;
                    foreach (QIGradualMovement qmov in toBeRemoved)
                    {
                        if (!mov.movements.Contains(qmov))
                            continue;
                        if (qmov.IsStacked())
                        {
                            //just a stacked movement that has finished.
                            qmov.OnMovementFinish(false);
                            mov.movements.Remove(qmov);
                        }
                        else
                        {
                            //the non stacked movement has finished!
                            qmov.OnMovementFinish(false);
                            mov.RemoveStaticMovement();
                            OnCurrentRigidMovementEnd(rb);
                        }
                    }
                    if (mov.movements.Count == 0)
                    {
                        //movement of this object has completely finished.
                        rigidbodiesToBeRemoved.Add(rb);
                    }
                }
                foreach (Rigidbody rb in rigidbodiesToBeRemoved)
                {
                    rigidGradualMovements.Remove(rb);
                    currentRigidGradualMovements.Remove(rb);
                }
            }
        }


        #endregion

        #region Manager Properties

        private QGradualMovementProperties currentProperties = new QGradualMovementProperties();

        /// <summary>
        /// A struct to define the properties that the gradual movement manager abides by.
        /// </summary>
        [Serializable]
        public struct QGradualMovementProperties
        {
            /// <summary>
            /// determines whether the gradual movement manager should use fixed update or the regular update intervals.
            /// </summary>
            public bool useFixedUpdate;
            /// <summary>
            /// For rigidbodies that are entered, uses the Rigidbody.MovePosition function when set to true rather than the rigidbody.position parameter.
            /// </summary>
            public bool useRigidBodyMovement;

            /// <summary>
            /// setting this to true will make it so that the gradual movement manager will not be destroyed when switching scenes, making it so that the properties are not lost (movement data from prior scene will be lost regardless.)
            /// if you set this to false, it will be possible to have different managers for different scenes working with different properties. 
            /// </summary>
            public bool dontDestroyOnLoad;

            /// <summary>
            /// instantiates a properties struct.
            /// </summary>
            /// <param name="useFixedUpdate"> determines whether the gradual movement manager should use fixed update or the regular update intervals.  </param>
            /// <param name="useRigidBodyMovement">  For rigidbodies that are entered, uses the Rigidbody.MovePosition function when set to true rather than the rigidbody.position parameter. </param>
            /// <param name="dontDestroyOnLoad">
            /// setting this to true will make it so that the gradual movement manager will not be destroyed when switching scenes, making it so that the properties are not lost (movement data from prior scene will be lost regardless.)
            /// if you set this to false, it will be possible to have different managers for different scenes working with different properties. 
            /// </param>
            public QGradualMovementProperties(bool useFixedUpdate, bool useRigidBodyMovement, bool dontDestroyOnLoad)
            {
                this.useFixedUpdate = useFixedUpdate;
                this.useRigidBodyMovement = useRigidBodyMovement;
                this.dontDestroyOnLoad = dontDestroyOnLoad;
            }
        }

        /// <summary>
        /// returns the current properties that the manager is abiding by.
        /// </summary>
        /// <returns> the current properties that the manager is abiding by. </returns>
        public static QGradualMovementProperties GetCurrentProperties()
        {
            return singleton.currentProperties;
        }

        /// <summary>
        /// Changes the properties that the manager is abiding by.
        /// </summary>
        /// <param name="newProperties"> the new properties struct </param>
        public static void ChangeProperties(QGradualMovementProperties newProperties)
        {
            HandlePropertiesChange(singleton.currentProperties, newProperties);
            singleton.currentProperties = newProperties;
        }

        private static void HandlePropertiesChange(QGradualMovementProperties oldProperties, QGradualMovementProperties newProperties)
        {
            if(!oldProperties.dontDestroyOnLoad && newProperties.dontDestroyOnLoad)
            {
                //change to dont destroy on load.
                DontDestroyOnLoad(singleton);
            }else if(oldProperties.dontDestroyOnLoad && !newProperties.dontDestroyOnLoad)
            {
                //change to DO destroy on load.
                SceneManager.MoveGameObjectToScene(singleton.gameObject, SceneManager.GetActiveScene());
            }
        }

        #endregion

        #region Gradual Movements Addition and Manipulation

        #region Properties

        private class AggregateGradualMovement
        {
            public List<QIGradualMovement> movements;
            public bool paused;
            public int aggregateNonStacked = 0;
            public QIGradualMovement currentStaticMovement = null;

            public AggregateGradualMovement()
            {
                movements = new List<QIGradualMovement>();
                paused = false;
                currentStaticMovement = null;
                aggregateNonStacked = 0;
            }

            public void RemoveStaticMovement()
            {
                if (currentStaticMovement == null)
                    return;
                movements.Remove(currentStaticMovement);
                currentStaticMovement = null;
                aggregateNonStacked--;
            }
        }

        private static Dictionary<GameObject, Queue<QIGradualMovement>> gradualMovements = new Dictionary<GameObject, Queue<QIGradualMovement>>();
        private static Dictionary<Rigidbody, Queue<QIGradualMovement>> rigidGradualMovements = new Dictionary<Rigidbody, Queue<QIGradualMovement>>();

        private static Dictionary<GameObject, AggregateGradualMovement> currentGradualMovements = new Dictionary<GameObject, AggregateGradualMovement>();
        private static Dictionary<Rigidbody, AggregateGradualMovement> currentRigidGradualMovements = new Dictionary<Rigidbody, AggregateGradualMovement>();

        #endregion

        #region Inner Manipulations

        private static void OnCurrentMovementEnd(GameObject g)
        {
            AggregateGradualMovement aggr = currentGradualMovements[g];
            Queue<QIGradualMovement> queue = gradualMovements[g];
            QIGradualMovement mov;
            while (queue.Count > 0)
            {
                mov = queue.Dequeue();
                aggr.movements.Add(mov);
                if (!mov.IsStacked())
                {
                    aggr.currentStaticMovement = mov;
                    break;
                }
            }
        }

        private static void OnCurrentRigidMovementEnd(Rigidbody rb)
        {
            AggregateGradualMovement aggr = currentRigidGradualMovements[rb];
            Queue<QIGradualMovement> queue = rigidGradualMovements[rb];
            QIGradualMovement mov;
            while (queue.Count > 0)
            {
                mov = queue.Dequeue();
                aggr.movements.Add(mov);
                if (!mov.IsStacked())
                {
                    aggr.currentStaticMovement = mov;
                    break;
                }
            }
        }

        #endregion

        #region Addition

        /// <summary>
        /// Adds a gameobject to be moved gradually.
        /// </summary>
        /// <param name="g"> the gameobject to be moved. </param>
        /// <param name="movement"> the well-defined gradual movement </param>
        /// <param name="stackWithCurrent"> if this is true and the passed movement is a stacking one, stacks the movement with the current movements being executed for the gameobject. If all movements added so far to the object is already stacked, this boolean will not matter.</param>
        /// <param name="attemptMerge"> attempts to merge this movement with other ones for the object. Stacking and non-stacking movements will never be merged. Keep in mind that when this is checked, though it may potentially increase efficiency for the future, can also produce a process-intensive function-call. </param>
        public static void AddGradualMovement(GameObject g, QIGradualMovement movement, bool stackWithCurrent = false, bool attemptMerge = false)
        {
            if (!gradualMovements.ContainsKey(g))
            {
                //did not exist before. REMEMBER TO DELETE A MOVEMENT IF IT HAS NO MORE WITHIN THE QUEUE. THERE MUST ALWAYS BE A CURRENT MOVEMENT FOR A MOVEMENT. 
                gradualMovements.Add(g, new Queue<QIGradualMovement>());
                currentGradualMovements.Add(g, new AggregateGradualMovement());
                currentGradualMovements[g].movements.Add(movement);
                if (!movement.IsStacked())
                {
                    currentGradualMovements[g].aggregateNonStacked++;
                    currentGradualMovements[g].currentStaticMovement = movement;
                }
            }
            else
            {
                //did exist before.

                if (currentGradualMovements[g].aggregateNonStacked != 0)
                {
                    if (movement.IsStacked())
                    {
                        //the movement is one that stacks.
                        if (stackWithCurrent)
                        {
                            //attempt to merge the current input movement with one of the current ones. 
                            if (attemptMerge)
                            {
                                bool merged = false;
                                foreach (QIGradualMovement mov in currentGradualMovements[g].movements)
                                {
                                    if (mov.IsStacked() && mov.AttemptMerge(movement))
                                    {
                                        merged = true;
                                        break;
                                    }
                                }
                                if (merged)
                                {
                                    return;
                                }
                            }
                            currentGradualMovements[g].movements.Add(movement);
                        }
                        else
                        {
                            //attempt to merge the current input movement with one of the stacked ones that are enqueued within the latest group. 
                            if (attemptMerge)
                            {
                                bool merged = false;
                                QIGradualMovement[] traversedMovements = gradualMovements[g].ToArray();
                                for (int i = traversedMovements.Length - 1; i > -1; i++)
                                {

                                    if (!traversedMovements[i].IsStacked())
                                        break;
                                    else if (traversedMovements[i].AttemptMerge(movement))
                                    {
                                        merged = true;
                                        break;
                                    }
                                }
                                if (merged)
                                {
                                    return;
                                }
                            }
                            //otherwise add it to the queue.
                            gradualMovements[g].Enqueue(movement);
                        }
                    }
                    else
                    {
                        //attempt to merge the current input movement with one of the stacked ones that are enqueued within the latest group. 
                        if (attemptMerge)
                        {
                            bool merged = false;
                            QIGradualMovement[] traversedMovements = gradualMovements[g].ToArray();
                            for (int i = traversedMovements.Length - 1; i > -1; i++)
                            {

                                if (traversedMovements[i].IsStacked())
                                    continue;
                                else if(traversedMovements[i].AttemptMerge(movement))
                                {
                                    merged = true;
                                }
                                break;
                            }
                            if (merged)
                            {
                                return;
                            }
                        }
                        //otherwise add it to the queue.
                        
                        gradualMovements[g].Enqueue(movement);
                        currentGradualMovements[g].aggregateNonStacked++;
                    }
                }
                else
                {
                    //special case in which everything is stacked up to now.
                    if (!movement.IsStacked())
                    {
                        //wont check for merging as is the only non-stacked.
                        currentGradualMovements[g].aggregateNonStacked++;
                        currentGradualMovements[g].currentStaticMovement = movement;
                    }
                    else
                    {
                        //attempt to merge the current input movement with one of the current ones. 
                        if (attemptMerge)
                        {
                            bool merged = false;
                            foreach (QIGradualMovement mov in currentGradualMovements[g].movements)
                            {
                                if (mov.AttemptMerge(movement))
                                {
                                    merged = true;
                                    break;
                                }
                            }
                            if (merged)
                            {
                                return;
                            }
                        }
                    }
                    currentGradualMovements[g].movements.Add(movement);
                }
            }
        }

        /// <summary>
        /// Adds a gameobject to be moved gradually.
        /// </summary>
        /// <param name="rb"> the rigidbody to be moved. </param>
        /// <param name="movement"> the well-defined gradual movement </param>
        /// <param name="stackWithCurrent"> if this is true and the passed movement is a stacking one, stacks the movement with the current movements being executed for the gameobject. If all movements added so far to the object is already stacked, this boolean will not matter. </param>
        /// <param name="attemptMerge"> attempts to merge this movement with other ones for the object. Stacking and non-stacking movements will never be merged. Keep in mind that when this is checked, though it may potentially increase efficiency for the future, can also produce a process-intensive function-call. </param>
        public static void AddGradualMovement(Rigidbody rb, QIGradualMovement movement, bool stackWithCurrent = false, bool attemptMerge = false)
        {
            if (!rigidGradualMovements.ContainsKey(rb))
            {
                //did not exist before. REMEMBER TO DELETE A MOVEMENT IF IT HAS NO MORE WITHIN THE QUEUE. THERE MUST ALWAYS BE A CURRENT MOVEMENT FOR A MOVEMENT. 
                rigidGradualMovements.Add(rb, new Queue<QIGradualMovement>());
                currentRigidGradualMovements.Add(rb, new AggregateGradualMovement());
                currentRigidGradualMovements[rb].movements.Add(movement);
                if (!movement.IsStacked())
                {
                    currentRigidGradualMovements[rb].aggregateNonStacked++;
                    currentRigidGradualMovements[rb].currentStaticMovement = movement;
                }
            }
            else
            {
                //did exist before.

                if (currentRigidGradualMovements[rb].aggregateNonStacked != 0)
                {
                    if (movement.IsStacked())
                    {
                        //the movement is one that stacks.
                        if (stackWithCurrent)
                        {
                            //attempt to merge the current input movement with one of the current ones. 
                            if (attemptMerge)
                            {
                                bool merged = false;
                                foreach (QIGradualMovement mov in currentRigidGradualMovements[rb].movements)
                                {
                                    if (mov.IsStacked() && mov.AttemptMerge(movement))
                                    {
                                        merged = true;
                                        break;
                                    }
                                }
                                if (merged)
                                {
                                    return;
                                }
                            }
                            currentRigidGradualMovements[rb].movements.Add(movement);
                        }
                        else
                        {
                            //attempt to merge the current input movement with one of the stacked ones that are enqueued within the latest group. 
                            if (attemptMerge)
                            {
                                bool merged = false;
                                QIGradualMovement[] traversedMovements = rigidGradualMovements[rb].ToArray();
                                for (int i = traversedMovements.Length - 1; i > -1; i++)
                                {

                                    if (!traversedMovements[i].IsStacked())
                                        break;
                                    else if (traversedMovements[i].AttemptMerge(movement))
                                    {
                                        merged = true;
                                        break;
                                    }
                                }
                                if (merged)
                                {
                                    return;
                                }
                            }
                            //otherwise add it to the queue.
                            rigidGradualMovements[rb].Enqueue(movement);
                        }
                    }
                    else
                    {
                        //attempt to merge the current input movement with one of the stacked ones that are enqueued within the latest group. 
                        if (attemptMerge)
                        {
                            bool merged = false;
                            QIGradualMovement[] traversedMovements = rigidGradualMovements[rb].ToArray();
                            for (int i = traversedMovements.Length - 1; i > -1; i++)
                            {

                                if (traversedMovements[i].IsStacked())
                                    continue;
                                else if (traversedMovements[i].AttemptMerge(movement))
                                {
                                    merged = true;
                                }
                                break;
                            }
                            if (merged)
                            {
                                return;
                            }
                        }
                        //otherwise add it to the queue.

                        rigidGradualMovements[rb].Enqueue(movement);
                        currentRigidGradualMovements[rb].aggregateNonStacked++;
                    }
                }
                else
                {
                    //special case in which everything is stacked up to now.
                    if (!movement.IsStacked())
                    {
                        //wont check for merging as is the only non-stacked.
                        currentRigidGradualMovements[rb].aggregateNonStacked++;
                        currentRigidGradualMovements[rb].currentStaticMovement = movement;
                    }
                    else
                    {
                        //attempt to merge the current input movement with one of the current ones. 
                        if (attemptMerge)
                        {
                            bool merged = false;
                            foreach (QIGradualMovement mov in currentRigidGradualMovements[rb].movements)
                            {
                                if (mov.AttemptMerge(movement))
                                {
                                    merged = true;
                                    break;
                                }
                            }
                            if (merged)
                            {
                                return;
                            }
                        }
                    }
                    currentRigidGradualMovements[rb].movements.Add(movement);
                }
            }
        }

        #endregion

        #region Manipulation

        #region Pausing

        /// <summary>
        /// pauses or unpauses the gradual movement of the input gameobject.
        /// </summary>
        /// <param name="g">the input gameobject</param>
        /// <param name="pause"> whether the gameobject should be paused or unpaused </param>
        public static void PauseGradualMovement(GameObject g, bool pause)
        {
            if (!currentGradualMovements.ContainsKey(g))
                return;
            currentGradualMovements[g].paused = pause;
        }

        /// <summary>
        /// pauses or unpauses the gradual movement of the input rigidbody.
        /// </summary>
        /// <param name="rb">the input rigidbody</param>
        /// <param name="pause"> whether the rigidbody should be paused or unpaused </param>
        public static void PauseGradualMovement(Rigidbody rb, bool pause)
        {
            if (!currentRigidGradualMovements.ContainsKey(rb))
                return;
            currentRigidGradualMovements[rb].paused = pause;
        }

        #endregion

        #region Removing

        /// <summary>
        /// Removes the gameobject from the gradual movement manager, ending all movements and enqueued movements.
        /// </summary>
        /// <param name="g"> the gameobject whose movements are supposed to be removed. </param>
        /// <returns> true if the object was removed, false if it didn't exist in the system in the first place. </returns>
        public static bool RemoveGradualMovement(GameObject g)
        {
            if (!gradualMovements.ContainsKey(g))
                return false;
            foreach(QIGradualMovement qgm in gradualMovements[g])
            {
                qgm.OnMovementFinish(true);
            }
            foreach (QIGradualMovement qgm in currentGradualMovements[g].movements)
            {
                qgm.OnMovementFinish(true);
            }
            gradualMovements.Remove(g);
            currentGradualMovements.Remove(g);
            return true;
        }

        /// <summary>
        /// Removes the rigidbody from the gradual movement manager, ending all movements and enqueued movements.
        /// </summary>
        /// <param name="rb"> the rigidbody whose movements are supposed to be removed. </param>
        /// <returns> true if the object was removed, false if it didn't exist in the system in the first place. </returns>
        public static bool RemoveGradualMovement(Rigidbody rb)
        {
            if (!rigidGradualMovements.ContainsKey(rb))
                return false;
            foreach (QIGradualMovement qgm in rigidGradualMovements[rb])
            {
                qgm.OnMovementFinish(true);
            }
            foreach (QIGradualMovement qgm in currentRigidGradualMovements[rb].movements)
            {
                qgm.OnMovementFinish(true);
            }
            rigidGradualMovements.Remove(rb);
            currentRigidGradualMovements.Remove(rb);
            return true;
        }

        /// <summary>
        /// Removes the current gradual movement of the gameobject, moving to the next or group of movements removing the object from the system if there are no movements left.
        /// </summary>
        /// <param name="g"> the gameobject </param>
        /// <param name="removeStacked"> if set to true, the stacked movements that are currently active will also be removed, preventing them from being carried over if they have not yet been completed. </param>
        public static void RemoveCurrentGradualMovement(GameObject g, bool removeStacked = false)
        {
            if (!currentGradualMovements.ContainsKey(g))
                return;
            currentGradualMovements[g].currentStaticMovement.OnMovementFinish(true);
            currentGradualMovements[g].RemoveStaticMovement();
            if (removeStacked)
            {
                currentGradualMovements[g].movements.Clear();
                foreach (QIGradualMovement qgm in currentGradualMovements[g].movements)
                {
                    if(qgm.IsStacked())
                        qgm.OnMovementFinish(true);
                }
            }
            OnCurrentMovementEnd(g);
        }

        /// <summary>
        /// Removes the current gradual movement of the rigidbody, moving to the next group of movements or removing the object from the system if there are no movements left.
        /// </summary>
        /// <param name="rb"> the rigidbody </param>
        /// <param name="removeStacked"> if set to true, the stacked movements that are currently active will also be removed, preventing them from being carried over if they have not yet been completed. </param>
        public static void RemoveCurrentGradualMovement(Rigidbody rb, bool removeStacked = false)
        {
            if (!currentRigidGradualMovements.ContainsKey(rb))
                return;
            currentRigidGradualMovements[rb].currentStaticMovement.OnMovementFinish(true);
            currentRigidGradualMovements[rb].RemoveStaticMovement();
            if (removeStacked)
            {
                currentRigidGradualMovements[rb].movements.Clear();
                foreach (QIGradualMovement qgm in currentRigidGradualMovements[rb].movements)
                {
                    if (qgm.IsStacked())
                        qgm.OnMovementFinish(true);
                }
            }
            OnCurrentRigidMovementEnd(rb);
        }

        #endregion

        #endregion

        #endregion


    }


    /// <summary>
    /// The Gradual Movement interface that all gradual movements must implement.
    /// </summary>
    public interface QIGradualMovement
    {
        /// <summary>
        /// Returns the displacement of the object as affected by this gradual movement, while also keeping track of the time passed. If the gradual movement is done, returns negative infinity to signal so.
        /// </summary>
        /// <param name="time"> the time that has passed since the last frame. </param>
        /// <returns> the displacement to take place within the frame. Negative infinity if the movement is done. </returns>
        Vector3 GetApplyTransformation(float time);
        /// <summary>
        /// Returns whether the gradual movement should be stacked along with a series of movements; or only start when a series of movements has finished and define its own series of movements upon which other movements may stack.
        /// </summary>
        /// <returns>true for stacked, false for not stacked.</returns>
        bool IsStacked();
        /// <summary>
        /// The function called by the manager when the movement has finished and been removed from the current movements of the assigned rigidbody/gameobject. The function will not be called when changing scenes, though the movements and their objects will nonetheless be removed.
        /// </summary>
        /// <param name="premature"> this will be true if the movement was marked as finished before the movement was completed, such as when the object is removed from movement. </param>
        void OnMovementFinish(bool premature);

        /// <summary>
        /// Attempts to merge the input movement into this current movement. These are often implemented such that similar types of movement, and especially linear ones over similar times, can be handled as a single movement instead to increase performance.
        /// </summary>
        /// <param name="mov"> The movement to be merged. </param>
        /// <returns> whether the input movement has been integrated into this movement. If true, this class is now responsible for adding the displacement of the other movement into itself. If false, the movement that has been attempted to be merged will be queued normally.</returns>
        bool AttemptMerge(QIGradualMovement mov);

        /// <summary>
        /// Based on the time calls the manager has made, returns the amount of time left before the movement is done and the GetApplyTransformation function returns negative infinity.
        /// </summary>
        /// <returns>the amount of time left before the movement is done and the GetApplyTransformation function returns negative infinity.</returns>
        float GetTimeLeft();
    }

}


///// <summary>
///// The gradual movement base class that all gradual movements implement. The base class is designed to work along with the QGradualMovementManager, with the movement itself being defined by a few virtual functions.
///// </summary>
//public class QGradualMovement
//{

//    public virtual bool IsStacked()
//    {
//        return false;
//    }


//    public virtual void OnMovementFinish(bool premature)
//    {

//    }

//    public virtual bool AttemptMerge(QGradualMovement mov)
//    {
//        return false;
//    }

//}