using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QUnity.Utility
{

    [RequireComponent(typeof(AudioSource))]
    public class QMusicManager : MonoBehaviour
    {

        private static QMusicManager Singleton;
        private AudioSource audioSource;

        private bool isPlaying = false;

        [SerializeField]
        private float maxWaitTime = 0f;
        [SerializeField]
        private float minWaitTime = 0f;

        private Dictionary<string, List<AudioClip>> ClipGroups = new Dictionary<string, List<AudioClip>>();
        private Dictionary<string, MusicGroupProperties> ClipGroupProperties = new Dictionary<string, MusicGroupProperties>();
        private Queue<AudioClip> ClipQueue;

        private string activeGroup = "None";
        private bool active = false;
        private bool paused = false;

        #region Unity Functions - Instantiation and Music Playback Detection

        private void Awake()
        {
            if (Singleton != null)
            {
                Destroy(gameObject);
                return;
            }
            Singleton = this;
            DontDestroyOnLoad(gameObject);
            isPlaying = false;
            active = false;
            audioSource = GetComponent<AudioSource>();
            activeGroup = "None";
            default_volume = audioSource.volume;
            defaultMaxTime = maxWaitTime;
            defaultMinTime = minWaitTime;
        }

        private void Update()
        {
            if (paused || !active)
            {
                isPlaying = audioSource.isPlaying;
                return;
            }
            if (isPlaying && !audioSource.isPlaying)
            {
                onEndPlaying();
            }
            isPlaying = audioSource.isPlaying;
        }

        #endregion

        #region Callbacks and Music Switch Management

        private float default_volume;
        private float defaultMinTime;
        private float defaultMaxTime;
        private List<AudioClip> AlreadyPlayedInGroup = new List<AudioClip>();
        private void onEndPlaying()
        {
            audioSource.volume = default_volume;
            minWaitTime = defaultMinTime;
            maxWaitTime = defaultMaxTime;
            if (ClipQueue.Count > 0)
            {
                //play from queue.
                active = false;
                StartCoroutine(PlayAfterWait((minWaitTime < maxWaitTime) ? UnityEngine.Random.Range(minWaitTime, maxWaitTime) : 0, ClipQueue.Dequeue()));
            }
            else
            {
                //play from the active group.
                if (ClipGroups.ContainsKey(activeGroup))
                {
                    if (ClipGroupProperties[activeGroup].special_volume != -1)
                        audioSource.volume = ClipGroupProperties[activeGroup].special_volume;
                    if (ClipGroupProperties[activeGroup].minWaitTime != -1)
                        minWaitTime = ClipGroupProperties[activeGroup].minWaitTime;
                    if (ClipGroupProperties[activeGroup].maxWaitTime != -1)
                        minWaitTime = ClipGroupProperties[activeGroup].maxWaitTime;
                    if (ClipGroupProperties[activeGroup].loop && AlreadyPlayedInGroup.Count == ClipGroups[activeGroup].Count)
                        AlreadyPlayedInGroup.Clear();
                    for (int i = 0; i < ClipGroups[activeGroup].Count; i++)
                    {
                        if (AlreadyPlayedInGroup.Contains(ClipGroups[activeGroup][i]))
                            continue;
                        if (ClipGroupProperties[activeGroup].shuffle && UnityEngine.Random.Range(0, 1) > (float)(i + 1) / (float)ClipGroups[activeGroup].Count)
                            continue;
                        AlreadyPlayedInGroup.Add(ClipGroups[activeGroup][i]);
                        active = false;
                        StartCoroutine(PlayAfterWait((minWaitTime < maxWaitTime) ? UnityEngine.Random.Range(minWaitTime, maxWaitTime) : 0, ClipGroups[activeGroup][i]));
                        return;
                    }
                }
            }
        }

        #endregion

        #region Public Functions

        public static QMusicManager GetSingleton()
        {
            return Singleton;
        }

        /// <summary>
        /// Sets the volume value.
        /// </summary>
        /// <param name="new_volume"> The volume value, with 0 meaning muted and 1 indicating full volume </param>
        public void SetVolume(float new_volume)
        {
            audioSource.volume = Mathf.Clamp(new_volume, 0, 1);
            default_volume = new_volume;
            ExecuteOnVolumeChange();
        }

        /// <summary>
        /// Checks whether music is currently playing, or whether there is no music to be played or currently the manager is waiting inbetween clips.
        /// </summary>
        public bool isMusicPlaying()
        {
            return isPlaying;
        }

        /// <summary>
        /// Adds the clip to the given group
        /// </summary>
        /// <param name="clip"> The clip to be added </param>
        /// <returns> True: was added to the group or already existed in the group in the first place, false: the given group does not exist </returns>
        public bool AddToGroup(string groupName, AudioClip clip)
        {
            if (!ClipGroups.ContainsKey(groupName))
            {
                return false;
            }
            if (!ClipGroups[groupName].Contains(clip))
                ClipGroups[groupName].Add(clip);
            return true;
        }

        /// <summary>
        /// Enqueues the provided music clip.
        /// </summary>
        public void Enqueue(AudioClip audioClip)
        {
            ClipQueue.Enqueue(audioClip);
        }

        /// <summary>
        /// Removes the clip from all the groups it is to be found in as well as the entirety of the queue.
        /// </summary>
        public void RemoveClip(AudioClip audioClip)
        {
            RemoveFromAllGroups(audioClip);
            RemoveFromQueue(audioClip);
        }

        /// <summary>
        /// Removes the given clip from all groups.
        /// </summary>
        /// <returns> Number of groups the item was removed from. </returns>
        public int RemoveFromAllGroups(AudioClip audioClip)
        {
            int returned = 0;
            foreach (string s in ClipGroups.Keys)
            {
                returned += RemoveFromGroup(s, audioClip) ? 1 : 0;
            }
            return returned;
        }


        /// <summary>
        /// Removes the clip from the specified group
        /// </summary>
        /// <returns> True if removed, false if already wasn't in group or the group doesn't exist. </returns>
        public bool RemoveFromGroup(string groupName, AudioClip audioClip)
        {
            if (ClipGroups.ContainsKey(groupName))
            {
                return ClipGroups[groupName].Remove(audioClip);
            }
            return false;
        }

        /// <summary>
        /// Removes the clip from the music queue.
        /// </summary>
        /// <param name="removeOnce"> If true, all instances of the clip will be removed from the queue. If false, only the instance about to be played soonest will be removed. </param>
        /// <returns> Number of times the item was removed. </returns>
        public int RemoveFromQueue(AudioClip audioClip, bool removeOnce = false)
        {
            int removed = 0;
            if (ClipQueue.Contains(audioClip))
            {
                AudioClip[] array = ClipQueue.ToArray();
                List<AudioClip> new_array = new List<AudioClip>();
                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i] == audioClip && !(removeOnce && removed == 1))
                        continue;
                    new_array.Add(array[i]);
                }
                ClipQueue = new Queue<AudioClip>(new_array);
            }
            return removed;
        }


        public void ClearQueue()
        {
            ClipQueue.Clear();
        }

        /// <summary>
        /// Removes the group.
        /// </summary>
        /// <returns> True if the group was successfully removed, false if the group did not exist in the first place </returns>
        public bool RemoveGroup(string groupName)
        {
            if (activeGroup == groupName)
                active = false;
            ClipGroupProperties.Remove(groupName);
            return ClipGroups.Remove(groupName);
        }

        /// <summary>
        /// Returns all the names of all groups that have been created.
        /// </summary>
        public List<string> GetGroupNames()
        {
            return new List<string>(ClipGroups.Keys);
        }

        /// <summary>
        /// Returns a copy of all the clips in the specified group. If the group does not exist returns null.
        /// </summary>
        public List<AudioClip> GetGroupClips(string groupName)
        {
            if (!ClipGroups.ContainsKey(groupName))
                return null;
            return new List<AudioClip>(ClipGroups[groupName]);
        }

        /// <summary>
        /// Creates a group with the given name
        /// </summary>
        /// <param name="properties"> The properties that define the behaviour of this specific group </param>
        /// <returns> True if group was created, false if group already exists. </returns>
        public bool CreateGroup(string name, MusicGroupProperties properties = new MusicGroupProperties())
        {
            if (ClipGroups.ContainsKey(name))
                return false;
            ClipGroups.Add(name, new List<AudioClip>());
            ClipGroupProperties.Add(name, properties);
            return true;
        }

        /// <summary>
        /// Sets the active group to this one. If was already active, resets the playing to the start of the group.
        /// </summary>
        /// <param skipCurrent = "skipCurrent"> Indicates whether the current music being played by another group should be halted and the specified group be immediately played. This, however, does not
        /// halt the queued music being played if any clips exist in it. </param>
        /// <returns> True if the group exists and was switched to, false if the group does not exist. </returns>
        public bool SetActiveGroup(string name, bool skipCurrent = false)
        {
            AlreadyPlayedInGroup.Clear();
            if (ClipGroups.ContainsKey(name))
            {
                activeGroup = name;
                if (skipCurrent)
                    audioSource.Stop();
                return true;
            }
            return false;
        }

        public void EnablePause(bool pause)
        {
            if (pause)
                audioSource.Pause();
            else
                audioSource.UnPause();

        }


        /// <summary>
        /// Restarts the current music that was being played.
        /// </summary>
        public void RestartCurrentMusic()
        {
            audioSource.Stop();
            audioSource.Play();
        }

        /// <summary>
        /// Sets the minimum time in seconds to be waited before the next song is to be played.
        /// </summary>
        public void SetMinWaitTime(float newTime)
        {
            minWaitTime = newTime;
            defaultMinTime = newTime;
        }

        /// <summary>
        /// Sets the maximum time in seconds to be waited before the next song is to be played.
        /// </summary>
        public void SetMaxWaitTime(float newTime)
        {
            maxWaitTime = newTime;
            defaultMaxTime = newTime;
        }

        #endregion

        #region Private Functions

        private IEnumerator PlayAfterWait(float wait_time, AudioClip audio)
        {
            if (wait_time > 0)
            {
                yield return new WaitForEndOfFrame();
                wait_time -= Time.deltaTime;
            }
            audioSource.clip = audio;
            audioSource.Play();
            ExecuteOnClipPlay();
            active = true;
        }

        #endregion

        #region Event Systems


        #region Volume Change

        private event Action<float> onVolumeChange;

        private void ExecuteOnVolumeChange()
        {
            onVolumeChange?.Invoke(audioSource.volume);
        }

        public void SubscribeToVolumeChange(Action<float> action)
        {
            onVolumeChange += action;
        }

        public void UnSubscribeToVolumeChange(Action<float> action)
        {
            onVolumeChange -= action;
        }

        #endregion

        #region Clip Playing

        private event Action<AudioClip> onClipPlay;

        private void ExecuteOnClipPlay()
        {
            onClipPlay?.Invoke(audioSource.clip);
        }

        public void SubscribeToClipPlay(Action<AudioClip> action)
        {
            onClipPlay += action;
        }

        public void UnSubscribeToClipPlay(Action<AudioClip> action)
        {
            onClipPlay -= action;
        }

        #endregion

        #endregion

    }


    public struct MusicGroupProperties
    {
        public bool shuffle;
        public bool loop;
        public float special_volume;
        public float minWaitTime;
        public float maxWaitTime;

        /// <summary>
        /// The properties that define the behaviour of a music group when it is being played by the MusicManager class.
        /// </summary>
        /// <param name="Shuffle"> Whether the clips should be shuffled. Each clip will still only be played once. </param>
        /// <param name="Loop"> Whether, when all the clips are done playing, the group should restart. </param>
        /// <param name="SpecialVolume"> The volume to which the Audio Source will be set when playing this group. Keep in mind
        /// that the Audio Source will switch back to the old volume after the music group is switched from. Also, if you would like to reset the volume
        /// you may still use the SetVolume function, which will override the SpecialVolume if used after the group is already activated.
        /// Leave this value as -1 if you would like the Audio Source to stay at the default volume/ the value set by the MusicManager. </param>
        /// <param name="MinWaitTime"> The minimum amount of time in seconds to be waited before the next song is played. Leave it as -1 if you want the
        /// default MusicManager value to be used. </param>
        /// <param name="MaxWaitTime"> The maximum amount of time in seconds to be waited before the next song is played. Leave it as -1 if you want the
        /// default MusicManager value to be used.</param>
        public MusicGroupProperties(bool Shuffle = true, bool Loop = true, float SpecialVolume = -1, float MinWaitTime = -1, float MaxWaitTime = -1)
        {
            shuffle = Shuffle;
            loop = Loop;
            special_volume = SpecialVolume;
            minWaitTime = MinWaitTime;
            maxWaitTime = MaxWaitTime;
        }
    }

}