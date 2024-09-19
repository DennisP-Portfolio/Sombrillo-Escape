using System;
using System.Collections.Generic;

[Flags]
public enum EWallState
{
    // 0000 = No walls
    // 1111 = All walls
    Left = 1, // 0001
    Right = 2, // 0010
    Up = 4, // 0100
    Down = 8, // 1000

    Visited = 128, // 1000 0000
}

public struct Position
{
    public int X;
    public int Y;
}

public struct Neighbour
{
    public Position Position;
    public EWallState SharedWall;
}

public static class MazeGenerator
{
    private static EWallState GetOppositeWall(EWallState wall)
    {
        switch (wall)
        {
            case EWallState.Right:
                return EWallState.Left;
            case EWallState.Left:
                return EWallState.Right;
            case EWallState.Up:
                return EWallState.Down;
            case EWallState.Down:
                return EWallState.Up;
            default:
                return EWallState.Left;
        }
    }

    private static EWallState[,] ApplyRecursiveBacktracker(EWallState[,] maze, int width, int height)
    {
        // Create a random number generator for selecting the starting position
        var rng = new System.Random();

        // Create a stack to keep track of visited positions
        var positionStack = new Stack<Position>();

        // Choose a random starting position within the maze and mark it as visited
        var position = new Position { X = rng.Next(0, width), Y = rng.Next(0, height) };
        maze[position.X, position.Y] |= EWallState.Visited;

        // Add the starting position to the stack
        positionStack.Push(position);

        // Keep visiting positions until all have been visited
        while (positionStack.Count > 0)
        {
            // Pop the most recently added position from the stack
            var current = positionStack.Pop();

            // Get the unvisited neighbors of the current position
            var neighbours = GetUnvisitedNeighbours(current, maze, width, height);

            // If there are unvisited neighbors, visit one at random
            if (neighbours.Count > 0)
            {
                // Add the current position back to the stack
                positionStack.Push(current);

                // Choose a random unvisited neighbor
                var randomIndex = rng.Next(0, neighbours.Count);
                var randomNeighbour = neighbours[randomIndex];
                var neighbourPos = randomNeighbour.Position;

                // Remove the shared wall between the current position and the chosen neighbor
                maze[current.X, current.Y] &= ~randomNeighbour.SharedWall;
                maze[neighbourPos.X, neighbourPos.Y] &= ~GetOppositeWall(randomNeighbour.SharedWall);

                // Mark the chosen neighbor as visited and add it to the stack
                maze[neighbourPos.X, neighbourPos.Y] |= EWallState.Visited;
                positionStack.Push(neighbourPos);
            }
        }

        return maze;
    }

    private static List<Neighbour> GetUnvisitedNeighbours(Position pos, EWallState[,] maze, int width, int height)
    {
        var list = new List<Neighbour>();

        // Check left neighbour
        if (pos.X > 0)
        {
            if (!maze[pos.X - 1, pos.Y].HasFlag(EWallState.Visited))
            {
                list.Add(new Neighbour
                {
                    Position = new Position
                    {
                        X = pos.X - 1,
                        Y = pos.Y,
                    },
                    SharedWall = EWallState.Left
                }); ;
            }
        }

        // Check down neighbour
        if (pos.Y > 0)
        {
            if (!maze[pos.X, pos.Y - 1].HasFlag(EWallState.Visited))
            {
                list.Add(new Neighbour
                {
                    Position = new Position
                    {
                        X = pos.X,
                        Y = pos.Y - 1,
                    },
                    SharedWall = EWallState.Down
                }); ;
            }
        }

        // Check up neighbour
        if (pos.Y < height - 1)
        {
            if (!maze[pos.X, pos.Y + 1].HasFlag(EWallState.Visited))
            {
                list.Add(new Neighbour
                {
                    Position = new Position
                    {
                        X = pos.X,
                        Y = pos.Y + 1,
                    },
                    SharedWall = EWallState.Up
                }); ;
            }
        }

        // Check right neighbour
        if (pos.X < width - 1)
        {
            if (!maze[pos.X + 1, pos.Y].HasFlag(EWallState.Visited))
            {
                list.Add(new Neighbour
                {
                    Position = new Position
                    {
                        X = pos.X + 1,
                        Y = pos.Y,
                    },
                    SharedWall = EWallState.Right
                }); ;
            }
        }

        return list;
    }

    public static EWallState[,] Generate(int width, int height)
    {
        // create a new 2D array to represent the maze
        EWallState[,] maze = new EWallState[width, height];

        // set the initial state of all cells to have walls on all sides
        EWallState initial = EWallState.Right | EWallState.Left | EWallState.Up | EWallState.Down;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                maze[i, j] = initial;
            }
        }

        // open a path at the top and bottom of the maze
        maze[width / 2, 0] = EWallState.Right | EWallState.Left | EWallState.Up;
        maze[width / 2, height - 1] = EWallState.Right | EWallState.Left | EWallState.Down;

        // apply the recursive backtracking algorithm to generate the maze
        return ApplyRecursiveBacktracker(maze, width, height);
    }
}
