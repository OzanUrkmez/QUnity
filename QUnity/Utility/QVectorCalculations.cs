﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using QuestryGameGeneral.Calculations;

namespace QUnity
{


    public static class QVectorCalculations
    {

        #region Margin

        /// <summary>
        /// Assesses whether the two vectors are within the provided margin of each other. That is, whether their proximity is within the provided margin.
        /// </summary>
        /// <param name="v1">First vector provided</param>
        /// <param name="v2">Second Vector provided</param>
        /// <param name="margin">The margin</param>
        /// <returns>true if the two vectors within each other's margin. False if otherwise</returns>
        public static bool Vector2WithinMargin(Vector2 v1, Vector2 v2, float margin)
        {
            return Comparisons.WithinMargin(v1.x, v2.x, margin) && Comparisons.WithinMargin(v1.y, v2.y, margin);
        }

        /// <summary>
        /// Assesses whether the two vectors are within the provided margin of each other. That is, whether their proximity is within the provided margin.
        /// </summary>
        /// <param name="v1">First vector provided</param>
        /// <param name="v2">Second Vector provided</param>
        /// <param name="margin">The margin</param>
        /// <returns>true if the two vectors within each other's margin. False if otherwise</returns>
        public static bool Vector3WithinMargin(Vector3 v1, Vector3 v2, float margin)
        {
            return Comparisons.WithinMargin(v1.x, v2.x, margin) && Comparisons.WithinMargin(v1.y, v2.y, margin) &&
            Comparisons.WithinMargin(v1.z, v2.z, margin);
        }

        /// <summary>
        /// Uses the Mathf.Approximately function on the x and y coordiantes of 2D vectors to determine whether they are equal.
        /// </summary>
        /// <param name="v1">first vector</param>
        /// <param name="v2">second vector</param>
        /// <returns>true if the two vectors are approximately equal, false otherwise.</returns>
        public static bool Vector2MathfApproximately(Vector2 v1, Vector2 v2)
        {
            return Mathf.Approximately(v1.x, v2.x) && Mathf.Approximately(v1.y, v2.y);
        }

        /// <summary>
        /// Uses the Mathf.Approximately function on the x, y and z coordiantes of 3D vectors to determine whether they are equal.
        /// </summary>
        /// <param name="v1">first vector</param>
        /// <param name="v2">second vector</param>
        /// <returns>true if the two vectors are approximately equal, false otherwise.</returns>
        public static bool Vector3MathfApproximately(Vector3 v1, Vector3 v2)
        {
            return Mathf.Approximately(v1.x, v2.x) && Mathf.Approximately(v1.y, v2.y) && Mathf.Approximately(v1.z, v2.z);
        }

        #endregion


        #region Pass Point

        /// <summary>
        /// Assesses whether the specified vector, defined by a direction and any point on the vector, passess the specified point
        /// </summary>
        /// <param name="anypoint"> Any point along the vector</param>
        /// <param name="direction"> The direction of the vector.</param>
        /// <param name="point"> The point being assessed.</param>
        /// <returns></returns>
        public static bool Vector2PassesPoint(Vector2 anypoint, Vector2 direction, Vector2 point)
        {
            if (anypoint == point) return true;
            for (int i = 0; i < 2; i++)
            {
                int j = i == 0 ? 1 : 0;
                float factor;
                try
                {
                    factor = (point[i] - anypoint[i]) / direction[i];
                }
                catch
                {
                    factor = 0;
                }

                if ((anypoint[j] + direction[j] * factor) == point[j])
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Assesses whether the specified vector, defined by a direction and any point on the vector, passess the specified point within the margin
        /// </summary>
        /// <param name="anypoint"> Any point along the vector</param>
        /// <param name="direction"> The direction of the vector.</param>
        /// <param name="point"> The point being assessed.</param>
        /// <param name="margin"> The margin</param>
        /// <returns></returns>
        public static bool Vector2PassesPoint(Vector2 anypoint, Vector2 direction, Vector2 point, float margin)
        {
            if (Vector2WithinMargin(anypoint, point, margin)) return true;

            for (int i = 0; i < 2; i++)
            {
                int j = i == 0 ? 1 : 0;
                float factor;
                try
                {
                    factor = (point[i] - anypoint[i]) / direction[i];
                }
                catch
                {
                    factor = 0;
                }

                if (factor < 0) return false;

                if (Comparisons.WithinMargin((anypoint[j] + direction[j] * factor), point[j], margin))
                    return true;
            }
            return false;

        }

        /// <summary>
        /// Assesses whether the ray passess the given point.
        /// </summary>
        /// <param name="raystart"> The origin point of the ray.</param>
        /// <param name="direction"> The direction the ray follows.</param>
        /// <param name="point"> The point being assessed.</param>
        /// <returns>True if the ray starts on or passess the specified point.</returns>
        public static bool Vector2RayPassesPoint(Vector2 raystart, Vector2 direction, Vector2 point)
        {
            if (raystart == point) return true;
            for (int i = 0; i < 2; i++)
            {
                int j = i == 0 ? 1 : 0;
                float factor;
                try
                {
                    factor = (point[i] - raystart[i]) / direction[i];
                }
                catch
                {
                    factor = 0;
                }

                if (factor < 0) return false; //this is because the vector at hand is a ray.

                if ((raystart[j] + direction[j] * factor) == point[j])
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Assesses whether the ray passess the given point within the specified margin.
        /// </summary>
        /// <param name="raystart"> The origin point of the ray.</param>
        /// <param name="direction"> The direction the ray follows.</param>
        /// <param name="point"> The point being assessed.</param>
        /// <param name="margin"> The margin.</param>
        /// <returns>True if the two vectors are within the same margin of each other or the ray itself passess the point within the margin.</returns>
        public static bool Vector2RayPassesPoint(Vector2 raystart, Vector2 direction, Vector2 point, float margin)
        {
            if (Vector2WithinMargin(raystart, point, margin)) return true;

            for (int i = 0; i < 2; i++)
            {
                int j = i == 0 ? 1 : 0;
                float factor;
                try
                {
                    factor = (point[i] - raystart[i]) / direction[i];
                }
                catch
                {
                    factor = 0;
                }

                if (factor < 0) return false;

                if (Comparisons.WithinMargin((raystart[j] + direction[j] * factor), point[j], margin))
                    return true;
            }
            return false;

        }

        #endregion

        #region Conversions

        public static Vector2 Vector3ToBirdView(Vector3 v)
        {
            return new Vector2(v.x, v.z);
        }

        public static float Vector2ToAngleFromXPositiveAxis(Vector2 v)
        {
            float returned = Vector2.SignedAngle(new Vector2(1, 0).normalized, v.normalized);
            return (returned > 0) ? 360f - returned : -returned;
        }

        #endregion

        #region Misc Checks

        /// <returns>true if the given between vector is within the acute angle formed between the first and second vectors.</returns>
        public static bool Vector2IsInBetweenTwoAcute(Vector2 first, Vector2 second, Vector2 between)
        {
            float angle = Vector2.SignedAngle(first, second);
            if(angle >= 0)
            {
                float angle2 = Vector2.SignedAngle(first, between);
                return angle2 >= 0 && angle2 <= angle;
            }else
            {
                angle = -angle;
                float angle2 = Vector2.SignedAngle(second, between);
                return angle2 >= 0 && angle2 <= angle;
            }
        }

        public static bool Vector2IsInBetweenTwoObtuse(Vector2 first, Vector2 second, Vector2 between)
        {
            return !Vector2IsInBetweenTwoAcute(first, second, between);
        }

        #endregion

    }

}

