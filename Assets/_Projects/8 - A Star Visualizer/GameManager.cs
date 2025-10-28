using UnityEngine;

namespace Devdy.AStarVisualizer
{
    /// <summary>
    /// Main game manager that orchestrates all systems and handles game state.
    /// Provides centralized access to shared game configuration.
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        private void Start()
        {
            InitializeSRDebugger();
        }

        private void InitializeSRDebugger()
        {
            SRDebug.Instance.PinAllOptions("AStarVisualizer");
        }

        /// <summary>
        /// Restarts the game by resetting all runtime states without reloading the scene.
        /// Applies all settings from SROptions including grid size, polygon type, animation speed, and diagonal movement.
        /// </summary>
        public void RestartGame()
        {
            if (AStarPathfinder.Instance.IsPathfinding())
            {
                Debug.LogWarning("Cannot restart while pathfinding is in progress!");
                return;
            }

            int width = SROptions.Current.AStarVisualizer_GridWidth;
            int height = SROptions.Current.AStarVisualizer_GridHeight;
            PolygonType polyType = (PolygonType)SROptions.Current.AStarVisualizer_PolygonType;
            float animSpeed = SROptions.Current.AStarVisualizer_AnimationSpeed;
            bool allowDiagonal = SROptions.Current.AStarVisualizer_AllowDiagonal;

            GridManager.Instance.ChangeGridSize(width, height);
            GridManager.Instance.ChangePolygonType(polyType);
            
            float delay = (1f - animSpeed) * 0.5f;
            AStarPathfinder.Instance.SetStepDelay(delay);
            AStarPathfinder.Instance.SetDiagonalMovement(allowDiagonal);
            
            Debug.Log($"Game restarted: Grid {width}x{height}, Type: {polyType}, Speed: {animSpeed}, Diagonal: {allowDiagonal}");
        }
    }
}