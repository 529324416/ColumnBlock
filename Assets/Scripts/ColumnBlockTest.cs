
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class ColumnBlockTest: MonoBehaviour{

    [SerializeField] private bool drawColumnBlock = true;
    [SerializeField] private bool drawCombinedBlock = true;
    [SerializeField] private bool drawActiveRange = true;
    [SerializeField] private Vector2 padding = new Vector2(0.1f, 0.1f);
    [SerializeField] private Color wireColor = Color.white;
    [SerializeField] private int width = 2;

    private TilemapReader gridMap;
    private ColumnBlock[] blocks;
    private CombinedColumnBlock[] combinedBlocks;
    private CombinedColumnBlock[] activeRange;
    private Vector2 gridSize;
    private int lastWidth;

    public void Start(){
        gridMap = new TilemapReader(GetComponent<Tilemap>());
        gridSize = transform.parent.GetComponent<Grid>().cellSize;
        blocks = ColumnBlockHelper.searchColumnBlocks(gridMap);
        activeRange = ColumnBlockHelper.searchActiveRange(blocks);
        if(width >= 2)combinedBlocks = ColumnBlockHelper.combineBlocks(blocks, width);
        lastWidth = -1;
    }
    public void Update(){
        if(lastWidth != width){
            if(width >= 2){
                combinedBlocks = ColumnBlockHelper.combineBlocks(blocks, width);
                lastWidth = width;
            }
        }
    }

    public void OnDrawGizmos(){

        if(drawColumnBlock && blocks != null){
            Gizmos.color = wireColor;
            foreach(ColumnBlock block in blocks){
                _drawColumnBlock(block);
            }
        }
        if(drawCombinedBlock && combinedBlocks != null){
            Gizmos.color = wireColor;
            foreach(CombinedColumnBlock block in combinedBlocks){
                _drawCombinedBlock(block);
            }
        }
        if(drawActiveRange && activeRange != null){
            Gizmos.color = wireColor;
            foreach(CombinedColumnBlock block in activeRange){
                _drawCombinedBlock(block);
            }
        }
    }
    private void _drawCombinedBlock(CombinedColumnBlock block){

        Vector2 center = new Vector2(block.x + block.width / 2f, block.y + block.height / 2f) * gridSize;
        Vector2 size = new Vector2(block.width - padding.x, block.height - padding.y) * gridSize;
        Gizmos.DrawWireCube(center, size);
    }
    private void _drawColumnBlock(ColumnBlock block){

        Vector2 center = new Vector2(block.x + 0.5f, block.y + block.height / 2f) * gridSize;
        Vector2 size = new Vector2(1 - padding.x, block.height - padding.y) * gridSize;
        Gizmos.DrawWireCube(center, size);
    }
}