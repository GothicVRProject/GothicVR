using UnityEngine;
using System.Collections.Generic;
using GVR.Util;

namespace GVR.Manager
{
    public class AudioSourceManager : SingletonBehaviour<AudioSourceManager>
    {
        private Dictionary<GameObject, AudioSource> audioSources = new Dictionary<GameObject, AudioSource>();
        private AudioListener audioListener = null;

        private Vector3Int playerCell;

        private bool IsAudioListenerObtained
        {
            get
            {
                return audioListener != null;
            }
        }

        // Spatial partitioning variables
        private Dictionary<Vector3Int, List<GameObject>> gridCells = new Dictionary<Vector3Int, List<GameObject>>();
        private float cellSize = 50f; // Cell size in meters

        private void Create()
        {
            // Initialize the grid cells
            gridCells.Clear();
            // invokes the function once every second
        }

        private void Start()
        {
            // run the function every half a second
            InvokeRepeating("UpdateAudioSourcesAroundPlayer", 1f, 0.5f);
        }

        private void UpdateAudioSourcesAroundPlayer()
        {
            if (!IsAudioListenerObtained)
            {
                return;
            }

            // Calculate the current grid cell position of the audio listener
            Vector3Int listenerCell = GetGridCellFromPosition(audioListener.transform.position);

            if (listenerCell == playerCell)
            {
                return;
            }

            playerCell = listenerCell;

            // Iterate over the cells around the player in a circular area
            UpdateAudioSourcesWithinRadius(listenerCell: listenerCell, radius: 3);
        }

        public void AddAudioSource(GameObject gameObj, AudioSource audioSource)
        {
            if (audioSources.ContainsKey(gameObj))
                return;

            audioSources.Add(gameObj, audioSource);

            // Add the audio source to the appropriate grid cell
            Vector3Int gridCell = GetGridCellFromPosition(gameObj.transform.position);
            if (!gridCells.TryGetValue(gridCell, out List<GameObject> audioSourcesInCell))
            {
                audioSourcesInCell = new List<GameObject>();
                gridCells.Add(gridCell, audioSourcesInCell);
            }
            audioSourcesInCell.Add(gameObj);
            
            // Deactivate the GameObject to prevent audio from being played and CPU usage.
            // This manager will handle reactivation next frame.
            gameObj.SetActive(false);
        }

        public void SetAudible(GameObject gameObj, bool isAudible)
        {
            gameObj.SetActive(isAudible);
        }

        public void SetAudioListener(AudioListener audioListener)
        {
            this.audioListener = audioListener;
        }

        /// <summary>
        /// Converts a position in world space to the corresponding grid cell position.
        /// This function divides the position coordinates by the cell size and rounds down to the nearest integer to determine the grid cell coordinates.
        /// The resulting grid cell position is returned as a Vector3Int.
        /// </summary>
        /// <param name="position">The position in world space.</param>
        /// <returns>The corresponding grid cell position.</returns>
        private Vector3Int GetGridCellFromPosition(Vector3 position)
        {
            int x = Mathf.FloorToInt(position.x / cellSize);
            int y = Mathf.FloorToInt(position.y / cellSize);
            int z = Mathf.FloorToInt(position.z / cellSize);
            return new Vector3Int(x, y, z);
        }

        /// <summary>
        /// Updates the audio sources within a specified radius around the listener cell.
        /// It iterates over each cell within the radius and checks if it falls within the circular area.
        /// If so, it retrieves the audio sources associated with that cell and determines if they should be audible.
        /// https://demonstrations.wolfram.com/ApproximatingSpheresWithBoxes/ - graphical visualisation of how the sphere would look like
        /// </summary>
        /// <param name="listenerCell">The position of the listener cell.</param>
        /// <param name="radius">The radius of the circular area.</param>
        private void UpdateAudioSourcesWithinRadius(Vector3Int listenerCell, int radius)
        {
            for (int x = listenerCell.x - radius; x <= listenerCell.x + radius; x++)
            {
                for (int y = listenerCell.y - radius; y <= listenerCell.y + radius; y++)
                {
                    for (int z = listenerCell.z - radius; z <= listenerCell.z + radius; z++)
                    {
                        Vector3Int currentCell = new Vector3Int(x, y, z);

                        // Skip if the current cell is outside the desired circular area
                        if (Vector3.Distance(listenerCell, currentCell) > radius)
                        {
                            continue;
                        }

                        // Iterate over the audio sources in the current cell
                        if (gridCells.TryGetValue(currentCell, out List<GameObject> audioSourcesInCell))
                        {
                            foreach (GameObject audioSourceObj in audioSourcesInCell)
                            {
                                bool isAudible = ShouldBeAudible(audioSourceObj);
                                SetAudible(audioSourceObj, isAudible);
                            }
                        }
                    }
                }
            }
        }

        private bool ShouldBeAudible(GameObject gameObj)
        {
            float maxDistance = audioSources[gameObj].maxDistance;
            float distanceFromPlayer = Vector3.Distance(gameObj.transform.position, audioListener.transform.position);
            return distanceFromPlayer <= maxDistance;
        }

        public void ResetDictionaries()
        {
            audioSources = new Dictionary<GameObject, AudioSource>();
            gridCells = new Dictionary<Vector3Int, List<GameObject>>();
        }
    }
}
