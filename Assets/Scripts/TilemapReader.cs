

using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapReader: IGrid{

    Tilemap tilemap;
    public TilemapReader(Tilemap tilemap){
        this.tilemap = tilemap;
    }
    public int width => tilemap.size.x;
    public int height => tilemap.size.y;
    public bool isObstacle(int x, int y){
        return tilemap.HasTile(new Vector3Int(x, y, 0));
    }
    public bool isEmpty(int x, int y){
        return !isObstacle(x, y);
    }
}