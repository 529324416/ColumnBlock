using System;
using System.Collections.Generic;

public interface IGrid{

    int width{get;}
    int height{get;}
    bool isObstacle(int x, int y);
    bool isEmpty(int x, int y);
}

public struct ColumnBlock{
    public int id;
    public int x;
    public int y;
    public int height;
    public ColumnBlock(int id, int x, int y, int height){
        this.id = id;
        this.x = x;
        this.y = y;
        this.height = height;
    }
    public override int GetHashCode(){
        return id;
    }
    public override bool Equals(object obj){
        if(obj is ColumnBlock){
            return id == obj.GetHashCode();
        }
        return false;
    }
    public static bool operator>(ColumnBlock left, ColumnBlock right){
        return left.x > right.x;
    }
    public static bool operator<(ColumnBlock left, ColumnBlock right){
        return left.x < right.x;
    }
    public static bool operator>=(ColumnBlock left, ColumnBlock right){
        return left.x >= right.x;
    }
    public static bool operator<=(ColumnBlock left, ColumnBlock right){
        return left.x <= right.x;
    }
    public static bool operator==(ColumnBlock left, ColumnBlock right){
        return left.id == right.id;
    }
    public static bool operator!=(ColumnBlock left, ColumnBlock right){
        return left.id != right.id;
    }
}

public struct CombinedColumnBlock{

    public int x;
    public int y;
    public int width;
    public int height;
    public int[] ids;       // 其合并的柱块Id列表
    public CombinedColumnBlock(int x, int y, int width, int height, int[] ids){
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
        this.ids = ids;
    }
}


public static class ColumnBlockHelper{

    /// <summary>搜索可活动范围</summary>
    public static CombinedColumnBlock[] searchActiveRange(ColumnBlock[] blocks){

        Dictionary<int, List<ColumnBlock>> dict = classifyColumnBlocksByHeight(blocks);
        List<CombinedColumnBlock> buffer = new List<CombinedColumnBlock>();
        foreach(List<ColumnBlock> subblocks in dict.Values){
            foreach(CombinedColumnBlock block in searchActiveRange(subblocks)){
                buffer.Add(block);
            }
        }
        return buffer.ToArray();
    }
    /// <summary>搜索可活动范围</summary>
    private static IEnumerable<CombinedColumnBlock> searchActiveRange(List<ColumnBlock> blocks){

        if(blocks.Count == 1){
            ColumnBlock b = blocks[0];
            yield return new CombinedColumnBlock(b.x, b.y, 1, 1, new int[]{b.id});
        }else if(blocks.Count > 1){
            ColumnBlock start = blocks[0];
            ColumnBlock prev = blocks[0];
            ColumnBlock current;
            List<int> buffer = new List<int>();
            for(int i = 1; i < blocks.Count; i ++){
                current = blocks[i];
                if(current.x - prev.x == 1){
                    prev = current;
                    buffer.Add(current.id);
                }else{
                    yield return new CombinedColumnBlock(start.x, start.y, prev.x - start.x + 1, 1, buffer.ToArray());
                    buffer.Clear();
                    start = current;
                    prev = current;
                }
            }
            yield return new CombinedColumnBlock(start.x, start.y, prev.x - start.x + 1, 1, buffer.ToArray());
        }
    }


    /// <summary>根据宽度来合并柱块</summary>
    public static CombinedColumnBlock[] combineBlocks(ColumnBlock[] blocks, int width){

        List<CombinedColumnBlock> result = new List<CombinedColumnBlock>();
        Dictionary<int, List<ColumnBlock>> dict = classifyColumnBlocksByHeight(blocks);
        foreach(List<ColumnBlock> subblocks in dict.Values){
            foreach(CombinedColumnBlock block in combineBlocks(subblocks, width)){
                result.Add(block);
            }
        }
        return result.ToArray();
    }
    /// <summary>将所有的柱块按照柱点y坐标进行分类</summary>
    public static Dictionary<int, List<ColumnBlock>> classifyColumnBlocksByHeight(ColumnBlock[] blocks){

        Dictionary<int, List<ColumnBlock>> dict = new Dictionary<int, List<ColumnBlock>>();
        foreach(ColumnBlock block in blocks){
            if(!dict.ContainsKey(block.y)){
                dict.Add(block.y, new List<ColumnBlock>());
            }
            dict[block.y].Add(block);
        }
        return dict;
    }

    /// <summary>给定一组拥有同一柱点高度的柱块,通过滑窗搜索思想将其合并为数个合并柱块</summary>
    public static IEnumerable<CombinedColumnBlock> combineBlocks(List<ColumnBlock> blocks, int width){
        
        if(blocks.Count == width){
            // 如果宽度正好与blocks的长度相等，则这个列表至多会产生一个可能的合并柱块

            if(isColumnBlockContinues(blocks.ToArray(), out CombinedColumnBlock block)){
                yield return block;
            }
        }else if(blocks.Count > width){
            // 进行滑窗搜索

            int pos = 0;
            int finalPosition = blocks.Count - width;   // 最后一个滑窗位置
            ColumnBlock[] buffer = new ColumnBlock[width];
            while(pos <= finalPosition){
                blocks.CopyTo(pos, buffer, 0, width);
                if(isColumnBlockContinues(buffer, out CombinedColumnBlock block)){
                    yield return block;
                    pos += width;
                    continue;
                }
                pos ++;
            }
        }
    }

    /// <summary>判断一组柱块是否是连续的</summary>
    public static bool isColumnBlockContinues(ColumnBlock[] blocks, out CombinedColumnBlock blcok){

        blcok = default(CombinedColumnBlock);
        if(blocks == null || blocks.Length <= 1)return false;
        int x = blocks[0].x;
        int minHeight = blocks[0].height;

        // 判断柱块是否连续
        for(int i = 1; i < blocks.Length; i ++){
            if(blocks[i].x - x == i){
                minHeight = Math.Min(minHeight, blocks[i].height);
                continue;
            }
            return false;
        }

        // 获取所有子柱块的id并返回一个合并柱块
        int[] ids = new int[blocks.Length];
        for(int i = 0; i < blocks.Length; i ++){
            ids[i] = blocks[i].id;
        }
        blcok = new CombinedColumnBlock(x, blocks[0].y, blocks.Length, minHeight, ids);
        return true;     
    }



    /// <summary>搜索一个地图内所有的柱块</summary>
    public static ColumnBlock[] searchColumnBlocks(IGrid map){

        if(map.width <= 0 || map.height <= 0)return new ColumnBlock[0];

        // 搜索所有的柱块并存入列表中
        List<ColumnBlock> blocks = new List<ColumnBlock>();
        for(int x = 0; x < map.width; x ++){
            foreach(ColumnBlock block in searchColumnBlocks(map, x)){
                blocks.Add(block);
            }
        }

        // 修改每个柱块的id为其在列表中的索引
        ColumnBlock[] output = new ColumnBlock[blocks.Count];
        for(int i = 0; i < blocks.Count; i ++){
            output[i] = blocks[i];
            output[i].id = i;
        }
        return output;
    }
    /// <summary>搜索给定柱体内所有的柱块</summary>
    private static IEnumerable<ColumnBlock> searchColumnBlocks(IGrid map, int x){
        
        bool lstGrounded = false;           // 标记前一个位置是否是实体
        for(int y = 0; y < map.height; y ++){
            if(lstGrounded){
                if(map.isEmpty(x, y)){
                    // 满足柱块的条件,开始进入柱块搜索器开始查询一个柱块的高度

                    yield return findColumnBlock(map, x, y, out int offset);
                    y += offset;
                }
            }else{
                lstGrounded = map.isObstacle(x, y);
            }
        }
    }
    /// <summary>探索一个柱块的长度,并返回一个柱块结构体</summary>
    private static ColumnBlock findColumnBlock(IGrid map, int x, int y, out int height){

        // 由于柱块的起始条件是遇到一个空白的区域，所以柱块的高度至少为1,而检测位置是从该空白区域的下一个位置开始
        height = 1;
        for(int j = y + 1; j < map.height; j ++){
            if(map.isEmpty(x, j)){
                height ++;
                continue;
            }
            break;
        }
        return new ColumnBlock(-1, x, y, height);
    }

}