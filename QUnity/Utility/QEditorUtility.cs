using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QUnity.Utility
{
    public static class QEditorUtility
    {

        #region Tag Management

        private static SerializedObject TagManager;
        private static SerializedProperty tagsProp;

        /// <summary>
        /// updates the tag manager object held in memory by this class.
        /// </summary>
        /// <param name="tagManager">new tag manager object.</param>
        public static void UpdateTagManager(SerializedObject tagManager)
        {
            TagManager = tagManager;
            tagsProp = tagManager.FindProperty("tags");
        }

        /// <summary>
        /// Adds a tag to the current Unity Editor instance if it doesn't already exist. Also updates the current tag manager through UpdateManager function
        /// </summary>
        /// <param name="tagManager"> The tag manager. can usually be got through AssetDatabase.LoadAllAssetsAtPath() function. This object will be saved for future use. </param>
        /// <param name="newTag">the tag to be added.</param>
        public static void AddTag(SerializedObject tagManager, string newTag)
        {
            UpdateTagManager(tagManager);
            AddTag(newTag);
        }

        /// <summary>
        /// Adds a tag to the current Unity Editor Instance if it doesn't already exist.
        /// </summary>
        /// <param name="newTag">the tag to be added.</param>
        public static void AddTag(string newTag)
        {
            TagManager.Update(); //TODO Is this necessary? Does it use too many resources?
            for(int i = 0; i < tagsProp.arraySize; i++)
            {
                SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);

                if (t.stringValue.Equals(newTag))
                    return;
            }

            tagsProp.InsertArrayElementAtIndex(0);
            SerializedProperty newTagProp = tagsProp.GetArrayElementAtIndex(0);
            newTagProp.stringValue = newTag;
            TagManager.ApplyModifiedProperties();
        }

        /// <summary>
        /// Adds multiple tags to the current Unity Editor instance. Only does so for those that are not already added. Also updates the current tag manager through UpdateManager function
        /// </summary>
        /// <param name="newTag">the tags to be added.</param>
        public static void AddTag(string[] newTag)
        {
            for (int i = 0; i < newTag.Length; i++)
            {
                AddTag(newTag[i]);
            }
        }

        /// <summary>
        /// Adds multiple tags to the current Unity Editor instance. Only does so for those that are not already added. Also updates the current tag manager through UpdateManager function
        /// </summary>
        /// <param name="tagManager"> The tag manager. can usually be got through AssetDatabase.LoadAllAssetsAtPath() function. This object will be saved for future use. </param>
        /// <param name="newTag">the tags to be added.</param>
        public static void AddTag(SerializedObject tagManager, string[] newTag)
        {
            UpdateTagManager(tagManager);

            for(int i = 0; i < newTag.Length; i++)
            {
                AddTag(newTag[i]);
            }
        }

        #endregion
    }
}
