using System.Collections.Generic;

public class Game
{

    private Board _board;

    private List<Player> _players;

    private Player _current;

    private EGameMode? _gameMode;

    public void CreateGame(EGameMode gameMode)
    {
        _gameMode = gameMode;

        _board = new Board();
        _players = new List<Player>();
    }

    public void RegisterHumanPlayer(float playerId)
    {
        Player player = CreatePlayer(playerId, true);
        _players.Add(player);
    }

    public void RegisterAIPlayer(float playerId, EDifficulty difficulty)
    {
        Player player = CreatePlayer(playerId, false, difficulty);
        _players.Add(player);
    }

    public void StartGame(float firstPlayerId)
    {
        Player p1 = _players.Find(player => player.Id == firstPlayerId);
        p1.Marker = Constants.Marker.X;

        Player p2 = _players.Find(player => player.Id != firstPlayerId);
        p2.Marker = Constants.Marker.O;

        _current = p1;
    }

    public bool ItsAITurn()
    {
        return _current is AIPlayer;
    }

    public Board GetBoard()
    {
        return _board.Clone();
    }

    public int MakeAIPlay()
    {
        AIPlayer ai = (AIPlayer)_current;
        return ai.Play(_board);
    }

    public bool PlayAt(int position)
    {
        if (!_board.CanPlayAt(position))
            return false;

        _board.PlayAt(position, _current.Marker);

        ChangeTurn();

        return true;
    }

    public bool CanPlayAt(int position)
    {
        return _board.CanPlayAt(position);
    }

    public float? GetWinner()
    {
        foreach (Player player in _players)
        {
            bool playerWon = _board.IsWinner(player.Marker);
            if (playerWon)
                return player.Id;
        }

        return null;
    }

    public string GetTurn(float playerId, bool myTurn)
    {
        if (_gameMode == EGameMode.Online)
        {
            return myTurn ? "YOUR TURN" : "OPPONENT TURN";

        }
        else if (_gameMode == EGameMode.VsAI)
        {
            return ItsAITurn() ? "AI TURN" : "YOUR TURN";

        }
        else if (_gameMode == EGameMode.VsHuman)
        {
            return playerId == _current.Id ? "P1 TURN" : "P2 TURN";

        }

        return string.Empty;
    }

    public Player GetCurrentPlayer()
    {
        return _current;
    }

    public List<int> GetWinningPositions()
    {
        return _board.GetWinningPositions();
    }

    public bool ItsADraw()
    {
        return !_board.HasEmptyPositions();
    }

    private Player CreatePlayer(float playerId, bool human, EDifficulty? difficulty = null)
    {
        if (human)
            return new HumanPlayer() { Id = playerId };

        return new AIPlayer() { Id = playerId, Difficulty = difficulty.Value };
    }

    private void ChangeTurn()
    {
        _current = _players.Find(player => player.Id != _current.Id);
    }
}
