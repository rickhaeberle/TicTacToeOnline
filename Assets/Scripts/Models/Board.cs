using System.Collections.Generic;
using System.Linq;

/*
0   1   2
3   4   5
6   7   8     
*/

public class Board
{
    private char[] _board;

    private List<int[]> _winningPositions = new List<int[]>() {
        new int[3] { 0, 1, 2 },
        new int[3] { 3, 4, 5 },
        new int[3] { 6, 7, 8 },
        new int[3] { 0, 3, 6 },
        new int[3] { 1, 4, 7 },
        new int[3] { 2, 5, 8 },
        new int[3] { 0, 4, 8 },
        new int[3] { 2, 4, 6 }
    };

    public Board()
    {
        _board = new char[Constants.Board.SIZE];
    }

    public void PlayAt(int position, char marker)
    {
        _board[position] = marker;
    }

    public char GetMarkerAt(int position)
    {
        return _board[position];
    }

    public bool CanPlayAt(int position)
    {
        return _board[position] == default;
    }

    public bool HasEmptyPositions()
    {
        return _board.Any(position => position == default);
    }

    public List<int> GetEmptyPositions()
    {
        List<int> emptyPositions = new List<int>();

        for (int i = 0; i < Constants.Board.SIZE; i++)
        {
            char marker = GetMarkerAt(i);
            if (marker.Equals(default))
            {
                emptyPositions.Add(i);
            }
        }

        return emptyPositions;
    }

    public bool IsWinner(char marker)
    {
        foreach (int[] winningPosition in _winningPositions)
        {
            int position1 = winningPosition[0];
            int position2 = winningPosition[1];
            int position3 = winningPosition[2];

            if (_board[position1] == _board[position2] && _board[position2] == _board[position3] && _board[position3] == marker)
                return true;
        }

        return false;
    }

    public List<int> GetWinningPositions()
    {
        foreach (int[] winningPosition in _winningPositions)
        {
            int position1 = winningPosition[0];
            int position2 = winningPosition[1];
            int position3 = winningPosition[2];

            if (_board[position1] == _board[position2] && _board[position2] == _board[position3])
                return winningPosition.ToList();
        }

        return null;
    }

    public Board Clone()
    {
        return new Board
        {
            _board = (char[])_board.Clone()
        };
    }

}
