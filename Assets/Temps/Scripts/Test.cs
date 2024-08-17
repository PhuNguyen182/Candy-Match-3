using System;
using System.Collections.Generic;

public class Match3Grid
{
    private int[,] grid;
    private int rows;
    private int cols;
    private List<int> colors = new List<int> { 0, 1, 2, 3 }; // Representing 4 colors

    public Match3Grid(int rows, int cols)
    {
        this.rows = rows;
        this.cols = cols;
        grid = new int[rows, cols];
        FillGrid();
    }

    private void FillGrid()
    {
        Random rand = new Random();
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                List<int> availableColors = new List<int>(colors);

                // Remove colors that would violate the constraints
                if (row >= 2 && grid[row - 1, col] == grid[row - 2, col])
                {
                    availableColors.Remove(grid[row - 1, col]);
                }
                if (col >= 2 && grid[row, col - 1] == grid[row, col - 2])
                {
                    availableColors.Remove(grid[row, col - 1]);
                }

                // Randomly select a color from the available options
                grid[row, col] = availableColors[rand.Next(availableColors.Count)];
            }
        }

        // Ensure at least one matchable set exists
        EnsureMatchableSet();
    }

    private void EnsureMatchableSet()
    {
        // For simplicity, place a matchable set at a random position
        Random rand = new Random();
        int matchRow = rand.Next(rows - 2);
        int matchCol = rand.Next(cols - 2);
        int matchColor = colors[rand.Next(colors.Count)];

        grid[matchRow, matchCol] = matchColor;
        grid[matchRow + 1, matchCol] = matchColor;
        grid[matchRow + 2, matchCol] = matchColor;
    }

    public void PrintGrid()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                Console.Write(grid[row, col] + " ");
            }
            Console.WriteLine();
        }
    }
}

public class Program
{
    public static void Main()
    {
        Match3Grid match3Grid = new Match3Grid(8, 8);
        match3Grid.PrintGrid();
    }
}