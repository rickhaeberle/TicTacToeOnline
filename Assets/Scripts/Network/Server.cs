using System.Collections.Generic;
using System.Linq;

public class Server
{

    private List<PlayerNetworkWrapper> _players;

    private float _p1Id;
    private Game _game;

    public Server()
    {
        _players = new List<PlayerNetworkWrapper>();
    }

    public void RegisterPlayer(float playerId, PlayerConnection playerConn)
    {
        PlayerNetworkWrapper playerWrapper = new PlayerNetworkWrapper()
        {
            Id = playerId,
            PlayerConnection = playerConn
        };

        _players.Add(playerWrapper);

        if (_players.Count == Constants.NUMBER_OF_PLAYERS)
        {
            GoToGameScene();
        }
    }

    public void MarkPlayerAsReady(float playerId)
    {
        PlayerNetworkWrapper player = _players.Find(p => p.Id == playerId);
        if (player != null)
        {
            player.IsReady = true;
        }

        if (!_players.Any(p => !p.IsReady))
        {
            StartGame();
        }
    }

    private void GoToGameScene()
    {
        MatchMakerManager.StopBroadcasting();
        MatchMakerManager.singleton.ServerChangeScene(Constants.Scene.GAME);
    }

    private void StartGame()
    {

        if (_game == null)
        {
            _game = new Game();
            _p1Id = _players.Min(player => player.Id);

        }
        else
        {
            _p1Id = _players.Find(player => player.Id != _p1Id).Id;

        }

        _game.CreateGame(EGameMode.VsHuman);
        _players.ForEach(player => _game.RegisterHumanPlayer(player.Id));
        _game.StartGame(_p1Id);

        _players.ForEach(player => player.PlayerConnection.TellClientsToStartGame(_p1Id));
    }

    public void PlayedAt(float playerId, int position)
    {
        Player current = _game.GetCurrentPlayer();
        if (current.Id != playerId)
            return;

        bool played = _game.PlayAt(position);
        if (played)
        {
            _players.ForEach(player => player.PlayerConnection.TellClientsToPlayAt(position));

        }

        float? winnerId = _game.GetWinner();
        bool draw = _game.ItsADraw();

        if (winnerId != null || draw)
        {
            _players.ForEach(player => player.IsReady = false);

        }

    }

    public void QuitMatch()
    {
        _players.ForEach(player => player.PlayerConnection.TellClientsQuitMatch());
    }
}
