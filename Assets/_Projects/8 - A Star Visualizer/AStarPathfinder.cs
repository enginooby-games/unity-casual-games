using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Devdy.AStarVisualizer
{
    /// <summary>
    /// Implements A* pathfinding algorithm with step-by-step visualization.
    /// </summary>
    public class AStarPathfinder : Singleton<AStarPathfinder>
    {
        public float StepDelay { get; private set; } = 0.05f;
        public bool AllowDiagonalMovement { get; private set; } = true;
        
        private List<GridCell> openSet = new List<GridCell>();
        private HashSet<GridCell> closedSet = new HashSet<GridCell>();
        private bool isPathfinding = false;
        
        private const int MOVE_STRAIGHT_COST = 10;
        private const int MOVE_DIAGONAL_COST = 14;

        public void StartPathfinding()
        {
            if (isPathfinding) return;
            
            GridCell start = GridManager.Instance.GetStartCell();
            GridCell end = GridManager.Instance.GetEndCell();
            
            if (start == null || end == null)
            {
                Debug.LogWarning("Start or End cell not set!");
                return;
            }
            
            StartCoroutine(FindPathCoroutine(start, end));
        }

        private IEnumerator FindPathCoroutine(GridCell start, GridCell end)
        {
            isPathfinding = true;
            ResetPathfinding();
            
            openSet.Add(start);
            start.GCost = 0;
            start.HCost = CalculateHeuristic(start, end);
            
            while (openSet.Count > 0)
            {
                GridCell current = GetLowestFCostCell();
                
                if (current == end)
                {
                    yield return StartCoroutine(ShowFinalPath(end));
                    isPathfinding = false;
                    yield break;
                }
                
                openSet.Remove(current);
                closedSet.Add(current);
                current.ShowAsExplored();
                
                yield return new WaitForSeconds(StepDelay);
                
                List<GridCell> neighbors = GridManager.Instance.GetNeighbors(current, AllowDiagonalMovement);
                
                foreach (GridCell neighbor in neighbors)
                {
                    if (closedSet.Contains(neighbor)) continue;
                    
                    int tentativeGCost = current.GCost + CalculateMoveCost(current, neighbor);
                    
                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                        neighbor.ShowAsOpen();
                    }
                    else if (tentativeGCost >= neighbor.GCost)
                    {
                        continue;
                    }
                    
                    neighbor.GCost = tentativeGCost;
                    neighbor.HCost = CalculateHeuristic(neighbor, end);
                    neighbor.Parent = current;
                }
                
                yield return new WaitForSeconds(StepDelay);
            }
            
            Debug.Log("No path found!");
            isPathfinding = false;
        }

        private GridCell GetLowestFCostCell()
        {
            GridCell lowest = openSet[0];
            
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].FCost < lowest.FCost || 
                    (openSet[i].FCost == lowest.FCost && openSet[i].HCost < lowest.HCost))
                {
                    lowest = openSet[i];
                }
            }
            
            return lowest;
        }

        private int CalculateHeuristic(GridCell from, GridCell to)
        {
            Vector2Int fromPos = from.GridPosition;
            Vector2Int toPos = to.GridPosition;
            
            int dx = Mathf.Abs(fromPos.x - toPos.x);
            int dy = Mathf.Abs(fromPos.y - toPos.y);
            
            if (AllowDiagonalMovement)
            {
                int min = Mathf.Min(dx, dy);
                int max = Mathf.Max(dx, dy);
                return MOVE_DIAGONAL_COST * min + MOVE_STRAIGHT_COST * (max - min);
            }
            else
            {
                return MOVE_STRAIGHT_COST * (dx + dy);
            }
        }

        private int CalculateMoveCost(GridCell from, GridCell to)
        {
            Vector2Int fromPos = from.GridPosition;
            Vector2Int toPos = to.GridPosition;
            
            bool isDiagonal = fromPos.x != toPos.x && fromPos.y != toPos.y;
            return isDiagonal ? MOVE_DIAGONAL_COST : MOVE_STRAIGHT_COST;
        }

        private IEnumerator ShowFinalPath(GridCell endCell)
        {
            List<GridCell> path = new List<GridCell>();
            GridCell current = endCell;
            
            while (current != null)
            {
                path.Add(current);
                current = current.Parent;
            }
            
            path.Reverse();
            
            foreach (GridCell cell in path)
            {
                cell.ShowAsPath();
                yield return new WaitForSeconds(StepDelay * 0.5f);
            }
            
            Debug.Log($"Path found! Length: {path.Count}");
        }

        private void ResetPathfinding()
        {
            openSet.Clear();
            closedSet.Clear();
            
            GridCell[,] grid = GridManager.Instance.GetGrid();
            if (grid == null) return;
            
            foreach (GridCell cell in grid)
            {
                if (cell != null)
                {
                    cell.ResetPathfindingData();
                }
            }
        }

        public void SetDiagonalMovement(bool enabled)
        {
            AllowDiagonalMovement = enabled;
        }

        public void SetStepDelay(float delay)
        {
            StepDelay = Mathf.Clamp(delay, 0.01f, 1f);
        }

        public bool IsPathfinding() => isPathfinding;
    }
}