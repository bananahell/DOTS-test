using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct FindPathJob : IJob {

  public int2 startPosition;
  public int2 endPosition;
  public int2 gridSize;
  public float triangleSide;
  public float triangleHeight;
  public NativeList<int2> path;

  public void Execute() {
    NativeArray<PathNode> pathNodeArray = new(this.gridSize.x * this.gridSize.y, Allocator.Temp);
    for (int x = 0; x < this.gridSize.x; x++) {
      for (int y = 0; y < this.gridSize.y; y++) {
        PathNode pathNode = new() {
          x = x,
          y = y,
          index = this.CalculateIndex(x, y, this.gridSize.x),
          gCost = int.MaxValue,
          hCost = this.CalculateDistanceCost(new int2(x, y), this.endPosition)
        };
        pathNode.CalculateFCost();
        pathNode.isWalkable = true;
        pathNode.cameFromNodeIndex = -1;
        pathNodeArray[pathNode.index] = pathNode;
      }
    }
    NativeArray<int2> neighbourOffsetArrayUp = new(6, Allocator.Temp);
    neighbourOffsetArrayUp[0] = new int2(-1, 0);
    neighbourOffsetArrayUp[1] = new int2(+1, 0);
    neighbourOffsetArrayUp[2] = new int2(0, +1);
    neighbourOffsetArrayUp[3] = new int2(0, -1);
    neighbourOffsetArrayUp[4] = new int2(-2, -1);
    neighbourOffsetArrayUp[5] = new int2(+2, -1);
    NativeArray<int2> neighbourOffsetArrayDown = new(6, Allocator.Temp);
    neighbourOffsetArrayDown[0] = new int2(-1, 0);
    neighbourOffsetArrayDown[1] = new int2(+1, 0);
    neighbourOffsetArrayDown[2] = new int2(0, +1);
    neighbourOffsetArrayDown[3] = new int2(0, -1);
    neighbourOffsetArrayDown[4] = new int2(-2, +1);
    neighbourOffsetArrayDown[5] = new int2(+2, +1);
    int endNodeIndex = this.CalculateIndex(this.endPosition.x, this.endPosition.y, this.gridSize.x);
    PathNode startNode = pathNodeArray[this.CalculateIndex(this.startPosition.x, this.startPosition.y, this.gridSize.x)];
    startNode.gCost = 0;
    startNode.CalculateFCost();
    pathNodeArray[startNode.index] = startNode;
    NativeList<int> openList = new(Allocator.Temp);
    NativeList<int> closedList = new(Allocator.Temp);
    openList.Add(startNode.index);
    while (openList.Length > 0) {
      int currentNodeIndex = this.GetLowestCostFNodeIndex(openList, pathNodeArray);
      PathNode currentNode = pathNodeArray[currentNodeIndex];
      if (currentNodeIndex == endNodeIndex) {
        // Reached our destination!
        break;
      }
      // Remove current node from Open List
      for (int i = 0; i < openList.Length; i++) {
        if (openList[i] == currentNodeIndex) {
          openList.RemoveAtSwapBack(i);
          break;
        }
      }
      closedList.Add(currentNodeIndex);
      for (int i = 0; i < neighbourOffsetArrayUp.Length; i++) {
        int2 neighbourOffset = currentNode.x % 2 == 0
          ? currentNode.y % 2 == 0 ? neighbourOffsetArrayUp[i] : neighbourOffsetArrayDown[i]
          : currentNode.y % 2 == 0 ? neighbourOffsetArrayDown[i] : neighbourOffsetArrayUp[i];
        int2 neighbourPosition = new(currentNode.x + neighbourOffset.x, currentNode.y + neighbourOffset.y);
        if (!this.IsPositionInsideGrid(neighbourPosition, this.gridSize)) {
          // Neighbour not valid position
          continue;
        }
        int neighbourNodeIndex = this.CalculateIndex(neighbourPosition.x, neighbourPosition.y, this.gridSize.x);
        if (closedList.Contains(neighbourNodeIndex)) {
          // Already searched this node
          continue;
        }
        PathNode neighbourNode = pathNodeArray[neighbourNodeIndex];
        if (!neighbourNode.isWalkable) {
          // Not walkable
          continue;
        }
        int2 currentNodePosition = new(currentNode.x, currentNode.y);
        float tentativeGCost = currentNode.gCost + this.CalculateDistanceCost(currentNodePosition, neighbourPosition);
        if (tentativeGCost < neighbourNode.gCost) {
          neighbourNode.cameFromNodeIndex = currentNodeIndex;
          neighbourNode.gCost = tentativeGCost;
          neighbourNode.CalculateFCost();
          pathNodeArray[neighbourNodeIndex] = neighbourNode;
          if (!openList.Contains(neighbourNode.index)) {
            openList.Add(neighbourNode.index);
          }
        }
      }
    }
    PathNode endNode = pathNodeArray[endNodeIndex];
    if (endNode.cameFromNodeIndex != -1) {
      // Found a path
      this.path = this.CalculatePath(pathNodeArray, endNode);
    }
    pathNodeArray.Dispose();
    neighbourOffsetArrayUp.Dispose();
    neighbourOffsetArrayDown.Dispose();
    openList.Dispose();
    closedList.Dispose();
  }

  private NativeList<int2> CalculatePath(NativeArray<PathNode> pathNodeArray, PathNode endNode) {
    if (endNode.cameFromNodeIndex == -1) {
      // Couldn't find a path!
      return new NativeList<int2>(Allocator.Temp);
    } else {
      // Found a path
      this.path.Add(new int2(endNode.x, endNode.y));
      PathNode currentNode = endNode;
      while (currentNode.cameFromNodeIndex != -1) {
        PathNode cameFromNode = pathNodeArray[currentNode.cameFromNodeIndex];
        this.path.Add(new int2(cameFromNode.x, cameFromNode.y));
        currentNode = cameFromNode;
      }
      return this.path;
    }
  }

  private readonly bool IsPositionInsideGrid(int2 gridPosition, int2 gridSize) {
    return gridPosition.x >= 0 &&
           gridPosition.y >= 0 &&
           gridPosition.x < gridSize.x &&
           gridPosition.y < gridSize.y;
  }

  private readonly int GetLowestCostFNodeIndex(NativeList<int> openList, NativeArray<PathNode> pathNodeArray) {
    PathNode lowestCostPathNode = pathNodeArray[openList[0]];
    for (int i = 1; i < openList.Length; i++) {
      PathNode testPathNode = pathNodeArray[openList[i]];
      if (testPathNode.fCost < lowestCostPathNode.fCost) {
        lowestCostPathNode = testPathNode;
      }
    }
    return lowestCostPathNode.index;
  }

  private readonly int CalculateIndex(int x, int y, int gridWidth) {
    return x + (y * gridWidth);
  }

  private readonly float CalculateDistanceCost(int2 aPosition, int2 bPosition) {
    int xPositions = aPosition.x - bPosition.x;
    int yPositions = aPosition.y - bPosition.y;
    float xDistance = xPositions * this.triangleSide / 2;
    float yDistance = yPositions % 2 == 0
      ? yPositions * this.triangleHeight
      : aPosition.y % 2 == 0
        ? yPositions * this.triangleHeight * 4 / 3
        : yPositions * this.triangleHeight * 2 / 3;
    return (float)Math.Sqrt((xDistance * xDistance) + (yDistance * yDistance));
  }

}

public struct PathNode {

  public int x;
  public int y;
  public int index;
  public float gCost;
  public float hCost;
  public float fCost;
  public bool isWalkable;
  public int cameFromNodeIndex;

  public void CalculateFCost() {
    this.fCost = this.gCost + this.hCost;
  }

  public void SetIsWalkable(bool isWalkable) {
    this.isWalkable = isWalkable;
  }

}

public class Pathfinding : MonoBehaviour {



}
