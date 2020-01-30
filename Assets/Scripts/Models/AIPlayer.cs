using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIPlayer : Player
{
    public EDifficulty Difficulty { get; set; }

    private struct Move
    {
        public int Position { get; set; }
        public int Score { get; set; }
    }

    public override int Play(Board board)
    {

        float randomMoveChance = Difficulty == EDifficulty.Easy ? 0.4f : Difficulty == EDifficulty.Medium ? 0.2f : 0.075f;

        if (board.GetEmptyPositions().Count == 9)
            randomMoveChance = 1f;

        if (Random.value < randomMoveChance)
        {
            List<int> emptyPositions = board.GetEmptyPositions();
            return emptyPositions[Random.Range(0, emptyPositions.Count)];
        }

        Move move = Minimax(board);
        return move.Position;
    }

    private char GetOpponentMarker()
    {
        return Marker == Constants.Marker.X ? Constants.Marker.O : Constants.Marker.X;
    }

    private Move Minimax(Board board, bool max = true)
    {
        if (board.IsWinner(Marker))
        {
            return new Move { Score = 1 };

        }
        else if (board.IsWinner(GetOpponentMarker()))
        {
            return new Move { Score = -1 };

        }
        else if (!board.HasEmptyPositions())
        {
            return new Move { Score = 0 };

        }

        char currentMarker = max ? Marker : GetOpponentMarker();

        List<Move> moves = new List<Move>();

        List<int> emptyPositions = board.GetEmptyPositions();
        foreach (int position in emptyPositions)
        {
            Board newBoard = board.Clone();
            newBoard.PlayAt(position, currentMarker);

            Move nextMove = Minimax(newBoard, !max);
            nextMove.Position = position;
            moves.Add(nextMove);
        }

        if (max)
        {
            moves = moves.OrderByDescending(move => move.Score).ToList();

        }
        else
        {
            moves = moves.OrderBy(move => move.Score).ToList();

        }

        List<Move> bestMoves = moves.FindAll(move => move.Score == moves[0].Score);
        return bestMoves[Random.Range(0, bestMoves.Count)];
    }

}
