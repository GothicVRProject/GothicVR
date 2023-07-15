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
        }

        private void Update()
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
            int radius = 3;
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

        public void AddAudioSource(GameObject gameObj, AudioSource audioSource)
        {
            if (audioSources.ContainsKey(gameObj))
            {
                return;
            }
            audioSources.Add(gameObj, audioSource);

            // Add the audio source to the appropriate grid cell
            Vector3Int gridCell = GetGridCellFromPosition(gameObj.transform.position);
            if (!gridCells.TryGetValue(gridCell, out List<GameObject> audioSourcesInCell))
            {
                audioSourcesInCell = new List<GameObject>();
                gridCells.Add(gridCell, audioSourcesInCell);
            }
            audioSourcesInCell.Add(gameObj);

            // Deactivate the gameobject to prevent audio from being played and CPU usage
        }

        public void SetAudible(GameObject gameObj, bool isAudible)
        {
            gameObj.SetActive(isAudible);
        }

        public void SetAudioListener(AudioListener audioListener)
        {
            this.audioListener = audioListener;
        }

        private Vector3Int GetGridCellFromPosition(Vector3 position)
        {
            int x = Mathf.FloorToInt(position.x / cellSize);
            int y = Mathf.FloorToInt(position.y / cellSize);
            int z = Mathf.FloorToInt(position.z / cellSize);
            return new Vector3Int(x, y, z);
        }

        private bool ShouldBeAudible(GameObject gameObj)
        {
            if (audioListener == null)
                return false;

            float maxDistance = audioSources[gameObj].maxDistance;
            float distanceFromPlayer = Vector3.Distance(gameObj.transform.position, audioListener.transform.position);
            return distanceFromPlayer <= maxDistance;
        }
    }
}
