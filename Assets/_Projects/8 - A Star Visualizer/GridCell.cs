using UnityEngine;

namespace Devdy.AStarVisualizer
{
    /// <summary>
    /// Represents a single cell in the pathfinding grid.
    /// Handles visual representation and cell state.
    /// </summary>
    public class GridCell : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer; // Visual representation of the cell
        
        public Vector2Int GridPosition { get; private set; }
        public bool IsWalkable { get; private set; } = true;
        public bool IsStart { get; private set; }
        public bool IsEnd { get; private set; }
        
        // A* properties
        public int GCost { get; set; }
        public int HCost { get; set; }
        public int FCost => GCost + HCost;
        public GridCell Parent { get; set; }
        
        private static readonly Color COLOR_WALKABLE = new Color(0.9f, 0.9f, 0.9f);
        private static readonly Color COLOR_OBSTACLE = new Color(0.2f, 0.2f, 0.2f);
        private static readonly Color COLOR_START = new Color(0.2f, 0.8f, 0.2f);
        private static readonly Color COLOR_END = new Color(0.8f, 0.2f, 0.2f);
        private static readonly Color COLOR_PATH = new Color(0.3f, 0.5f, 1f);
        private static readonly Color COLOR_EXPLORED = new Color(1f, 0.8f, 0.4f);
        private static readonly Color COLOR_OPEN = new Color(0.5f, 1f, 0.5f);

        public void Initialize(Vector2Int gridPos, PolygonType polyType)
        {
            GridPosition = gridPos;
            SetupVisual(polyType);
            UpdateVisual();
        }

        private void SetupVisual(PolygonType polyType)
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            Sprite sprite = polyType switch
            {
                PolygonType.Square => CreateSquareSprite(),
                PolygonType.Hexagon => CreateHexagonSprite(),
                _ => CreateSquareSprite()
            };

            spriteRenderer.sprite = sprite;
        }

        private Sprite CreateSquareSprite()
        {
            Texture2D texture = new Texture2D(100, 100);
            Color[] pixels = new Color[100 * 100];
            
            for (int y = 0; y < 100; y++)
            {
                for (int x = 0; x < 100; x++)
                {
                    bool isBorder = x < 2 || x >= 98 || y < 2 || y >= 98;
                    pixels[y * 100 + x] = isBorder ? Color.black : Color.white;
                }
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, 100, 100), new Vector2(0.5f, 0.5f), 100);
        }

        private Sprite CreateHexagonSprite()
        {
            int size = 100;
            Texture2D texture = new Texture2D(size, size);
            Color[] pixels = new Color[size * size];
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float radius = 48f;
            
            // Generate hexagon vertices
            Vector2[] vertices = new Vector2[6];
            for (int i = 0; i < 6; i++)
            {
                float angle = Mathf.PI / 3f * i;
                vertices[i] = new Vector2(
                    center.x + radius * Mathf.Cos(angle),
                    center.y + radius * Mathf.Sin(angle)
                );
            }
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 point = new Vector2(x, y);
                    bool inside = IsPointInPolygon(point, vertices);
                    bool isBorder = inside && IsBorderPixel(point, vertices, 2f);
                    
                    if (inside)
                    {
                        pixels[y * size + x] = isBorder ? Color.black : Color.white;
                    }
                    else
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                }
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100);
        }

        private bool IsPointInPolygon(Vector2 point, Vector2[] vertices)
        {
            bool inside = false;
            int j = vertices.Length - 1;
            
            for (int i = 0; i < vertices.Length; i++)
            {
                if ((vertices[i].y > point.y) != (vertices[j].y > point.y) &&
                    point.x < (vertices[j].x - vertices[i].x) * (point.y - vertices[i].y) / 
                    (vertices[j].y - vertices[i].y) + vertices[i].x)
                {
                    inside = !inside;
                }
                j = i;
            }
            
            return inside;
        }

        private bool IsBorderPixel(Vector2 point, Vector2[] vertices, float borderWidth)
        {
            float minDist = float.MaxValue;
            
            for (int i = 0; i < vertices.Length; i++)
            {
                int j = (i + 1) % vertices.Length;
                float dist = DistanceToLineSegment(point, vertices[i], vertices[j]);
                minDist = Mathf.Min(minDist, dist);
            }
            
            return minDist <= borderWidth;
        }

        private float DistanceToLineSegment(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
        {
            Vector2 line = lineEnd - lineStart;
            float lineLength = line.magnitude;
            
            if (lineLength == 0) return Vector2.Distance(point, lineStart);
            
            float t = Mathf.Clamp01(Vector2.Dot(point - lineStart, line) / (lineLength * lineLength));
            Vector2 projection = lineStart + t * line;
            
            return Vector2.Distance(point, projection);
        }

        public void SetWalkable(bool walkable)
        {
            if (IsStart || IsEnd) return;
            IsWalkable = walkable;
            UpdateVisual();
        }

        public void SetAsStart()
        {
            IsStart = true;
            IsEnd = false;
            IsWalkable = true;
            UpdateVisual();
        }

        public void SetAsEnd()
        {
            IsEnd = true;
            IsStart = false;
            IsWalkable = true;
            UpdateVisual();
        }

        public void ClearSpecialState()
        {
            IsStart = false;
            IsEnd = false;
            UpdateVisual();
        }

        public void ShowAsExplored()
        {
            if (IsStart || IsEnd) return;
            spriteRenderer.color = COLOR_EXPLORED;
        }

        public void ShowAsOpen()
        {
            if (IsStart || IsEnd) return;
            spriteRenderer.color = COLOR_OPEN;
        }

        public void ShowAsPath()
        {
            if (IsStart || IsEnd) return;
            spriteRenderer.color = COLOR_PATH;
        }

        public void UpdateVisual()
        {
            if (IsStart)
            {
                spriteRenderer.color = COLOR_START;
            }
            else if (IsEnd)
            {
                spriteRenderer.color = COLOR_END;
            }
            else if (!IsWalkable)
            {
                spriteRenderer.color = COLOR_OBSTACLE;
            }
            else
            {
                spriteRenderer.color = COLOR_WALKABLE;
            }
        }

        public void ResetPathfindingData()
        {
            GCost = 0;
            HCost = 0;
            Parent = null;
            UpdateVisual();
        }
    }

    public enum PolygonType
    {
        Square = 0,
        Hexagon = 1
    }
}