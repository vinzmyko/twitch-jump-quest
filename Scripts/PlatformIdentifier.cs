using Godot;
using System.Collections.Generic;

public class PlatformIdentifier
{
    private TileMap tileMap;
    private int midgroundLayerId;
    private Dictionary<Vector2I, int> platformIds = new Dictionary<Vector2I, int>();

    public PlatformIdentifier(TileMap tileMap, int midgroundLayerId)
    {
        this.tileMap = tileMap;
        this.midgroundLayerId = midgroundLayerId;
    }

    public void IdentifyPlatforms()
    {
        int currentPlatformId = 1;
        HashSet<Vector2I> visited = new HashSet<Vector2I>();

        Rect2I usedRect = tileMap.GetUsedRect();
        for (int x = usedRect.Position.X; x < usedRect.End.X; x++)
        {
            for (int y = usedRect.Position.Y; y < usedRect.End.Y; y++)
            {
                Vector2I cellPos = new Vector2I(x, y);
                if (IsVisibleMidgroundTile(cellPos) && !visited.Contains(cellPos))
                {
                    FloodFillPlatform(cellPos, currentPlatformId, visited);
                    currentPlatformId++;
                }
            }
        }
    }

    private void FloodFillPlatform(Vector2I startPos, int platformId, HashSet<Vector2I> visited)
    {
        Queue<Vector2I> queue = new Queue<Vector2I>();
        queue.Enqueue(startPos);

        while (queue.Count > 0)
        {
            Vector2I currentPos = queue.Dequeue();
            if (visited.Contains(currentPos)) continue;

            visited.Add(currentPos);
            platformIds[currentPos] = platformId;

            // Check only directly adjacent tiles (no diagonals)
            Vector2I[] neighbors = new Vector2I[]
            {
                new Vector2I(currentPos.X + 1, currentPos.Y),
                new Vector2I(currentPos.X - 1, currentPos.Y),
                new Vector2I(currentPos.X, currentPos.Y + 1),
                new Vector2I(currentPos.X, currentPos.Y - 1)
            };

            foreach (var neighbor in neighbors)
            {
                if (IsVisibleMidgroundTile(neighbor) && !visited.Contains(neighbor))
                {
                    queue.Enqueue(neighbor);
                }
            }
        }
    }

    private bool IsVisibleMidgroundTile(Vector2I cellPos)
    {
        // Check if the Midground layer has a tile at this position
        if (tileMap.GetCellSourceId(midgroundLayerId, cellPos) == -1)
            return false;

        // Check if any layer above Midground has a tile at this position
        for (int layerId = tileMap.GetLayersCount() - 1; layerId > midgroundLayerId; layerId--)
        {
            if (tileMap.GetCellSourceId(layerId, cellPos) != -1)
                return false; // This Midground tile is covered by a higher layer
        }

        return true;
    }

    public int GetPlatformId(Vector2I cellPos)
    {
        return platformIds.TryGetValue(cellPos, out int id) ? id : -1;
    }

    // Debug method to print platform IDs
    public void PrintPlatformIds()
    {
        foreach (var kvp in platformIds)
        {
            GD.Print($"Cell: {kvp.Key}, Platform ID: {kvp.Value}");
        }
    }
}