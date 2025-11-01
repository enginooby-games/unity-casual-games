using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Devdy.FallingCatch
{
    /// <summary>
    /// Manages spawning of falling objects during gameplay.
    /// </summary>
    public class SpawnManager : Singleton<SpawnManager>
    {
        #region Fields
        [SerializeField] private GameObject fallingObjectPrefab; // Prefab with FallingObject component
        [SerializeField] private Transform spawnParent; // Parent for organization

        private List<GameObject> activeObjects = new List<GameObject>(); // Track spawned objects
        private Coroutine spawnCoroutine;

        private const float SPAWN_Y = 6f; // Y position for spawning
        #endregion

        #region Spawning Control
        public void StartSpawning()
        {
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
            }
            spawnCoroutine = StartCoroutine(SpawnRoutine());
        }

        public void StopSpawning()
        {
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null;
            }
        }

        public void ClearAllObjects()
        {
            foreach (var obj in activeObjects)
            {
                if (obj != null) Destroy(obj);
            }
            activeObjects.Clear();
        }
        #endregion

        #region Spawn Logic
        private IEnumerator SpawnRoutine()
        {
            while (GameManager.Instance.IsGameActive)
            {
                SpawnObject();
                yield return new WaitForSeconds(SROptions.Current.FallingCatch_SpawnInterval);
            }
        }

        private void SpawnObject()
        {
            float randomX = Random.Range(GameManager.Instance.Config.minX, GameManager.Instance.Config.maxX);
            Vector3 spawnPosition = new Vector3(randomX, SPAWN_Y, 0f);

            GameObject obj = Instantiate(fallingObjectPrefab, spawnPosition, Quaternion.identity, spawnParent);
            
            bool isGood = Random.value <= SROptions.Current.FallingCatch_GoodObjectRatio;
            obj.GetComponent<FallingObject>().Initialize(isGood);

            activeObjects.Add(obj);

            // Auto-cleanup after lifetime
            StartCoroutine(DestroyAfterLifetime(obj));
        }

        private IEnumerator DestroyAfterLifetime(GameObject obj)
        {
            yield return new WaitForSeconds(GameManager.Instance.Config.objectLifetime);
            
            if (obj != null)
            {
                activeObjects.Remove(obj);
                Destroy(obj);
            }
        }
        #endregion

        #region Cleanup
        public void RemoveObject(GameObject obj)
        {
            activeObjects.Remove(obj);
        }
        #endregion
    }
}