using System.Collections.Generic;
using UnityEngine;
using System;
using static UnityEditor.PlayerSettings;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Tests")]

public class Sweepotron_AI : MonoBehaviour
{
    public MineMap mineMap;
    bool[,] used;
    int width, height;
    bool videoMode;

    public void Initialize(int widthIn, int heightIn)
    {
        width = widthIn;
        height = heightIn;
        used = new bool[width, height];
    }

    public void SetVideoMode(bool value)
    {
        videoMode = value;
    }

    // Opens squares that match the number of neighboring flags and
    // flags squares around numbers that are fulfilled by flags
    public void SinglePointAlg()
    {
        int currentNum; // number of current square
        int flagNeighbors; // number of neighboring squares that are flagged

        Vector3Int pos = new();
        bool didSomething = true;

        // Break if no progress was made for one iteration
        while (didSomething)
        {
            didSomething = false;

            // Loop over all squares
            for (int ii = 0; ii < width; ii++)
            {
                for (int jj = 0; jj < height; jj++)
                {

                    pos.x = ii;
                    pos.y = jj;

                    if (mineMap.IsRevealed(pos) && !used[pos.x, pos.y])
                    {
                        HashSet<Vector3Int> undecidedNeighbors = GetUndecidedNeighbors(pos);
                        currentNum = mineMap.GetMineOrNumber(pos);
                        if (undecidedNeighbors.Count > 0 && currentNum > 0)
                        {

                            flagNeighbors = mineMap.CountNeighboringFlags(pos);

                            // Open neighbors if the number of neighboring flags matches the number
                            if (currentNum == flagNeighbors)
                            {
                                mineMap.OpenNeighbors(pos);
                                used[pos.x, pos.y] = true;
                                didSomething = true;
                                if (videoMode) return;
                            } 

                            // Flag all neighbors if the number equals the number of unopened squares
                            else if (currentNum == undecidedNeighbors.Count + flagNeighbors)
                            {
                                FlagTiles(undecidedNeighbors);
                                used[pos.x, pos.y] = true;
                                didSomething = true;
                                if (videoMode) return;
                            }

                        } 
                        else
                        {
                            // If it has no undecided neighbors or is a zero, there is no point looking at it again
                            used[pos.x, pos.y] = true;
                        }
                    }
                }
            }
        }
    }

    public void SetOverlapAlg()
    {
        HashSet<Vector3Int> nearbyPos;
        HashSet<Vector3Int> currentUndecided, comparisonUndecided;
        Vector3Int currentPos = new();
        int currentNum, comparsionNum;

        // Loop over all squares
        for (int ii = 0; ii < width; ii++)
        {
            for (int jj = 0; jj < height; jj++)
            {
                currentPos.x = ii;
                currentPos.y = jj;

                if (mineMap.IsRevealed(currentPos) && !used[currentPos.x, currentPos.y]) {

                    nearbyPos = GetRevealedNearby(currentPos);

                    foreach (Vector3Int comparisonPos in nearbyPos)
                    {
                        if (!mineMap.IsRevealed(comparisonPos)) continue;

                        currentUndecided = GetUndecidedNeighbors(currentPos);
                        comparisonUndecided = GetUndecidedNeighbors(comparisonPos);

                        if (comparisonUndecided.Count == 0) continue;

                        
                        currentNum = RemainingMines(currentPos);
                        comparsionNum = RemainingMines(comparisonPos);

                        HashSet<Vector3Int> diffSetCurrent = new(currentUndecided);
                        diffSetCurrent.ExceptWith(comparisonUndecided);

                        HashSet<Vector3Int> diffSetComparison = new(comparisonUndecided);
                        diffSetComparison.ExceptWith(currentUndecided);

                        if (diffSetComparison.Count > 0 && // Comparison square has unique squares
                            currentNum > diffSetCurrent.Count && // Current square can't be fulfilled with its unique squares
                            comparsionNum - (currentNum - diffSetCurrent.Count) == 0) // ComparisonNum - mines in overlapping squares == 0
                        {
                            OpenTiles(diffSetComparison);
                            return;
                        }
                        if (diffSetCurrent.Count > 0 && // Current square has unique squares
                            currentNum - diffSetCurrent.Count == comparsionNum && // Mines in overlapping squares fulfills the comparison number
                            diffSetCurrent.Count == currentNum - comparsionNum) // Unique squares satisfy all remaining mines
                        {
                            FlagTiles(diffSetCurrent);
                            return;
                        }
                    }
                }
            }
        }
    }

    // This algorithm is desgined to finish off the unopened squares at the end.
    // In these scenarios, there are just a few mines left and they need to be brute forced,
    // taking into account the total number of mines.
    public void RecursiveBacktrackingAlg()
    {
        int totalUnassignedMines = mineMap.RemainingMines();
        HashSet<Vector3Int> unknownSquares = GetAllUnknownSquares();
        Dictionary<Vector3Int, int> numberSquares = GetNeighboringNumberSquares(unknownSquares);
        HashSet<Vector3Int> currentMines = new();
        HashSet<Vector3Int> possibleMineSquares = new();

        // Try out mine combinations
        foreach (Vector3Int square in unknownSquares)
        {
            if (possibleMineSquares.Contains(square))
            {
                continue;
            }

            RecursiveBacktrack(totalUnassignedMines);
            RemoveAllMines();

            if (!possibleMineSquares.Contains(square))
            {
                mineMap.OpenTile(square);
                return;
            }
        }

        void RecursiveBacktrack(int unassignedMines)
        {
            // Check if this is a valid solution
            if (unassignedMines == 0)
            {
                foreach (int mineCount in numberSquares.Values)
                {
                    if (mineCount != 0) return;
                }

                // If it gets here, it's a valid solution. Mark all mines in solution as possible mines.
                foreach (Vector3Int mineSquare in currentMines)
                {
                    possibleMineSquares.Add(mineSquare);
                }
                return;
            }

            // Continue recursive backtracking
            foreach (Vector3Int unopenedSquare in unknownSquares)
            {
                if (!CanAddMine(unopenedSquare))
                    continue;

                AddMine(unopenedSquare);
                RecursiveBacktrack(unassignedMines - 1);
                RemoveMine(unopenedSquare);
            }
            return;
        }

        bool CanAddMine(Vector3Int unopenedSquare)
        {
            if (currentMines.Contains(unopenedSquare)) return false;

            HashSet<Vector3Int> neighbors = GetOpenNeighbors(unopenedSquare);

            foreach (Vector3Int neighbor in neighbors)
            {
                if (numberSquares[neighbor] < 1)
                {
                    return false;
                }
            }
            return true;
        }

        void AddMine(Vector3Int unopenedSquare)
        {
            HashSet<Vector3Int> neighbors = GetOpenNeighbors(unopenedSquare);
            foreach (Vector3Int neighbor in neighbors)
            {
                numberSquares[neighbor] -= 1;
            }
            currentMines.Add(unopenedSquare);
        }

        void RemoveMine(Vector3Int unopenedSquare)
        {
            HashSet<Vector3Int> neighbors = GetOpenNeighbors(unopenedSquare);
            foreach (Vector3Int neighbor in neighbors)
            {
                numberSquares[neighbor] += 1;
            }
            currentMines.Remove(unopenedSquare);
        }

        void RemoveAllMines()
        {
            foreach (Vector3Int mineSquare in unknownSquares)
            {
                if (currentMines.Contains(mineSquare))
                {
                    RemoveMine(mineSquare);
                }
            }
        }
    }

    // Create a dictionary of the open squares that neighbor the remaining unopened squares
    // and how many mines they have
    Dictionary<Vector3Int, int> GetNeighboringNumberSquares(HashSet<Vector3Int> unopenedSquares)
    {
        Dictionary<Vector3Int, int> output = new();
        foreach (Vector3Int square in unopenedSquares)
        {
            HashSet<Vector3Int> openNeighbors = GetOpenNeighbors(square);
            foreach (Vector3Int neighbor in openNeighbors)
            {
                output[neighbor] = RemainingMines(neighbor);
            }
        }
        return output;
    }

    public HashSet<Vector3Int> GetAllUnknownSquares()
    {
        HashSet<Vector3Int> output = new();
        for (int posX = 0; posX < width; posX++)
        {
            for (int posY = 0; posY < height; posY++)
            {
                Vector3Int pos = new(posX, posY);
                if (!mineMap.IsRevealed(pos) && !mineMap.IsFlagged(pos))
                {
                    output.Add(pos);
                }
            }
        }
        return output;
    }

    public void ConstraintMapAlg()
    {
        Dictionary<Vector3Int, List<Constraint>> constrainedNums = new();
        Vector3Int chainStartPos = new(-1,0,0);
        bool keepLooping = true;
        Queue<Constraint> constraintQueue = new();
        bool addedConstraint;
        bool madeProgress;

        while (keepLooping)
        {
            Constraint currentConstraint;
            bool startOfChain = false;

            // When the queue is empty, generate a new constraint from the next chain start pos
            if (constraintQueue.Count == 0)
            {
                startOfChain = true;
                chainStartPos = NextMeaningfulSquare(chainStartPos);
                if (chainStartPos.x < 0)
                {
                    // Ran out of starting squares
                    break;
                }

                currentConstraint = GenerateConstraint(chainStartPos);

                addedConstraint = AddConstraint(ref constrainedNums, ref currentConstraint);
                if (!addedConstraint)
                {
                    // Constraint & follow-ons have already been explored/queued
                    continue;
                }
            }
            else
            {
                currentConstraint = constraintQueue.Dequeue();
            }

            // Check if this lets us open/flag any squares
            madeProgress = ApplyConstraint(currentConstraint);

            if (madeProgress) 
            {
                return;
            }

            // Combine current constraint with existing constraints
            EnqueueCombinationConstraints(currentConstraint, ref constraintQueue, ref constrainedNums);

            // Translate current constraint to neighboring squares

            if (!startOfChain)
            {
                // Generate complementary constraint
                Constraint complementaryConstraint = GenerateComplementaryConstraint(currentConstraint);
                addedConstraint = AddConstraint(ref constrainedNums, ref complementaryConstraint);

                if (addedConstraint)
                {
                    constraintQueue.Enqueue(complementaryConstraint);
                }

                
            }

            HashSet<Vector3Int> newConstraintSquares = BorderAtLeastTwo(currentConstraint.constrainedSquares, currentConstraint.numberSquare);
            foreach (Vector3Int numberSquare in newConstraintSquares)
            {
                Constraint translatedConstraint = TranslateConstraint(numberSquare, currentConstraint);

                addedConstraint = AddConstraint(ref constrainedNums, ref translatedConstraint);
                if (!addedConstraint)
                {
                    // Constraint & follow-ons have already been explored
                    continue;
                }
                constraintQueue.Enqueue(translatedConstraint);

            }

        }
    }

    // Combine constraint with all existing constraints to create new constraints
    void EnqueueCombinationConstraints(Constraint inputConstraint,
                                       ref Queue<Constraint> constraintQueue,
                                       ref Dictionary<Vector3Int, List<Constraint>> constrainedNums)
    {
        List<Constraint> allConstraints = new(constrainedNums[inputConstraint.numberSquare]);
        int remainingMines = RemainingMines(inputConstraint.numberSquare);

        foreach (Constraint constraint in allConstraints) 
        {
            if (constraint.constrainedSquares.Overlaps(inputConstraint.constrainedSquares))
            {
                continue;
            }

            Constraint outputConstraint = new(inputConstraint.numberSquare,
                                              inputConstraint.constrainedSquares,
                                              inputConstraint.minMines + constraint.minMines,
                                              Mathf.Min(inputConstraint.maxMines + constraint.maxMines, remainingMines));

            outputConstraint.constrainedSquares.UnionWith(constraint.constrainedSquares);

            if (!IsMeaningful(outputConstraint))
            {
                continue;
            }

            bool addedConstraint = AddConstraint(ref constrainedNums, ref outputConstraint);
            if (addedConstraint)
            {
                constraintQueue.Enqueue(outputConstraint);
            }
        }
    }

    // Apply the constraint to see if we can open/flag any squares
    bool ApplyConstraint(Constraint constraint)
    {
        if (constraint.maxMines == 0)
        {
            OpenTiles(constraint.constrainedSquares);
            return true;
        }

        if (constraint.minMines == constraint.constrainedSquares.Count)
        {
            FlagTiles(constraint.constrainedSquares);
            return true;
        }

        return false;
    }

    // Try to add constraint to the dictionary. If it already exists or is not meaningful, return false
    bool AddConstraint(ref Dictionary<Vector3Int, List<Constraint>> constrainedNums, 
                       ref Constraint constraint)
    {
        if (!IsMeaningful(constraint))
        {
            // Constraint has no useful info
            return false;
        }

        if (!constrainedNums.ContainsKey(constraint.numberSquare))
        {
            constrainedNums[constraint.numberSquare] = new List<Constraint>();
        }
        else
        {
            // Check if constraint already exists, if so return false
            // If a constraint already exists but with wider bounds on the number of mines, narrow them.
            foreach (Constraint comparison in constrainedNums[constraint.numberSquare])
            {
                if (!constraint.SameSquares(comparison))
                {
                    continue;
                }

                // 100% identical, so discard it
                if (constraint.minMines == comparison.minMines &&
                    constraint.maxMines == comparison.maxMines)
                {
                    return false;
                }

                // If strictly wider than existing comparison constraint, discard it
                if (constraint.maxMines >= comparison.maxMines && constraint.minMines <= comparison.minMines)
                {
                    return false;
                }

                constraint.minMines = Mathf.Max(constraint.minMines, comparison.minMines);
                constraint.maxMines = Mathf.Min(constraint.maxMines, comparison.maxMines);

                comparison.minMines = constraint.minMines;
                comparison.maxMines = constraint.maxMines;
                return true;
            }
        }

        constrainedNums[constraint.numberSquare].Add(constraint);
        return true;
    }

    bool IsMeaningful(Constraint constraint)
    {
        if (constraint.constrainedSquares.Count == 0)
        {
            return false;
        }
        if (constraint.minMines == 0 && constraint.maxMines == constraint.constrainedSquares.Count)
        {
            return false;
        }

        return true;
    }

    // Create constraint from just a number square
    Constraint GenerateConstraint(Vector3Int pos)
    {
        HashSet<Vector3Int> nonConstrainedNeighbors = GetUndecidedNeighbors(pos);
        int remainingMines = RemainingMines(pos);
        return new Constraint(pos, nonConstrainedNeighbors, remainingMines, remainingMines);
    }

    // Apply the constraint to the other undecided neighbors around a number square
    Constraint GenerateComplementaryConstraint(Constraint inputConstraint)
    {
        int remainingMines = RemainingMines(inputConstraint.numberSquare);
        HashSet<Vector3Int> nonConstrainedNeighbors = GetUndecidedNeighbors(inputConstraint.numberSquare);
        foreach (Vector3Int constrainedPos in inputConstraint.constrainedSquares)
        {
            // All around the same number, so shouldn't need to check that they're neighbors.
            // Potential to have some issues if we flag/open a square and then keep running
            nonConstrainedNeighbors.Remove(constrainedPos);
        }

        return new Constraint(inputConstraint.numberSquare,
                              nonConstrainedNeighbors,
                              remainingMines - inputConstraint.maxMines,  // min is remaining - max
                              remainingMines - inputConstraint.minMines); // max is remaining - min
    }

    // Keep the same constrained squares and limits but apply it to a different number square
    Constraint TranslateConstraint(Vector3Int numberSquare, Constraint inputConstraint)
    {
        Constraint outputConstraint = new(inputConstraint);
        outputConstraint.numberSquare = numberSquare;

        HashSet<Vector3Int> constrainedSquares = new(outputConstraint.constrainedSquares);
        foreach (Vector3Int constrainedPos in constrainedSquares)
        {
            if (!IsNeighbor(numberSquare, constrainedPos))
            {
                outputConstraint.constrainedSquares.Remove(constrainedPos);
                outputConstraint.minMines -= 1;
            }
        }

        // maxMines can't be greater than remaining mines
        int remainingMines = RemainingMines(numberSquare);
        outputConstraint.maxMines = Mathf.Min(outputConstraint.maxMines, remainingMines);

        // minMines can't be less than 0.
        outputConstraint.minMines = Mathf.Max(outputConstraint.minMines, 0);

        return outputConstraint;
    }

    internal static bool IsNeighbor(Vector3Int pos, Vector3Int neighbor)
    {
        return (Mathf.Abs(pos.x - neighbor.x) <= 1) && (Mathf.Abs(pos.y - neighbor.y) <= 1);
    }

    // Return all open squares that border at least 2 of the input squares and are not the current position.
    HashSet<Vector3Int> BorderAtLeastTwo(HashSet<Vector3Int> inputSquares, Vector3Int currentPos)
    {
        HashSet<Vector3Int> seenOnce = new();
        HashSet<Vector3Int> seenTwice = new();

        foreach (Vector3Int inputPos in inputSquares)
        {
            HashSet<Vector3Int> openNeighbors = GetOpenNeighbors(inputPos);

            foreach (Vector3Int openNeighborPos in openNeighbors)
            {
                if (openNeighborPos == currentPos)
                {
                    continue;
                }

                if (seenOnce.Contains(openNeighborPos))
                {
                    seenTwice.Add(openNeighborPos);
                    continue;
                }

                seenOnce.Add(openNeighborPos);
            }
        }

        return seenTwice;
    }

    HashSet<Vector3Int> GetOpenNeighbors(Vector3Int inputPos)
    {
        HashSet<Vector3Int> neighbors = mineMap.GetNeighbors(inputPos);
        HashSet<Vector3Int> openNeighbors = new();
        foreach (Vector3Int testPos in neighbors)
        {
            if (mineMap.IsRevealed(testPos))
            {
                openNeighbors.Add(testPos);
            }
        }
        return openNeighbors;
    }

    Vector3Int NextMeaningfulSquare(Vector3Int pos)
    {
        // Increment pos to next number square with undecided neighbors
        bool validSquare = false;
        while (!validSquare)
        {
            pos = NextOpenSquare(pos);

            if (pos.x < 0)
            {
                return pos;
            }

            int remainingMines = RemainingMines(pos);
            if (remainingMines < 1) continue;

            return pos;
        }
        return pos;
    }

    Vector3Int NextOpenSquare(Vector3Int pos)
    {
        bool isRevealed = false;

        while(!isRevealed)
        {
            pos.x += 1;
            if (pos.x == width)
            {
                pos.x = 0;
                pos.y += 1;
                if (pos.y == height)
                {
                    pos.x = -999;
                    pos.y = -999;
                    return pos;
                }
            }
            isRevealed = mineMap.IsRevealed(pos);
        }
        return pos;
    }

    HashSet<Vector3Int> GetUndecidedNeighbors(Vector3Int pos)
    {
        HashSet<Vector3Int> neighbors = mineMap.GetNeighbors(pos);
        HashSet<Vector3Int> undecidedNeighbors = new();

        foreach (Vector3Int currentPos in neighbors)
        {
            if (!mineMap.IsRevealed(currentPos) && !mineMap.IsFlagged(currentPos))
            {
                undecidedNeighbors.Add(currentPos);
            }
        }

        return undecidedNeighbors;
    }

    HashSet<Vector3Int> GetRevealedNearby(Vector3Int pos)
    {
        HashSet<Vector3Int> revealedNearby = new();
        Vector3Int currentPos = pos;

        int horz_min = Mathf.Max(0, pos.x - 2);
        int horz_max = Mathf.Min(width - 1, pos.x + 2);
        int vert_min = Mathf.Max(0, pos.y - 2);
        int vert_max = Mathf.Min(height - 1, pos.y + 2);

        for (int ii = horz_min; ii <= horz_max; ii++)
        {
            for (int jj = vert_min; jj <= vert_max; jj++)
            {
                currentPos.x = ii;
                currentPos.y = jj;
                if (currentPos == pos) continue;
                if (mineMap.IsRevealed(currentPos))
                {
                    revealedNearby.Add(currentPos);
                }
            }
            
        }

        return revealedNearby;
    }

    int RemainingMines(Vector3Int pos)
    {
        return mineMap.GetMineOrNumber(pos) - mineMap.CountNeighboringFlags(pos);
    }

    void FlagTiles(HashSet<Vector3Int> tilesToOpen)
    {
        foreach (Vector3Int pos in tilesToOpen)
        {
            if (!mineMap.IsFlagged(pos))
            {
                mineMap.ToggleFlag(pos);
            }
        }
    }

    void OpenTiles(HashSet<Vector3Int> tilesToOpen)
    {
        foreach (Vector3Int pos in tilesToOpen)
        {
            mineMap.OpenTile(pos);
        }
    }
}
