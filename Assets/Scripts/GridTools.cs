using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class BlockLine
{
    [HideInInspector]
    public string Name;

    public LineOrientation lineOrientation;
    public LineDirection lineDirection;
    public List<LetterBlock> blocks = new List<LetterBlock>();

    public BlockLine(string _name)
    {
        Name = _name;
        // lineDirection = GridTools.GetLineDirection(this);
        // lineOrientation = GridTools.GetLineOrientation(this);
    }

    public BlockLine(string _name, List<LetterBlock> _blocks)
    {
        Name = _name;
        blocks = _blocks;
        lineDirection = GridTools.GetLineDirection(this);
        lineOrientation = GridTools.GetLineOrientation(this);
    }


}

public static class GridTools
{

    //Get lines of blocks
    public static List<BlockLine> GetEmptyLines(GridGenerator grid, LetterBlock startBlock)
    {

        //Our list that hold up right down left empty lines
        List<BlockLine> emptyLines = new List<BlockLine>();

        BlockLine rightLine = new BlockLine("Empty line right");
        BlockLine downLine = new BlockLine("Empty line down");
        BlockLine upLine = new BlockLine("Empty line up");
        BlockLine leftLine = new BlockLine("Empty line left");

        //Add our lines
        //Preference dictated by order of addition
        emptyLines.AddRange(new BlockLine[] { rightLine, downLine, leftLine, upLine });

        //If the desired start block isn't empty
        if (!IsInBounds(new Vector2Int(startBlock.gridRef.x, startBlock.gridRef.y)) ||
            !IsEmpty(new Vector2Int(startBlock.gridRef.x, startBlock.gridRef.y)) ||
            HasAdjacencies(new Vector2Int(startBlock.gridRef.x, startBlock.gridRef.y)))
        {
            Debug.Log("The desired start block is not valid - there are no empty lines");
            //Return our empty lists
            return emptyLines;
        }
        else
        {
            //Add the start block to each line
            rightLine.blocks.Add(startBlock);
            downLine.blocks.Add(startBlock);
            upLine.blocks.Add(startBlock);
            leftLine.blocks.Add(startBlock);


            Debug.Log("The desired start block was validated");

            //Add to rightline
            ///////////////
            for (int i = 1; i < Mathf.Infinity; i++)
            {
                if (!IsInBounds(new Vector2Int(startBlock.gridRef.x + i, startBlock.gridRef.y))) break;

                if (IsEmpty(new Vector2Int(startBlock.gridRef.x + i, startBlock.gridRef.y)) &&
                !HasAdjacencies(new Vector2Int(startBlock.gridRef.x + i, startBlock.gridRef.y)))
                {
                    rightLine.blocks.Add(grid.letterBlocks[startBlock.gridRef.x + i, startBlock.gridRef.y]);
                }
                else break;
            }
            ///////////////

            //Add to downline
            ///////////////
            for (int i = 1; i < Mathf.Infinity; i++)
            {
                if (!IsInBounds(new Vector2Int(startBlock.gridRef.x, startBlock.gridRef.y - i))) break;

                if (IsEmpty(new Vector2Int(startBlock.gridRef.x, startBlock.gridRef.y - i)) &&
                !HasAdjacencies(new Vector2Int(startBlock.gridRef.x, startBlock.gridRef.y - i)))
                {
                    downLine.blocks.Add(grid.letterBlocks[startBlock.gridRef.x, startBlock.gridRef.y - i]);
                }
                else break;
            }
            ///////////////

            //Add to upline
            ///////////////
            for (int i = 1; i < Mathf.Infinity; i++)
            {
                if (!IsInBounds(new Vector2Int(startBlock.gridRef.x, startBlock.gridRef.y + i))) break;

                if (IsEmpty(new Vector2Int(startBlock.gridRef.x, startBlock.gridRef.y + i)) &&
                !HasAdjacencies(new Vector2Int(startBlock.gridRef.x, startBlock.gridRef.y + i)))
                {
                    upLine.blocks.Add(grid.letterBlocks[startBlock.gridRef.x, startBlock.gridRef.y + i]);
                }
                else break;
            }
            ///////////////

            //Add to leftline
            ///////////////
            for (int i = 1; i < Mathf.Infinity; i++)
            {
                if (!IsInBounds(new Vector2Int(startBlock.gridRef.x - i, startBlock.gridRef.y))) break;

                if (IsEmpty(new Vector2Int(startBlock.gridRef.x - i, startBlock.gridRef.y)) &&
                !HasAdjacencies(new Vector2Int(startBlock.gridRef.x - i, startBlock.gridRef.y)))
                {
                    leftLine.blocks.Add(grid.letterBlocks[startBlock.gridRef.x - i, startBlock.gridRef.y]);
                }
                else break;
            }
            ///////////////

            rightLine.blocks.OrderBy(x => x.gridRef.x);
            leftLine.blocks.OrderBy(x => x.gridRef.x);
            downLine.blocks.OrderBy(x => x.gridRef.y);
            upLine.blocks.OrderBy(x => x.gridRef.y);



            Debug.Log("Upline count = " + upLine.blocks.Count);
            Debug.Log("Rightline count = " + rightLine.blocks.Count);
            Debug.Log("Downline count = " + downLine.blocks.Count);
            Debug.Log("Leftline count = " + leftLine.blocks.Count);

            return emptyLines;
        }
    }

    public static LineDirection GetLineDirection(BlockLine line)
    {

        if (line.lineOrientation == LineOrientation.Unknown) line.lineOrientation = GetLineOrientation(line);

        if (line.lineOrientation == LineOrientation.Unknown) return LineDirection.Unknown;


        //Find out if its a column or a row
        //Same x // column
        if (line.lineOrientation == LineOrientation.Vert)
        {
            if (line.blocks[0].gridRef.y > line.blocks[line.blocks.Count - 1].gridRef.y) return LineDirection.Forwards;
            else return LineDirection.Backwards;
        }

        else if (line.lineOrientation == LineOrientation.Horiz)
        {
            if (line.blocks[0].gridRef.x < line.blocks[line.blocks.Count - 1].gridRef.x) return LineDirection.Forwards;
            else return LineDirection.Backwards;
        }

        else return LineDirection.Unknown;

    }

    public static LineOrientation GetLineOrientation(BlockLine line)
    {

        if (line.blocks.Count < 2) return LineOrientation.Unknown;
        //Find out if its a column or a row
        //Same x // column
        else if (line.blocks[0].gridRef.x == line.blocks[line.blocks.Count - 1].gridRef.x) return LineOrientation.Vert;
        //Same y // row1
        else if (line.blocks[0].gridRef.y == line.blocks[line.blocks.Count - 1].gridRef.y) return LineOrientation.Horiz;

        else return LineOrientation.Unknown;
    }

    public static bool IsInBounds(Vector2Int checkRef)
    {

        GridGenerator grid = BrainControl.Get().grid;

        //If the grid ref is outwith the bounds of grid
        if (checkRef.x < 0 || checkRef.x > grid.letterBlocks.GetLength(0) - 1 || checkRef.y < 0 || checkRef.y > grid.letterBlocks.GetLength(1) - 1) return false;
        return true;
    }

    public static bool IsEmpty(Vector2Int checkRef)
    {

        LetterBlock checkBlock = null;
        GridGenerator grid = BrainControl.Get().grid;

        //If it has been assigned
        if (grid.letterBlocks[checkRef.x, checkRef.y] != null) checkBlock = grid.letterBlocks[checkRef.x, checkRef.y];
        else return false;

        //If the checked block is filled
        if (checkBlock.fillState == FillState.filled) return false;

        else return true;
    }

    //TODO: Make this return a list of adjacencies and check the count for a bool
    public static bool HasAdjacencies(Vector2Int blockRef)
    {

        Vector2Int checkRef;
        GridGenerator grid = BrainControl.Get().grid;

        //Check above
        checkRef = new Vector2Int(blockRef.x, blockRef.y + 1);

        if (IsInBounds(checkRef))
        {
            if (!IsEmpty(checkRef)) return true;
        }
        else return false;


        //Check right
        checkRef = new Vector2Int(blockRef.x + 1, blockRef.y);
        if (IsInBounds(checkRef))
        {
            if (!IsEmpty(checkRef)) return true;
        }
        else return false;

        //Check below
        checkRef = new Vector2Int(blockRef.x, blockRef.y - 1);
        if (IsInBounds(checkRef))
        {
            if (!IsEmpty(checkRef)) return true;
        }
        else return false;

        //Check left
        checkRef = new Vector2Int(blockRef.x - 1, blockRef.y);
        if (IsInBounds(checkRef))
        {
            if (!IsEmpty(checkRef)) return true;
        }
        else return false;

        return false;
    }

    public static List<BlockLine> GetFilledCross(LetterBlock block)
    {
        //Horiz
        BlockLine horiz = new BlockLine("horiz");
        BlockLine vert = new BlockLine("vert");

        horiz.lineOrientation = LineOrientation.Horiz;
        vert.lineOrientation = LineOrientation.Vert;


        horiz.blocks.Add(block);
        vert.blocks.Add(block);

        bool left = true;
        bool right = true;
        bool up = true;
        bool down = true;

        for (int i = 1; i < Mathf.Max(BrainControl.Get().grid.letterBlocks.GetLength(0), BrainControl.Get().grid.letterBlocks.GetLength(1)); i++)
        {



            if (right == true)
            {
                if (IsInBounds(new Vector2Int(block.gridRef.x + i, block.gridRef.y)) && !IsEmpty(new Vector2Int(block.gridRef.x + i, block.gridRef.y)))
                {
                    horiz.blocks.Add(BrainControl.Get().grid.letterBlocks[block.gridRef.x + i, block.gridRef.y]);
                    // right = true;
                }
                else right = false;
            }

            if (left == true)
            {
                if (IsInBounds(new Vector2Int(block.gridRef.x - i, block.gridRef.y)) && !IsEmpty(new Vector2Int(block.gridRef.x - i, block.gridRef.y)))
                {
                    horiz.blocks.Add(BrainControl.Get().grid.letterBlocks[block.gridRef.x - i, block.gridRef.y]);
                    // left = true;
                }
                else left = false;
            }

            if (up == true)
            {
                if (IsInBounds(new Vector2Int(block.gridRef.x, block.gridRef.y + i)) && !IsEmpty(new Vector2Int(block.gridRef.x, block.gridRef.y + i)))
                {
                    vert.blocks.Add(BrainControl.Get().grid.letterBlocks[block.gridRef.x, block.gridRef.y + i]);
                    // up = true;
                }
                else up = false;
            }

            if (down == true)
            {
                if (IsInBounds(new Vector2Int(block.gridRef.x, block.gridRef.y - i)) && !IsEmpty(new Vector2Int(block.gridRef.x, block.gridRef.y - i)))
                {
                    vert.blocks.Add(BrainControl.Get().grid.letterBlocks[block.gridRef.x, block.gridRef.y - i]);
                    // down = true;
                }
                else down = false;
            }

            if (!right && !left && !up && !down)
            {
                Debug.Log("Word cross was completed");
                return new List<BlockLine>() { horiz, vert };
            }

        }

        Debug.Log("Word cross was completed");
        return new List<BlockLine>() { horiz, vert };

    }

    public static bool WordIntoLine(WordRequest request, bool lockBlock)
    {

        GridGenerator grid = BrainControl.Get().grid;

        Debug.Log("Placing word: Grid = " + grid + ", request = " + request.word + " into " + request.placementType.ToString());

        //Get a placement
        Vector2Int placement = GetPlacement(grid, request.word, request.placementType);

        Debug.Log("Block at placement start is " + grid.letterBlocks[placement.x, placement.y]);

        //Get possible placements
        List<BlockLine> possibleLines = GetEmptyLines(grid, grid.letterBlocks[placement.x, placement.y]);

        //The returned line
        BlockLine line = null;

        //Find an empty line
        for (int i = 0; i < possibleLines.Count - 1; i++)
        {
            if (possibleLines[i].blocks.Count >= request.word.Length)
            {
                line = possibleLines[i];
                break;
            }
        }


        if (line != null)
        {
            if (GetLineDirection(line) == LineDirection.Forwards)
            {
                for (int i = 0; i < request.word.Length; i++)
                {
                    //Build the block from desired letter
                    line.blocks[i].BuildFromLetter(request.word[i]);
                }
            }
            else
            {
                for (int i = request.word.Length - 1; i > -1; i--)
                {
                    //Build the block from desired letter
                    line.blocks[i].BuildFromLetter(request.word[i]);
                }
            }



            //Add the assigned blocks to a level input
            //Oh I don't like this being assigned directly
            //TODO: fix
            BrainControl.Get().sessionManager.currentSession.currentLevel.inputs.Add(new BlockInput(line.blocks, true, false));

            return true;
        }
        else
        {
            Debug.Log("None of the lines were long enough");
            return false;
        }
    }

    public static Vector2Int GetPlacement(GridGenerator grid, string word, PlacementType p)
    {
        switch (p)
        {
            case PlacementType.BottomLeft: return new Vector2Int(0, 0);
            case PlacementType.BottomRight: return new Vector2Int(grid.letterBlocks.GetLength(0) - 1, 0);
            case PlacementType.TopRight: return new Vector2Int(grid.letterBlocks.GetLength(0) - 1, grid.letterBlocks.GetLength(1) - 1);
            case PlacementType.TopLeft: return new Vector2Int(0, grid.letterBlocks.GetLength(1) - 1);
            case PlacementType.Center: return new Vector2Int(grid.letterBlocks.GetLength(0) / 2, grid.letterBlocks.GetLength(1) / 2);

            case PlacementType.Random:
                Vector2Int r = Vector2Int.zero;
                int t = 0;
                //Random needs to be done iteratively
                do
                {
                    r = new Vector2Int(Random.Range(0, grid.letterBlocks.GetLength(0) - 1), Random.Range(0, grid.letterBlocks.GetLength(1) - 1));
                    t += 1;
                }
                while ((GetEmptyLines(grid, grid.letterBlocks[r.x, r.y]).Where(x => x.blocks.Count >= word.Length)).Count() < 1 && t < 20);
                return r;

            default: return new Vector2Int(0, 0);
        }
    }

    public static (string forwards, string backwards) WordFromLine(BlockLine line)
    {

        string forwards = "";
        string backwards = "";

        for (int i = 0; i < line.blocks.Count; i++)
        {
            forwards += line.blocks[i].letter.character;
        }

        for (int i = line.blocks.Count - 1; i > -1; i--)
        {
            backwards += line.blocks[i].letter.character;
        }

        return (forwards, backwards);

    }




}