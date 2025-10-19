using UnityEngine;
using System.Collections.Generic;

namespace Devdy.AStarVisualizer
{
    /// <summary>
    /// Manages the grid creation, cell placement, and user interaction with the grid.
    /// Provides shared grid data access for all pathfinding systems.
    /// </summary>
    public class GridManager : Singleton<GridManager>
    {
        [SerializeField] private GridCell cellPrefab; // Prefab for grid cells
        [SerializeField] private Camera mainCamera; // Reference to main camera for raycasting
        
        public int GridWidth { get; private set; } = 20;
        public int GridHeight { get; private set; } = 15;
        public float CellSize { get; private set; } = 1f;
        public PolygonType CurrentPolygonType { get; private set; } = PolygonType.Square;
        
        private GridCell[,] grid;
        private GridCell startCell;
        private GridCell endCell;

        protected override void Awake()
        {
            base.Awake();
            
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
        }

        private void Start()
        {
            CreateGrid();
        }

        private void Update()
        {
            HandleInput();
        }

        public void CreateGrid()
        {
            ClearGrid();
            grid = new GridCell[GridWidth, GridHeight];
            
            Vector2 offset = CalculateGridOffset();
            
            for (int x = 0; x < GridWidth; x++)
            {
                for (int y = 0; y < GridHeight; y++)
                {
                    Vector2 worldPos = CalculateWorldPosition(x, y, offset);
                    CreateCell(x, y, worldPos);
                }
            }
        }

        private Vector2 CalculateGridOffset()
        {
            float xOffset = CurrentPolygonType switch
            {
                PolygonType.Hexagon => -(GridWidth * CellSize * 0.75f) / 2f,
                _ => -(GridWidth * CellSize) / 2f
            };
            
            float yOffset = CurrentPolygonType switch
            {
                PolygonType.Hexagon => -(GridHeight * CellSize * 0.866f) / 2f,
                _ => -(GridHeight * CellSize) / 2f
            };
            
            return new Vector2(xOffset, yOffset);
        }

        private Vector2 CalculateWorldPosition(int x, int y, Vector2 offset)
        {
            return CurrentPolygonType switch
            {
                PolygonType.Hexagon => CalculateHexPosition(x, y, offset),
                _ => new Vector2(x * CellSize + offset.x, y * CellSize + offset.y)
            };
        }

        private Vector2 CalculateHexPosition(int x, int y, Vector2 offset)
        {
            float xPos = x * CellSize * 0.75f + offset.x;
            float yPos = y * CellSize * 0.866f + offset.y;
            
            if (x % 2 == 1)
            {
                yPos += CellSize * 0.433f;
            }
            
            return new Vector2(xPos, yPos);
        }

        private void CreateCell(int x, int y, Vector2 worldPos)
        {
            GameObject cellObj = Instantiate(cellPrefab.gameObject, worldPos, Quaternion.identity, transform);
            cellObj.name = $"Cell_{x}_{y}";
            
            GridCell cell = cellObj.GetComponent<GridCell>();
            cell.Initialize(new Vector2Int(x, y), CurrentPolygonType);
            grid[x, y] = cell;
        }

        private void ClearGrid()
        {
            if (grid == null) return;
            
            foreach (var cell in grid)
            {
                if (cell != null)
                {
                    Destroy(cell.gameObject);
                }
            }
            
            startCell = null;
            endCell = null;
        }

        private void HandleInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                HandleLeftClick();
            }
            else if (Input.GetMouseButtonDown(1))
            {
                HandleRightClick();
            }
        }

        private void HandleLeftClick()
        {
            GridCell cell = GetCellAtMousePosition();
            if (cell == null) return;
            
            if (Input.GetKey(KeyCode.LeftShift))
            {
                SetStartCell(cell);
            }
            else if (Input.GetKey(KeyCode.LeftControl))
            {
                SetEndCell(cell);
            }
            else
            {
                ToggleObstacle(cell);
            }
        }

        private void HandleRightClick()
        {
            GridCell cell = GetCellAtMousePosition();
            if (cell == null) return;
            
            cell.SetWalkable(true);
            cell.ClearSpecialState();
            
            if (cell == startCell) startCell = null;
            if (cell == endCell) endCell = null;
        }

        private GridCell GetCellAtMousePosition()
        {
            Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
            
            if (hit.collider == null) return null;
            return hit.collider.GetComponent<GridCell>();
        }

        private void SetStartCell(GridCell cell)
        {
            if (startCell != null)
            {
                startCell.ClearSpecialState();
            }
            
            startCell = cell;
            startCell.SetAsStart();
        }

        private void SetEndCell(GridCell cell)
        {
            if (endCell != null)
            {
                endCell.ClearSpecialState();
            }
            
            endCell = cell;
            endCell.SetAsEnd();
        }

        private void ToggleObstacle(GridCell cell)
        {
            if (cell.IsStart || cell.IsEnd) return;
            cell.SetWalkable(!cell.IsWalkable);
        }

        public void ChangeGridSize(int width, int height)
        {
            GridWidth = Mathf.Clamp(width, 5, 50);
            GridHeight = Mathf.Clamp(height, 5, 50);
            CreateGrid();
        }

        public void ChangePolygonType(PolygonType type)
        {
            CurrentPolygonType = type;
            CreateGrid();
        }

        public void ResetGrid()
        {
            foreach (var cell in grid)
            {
                if (cell != null)
                {
                    cell.SetWalkable(true);
                    cell.ClearSpecialState();
                    cell.ResetPathfindingData();
                }
            }
            
            startCell = null;
            endCell = null;
        }

        public List<GridCell> GetNeighbors(GridCell cell, bool allowDiagonal)
        {
            List<GridCell> neighbors = new List<GridCell>();
            Vector2Int pos = cell.GridPosition;
            
            if (CurrentPolygonType == PolygonType.Hexagon)
            {
                AddHexNeighbors(neighbors, pos);
            }
            else
            {
                AddSquareNeighbors(neighbors, pos, allowDiagonal);
            }
            
            return neighbors;
        }

        private void AddSquareNeighbors(List<GridCell> neighbors, Vector2Int pos, bool allowDiagonal)
        {
            TryAddNeighbor(neighbors, pos.x, pos.y + 1);
            TryAddNeighbor(neighbors, pos.x, pos.y - 1);
            TryAddNeighbor(neighbors, pos.x - 1, pos.y);
            TryAddNeighbor(neighbors, pos.x + 1, pos.y);
            
            if (!allowDiagonal) return;
            
            TryAddNeighbor(neighbors, pos.x - 1, pos.y + 1);
            TryAddNeighbor(neighbors, pos.x + 1, pos.y + 1);
            TryAddNeighbor(neighbors, pos.x - 1, pos.y - 1);
            TryAddNeighbor(neighbors, pos.x + 1, pos.y - 1);
        }

        private void AddHexNeighbors(List<GridCell> neighbors, Vector2Int pos)
        {
            int x = pos.x;
            int y = pos.y;
            bool isOddColumn = x % 2 == 1;
            
            TryAddNeighbor(neighbors, x + 1, y);
            TryAddNeighbor(neighbors, x - 1, y);
            TryAddNeighbor(neighbors, x, y + 1);
            TryAddNeighbor(neighbors, x, y - 1);
            
            if (isOddColumn)
            {
                TryAddNeighbor(neighbors, x + 1, y + 1);
                TryAddNeighbor(neighbors, x - 1, y + 1);
            }
            else
            {
                TryAddNeighbor(neighbors, x + 1, y - 1);
                TryAddNeighbor(neighbors, x - 1, y - 1);
            }
        }

        private void TryAddNeighbor(List<GridCell> neighbors, int x, int y)
        {
            if (x < 0 || x >= GridWidth || y < 0 || y >= GridHeight) return;
            
            GridCell neighbor = grid[x, y];
            if (neighbor != null && neighbor.IsWalkable)
            {
                neighbors.Add(neighbor);
            }
        }

        public GridCell GetStartCell() => startCell;
        public GridCell GetEndCell() => endCell;
        public GridCell[,] GetGrid() => grid;
    }
}