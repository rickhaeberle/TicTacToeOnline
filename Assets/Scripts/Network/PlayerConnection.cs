using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerConnection : NetworkBehaviour
{
    private float _playerId;

    void Start()
    {
        DontDestroyOnLoad(gameObject);

        if (isLocalPlayer)
        {
            _playerId = Random.Range(1f, 10000000f);

            if (isServer)
            {
                GameManager.Instance.CreateServer();

            }

            GameManager.Instance.OnQuitMatchEvent += TellServerQuitMatch;

            TellServerIAmConnected();
        }
    }

    private void OnDestroy()
    {
        if (BoardController.Instance != null)
        {
            BoardController.Instance.OnPlayEvent -= TellServerIHavePlayed;
            BoardController.Instance.OnReadyEvent -= TellServerIAmReady;

        }

        GameManager.Instance.OnQuitMatchEvent -= TellServerQuitMatch;

    }

    public void TellServerIAmConnected()
    {
        CmdRegisterPlayer(_playerId);
    }

    public void TellServerIAmReady()
    {
        CmdMarkPlayerAsReaady(_playerId);
    }

    public void TellServerIHavePlayed(int position)
    {
        CmdPlayedAt(_playerId, position);
    }

    public void TellServerQuitMatch()
    {
        CmdQuitMatch();
    }

    public void TellClientsToStartGame(float firstPlayerId)
    {
        RpcStartGame(firstPlayerId);
    }

    public void TellClientsToPlayAt(int position)
    {
        RpcPlayAt(position);
    }

    public void TellClientsQuitMatch()
    {
        if (isLocalPlayer)
        {
            StartCoroutine(TellClientsEndGameRoutine());
        }
        else
        {
            RpcQuitMatch();
        }
    }

    // Delay antes de matar o servidor?
    private IEnumerator TellClientsEndGameRoutine()
    {
        yield return new WaitForSeconds(0.8f);
        RpcQuitMatch();
    }

    // COMMAND
    [Command]
    private void CmdRegisterPlayer(float playerId)
    {
        GameManager.Instance.Server.RegisterPlayer(playerId, this);
    }

    [Command]
    private void CmdMarkPlayerAsReaady(float playerId)
    {
        GameManager.Instance.Server.MarkPlayerAsReady(playerId);
    }

    [Command]
    private void CmdPlayedAt(float playerId, int position)
    {
        GameManager.Instance.Server.PlayedAt(playerId, position);
    }

    [Command]
    private void CmdQuitMatch()
    {
        GameManager.Instance.Server.QuitMatch();
    }

    // RPC
    [ClientRpc]
    private void RpcStartGame(float firstPlayerId)
    {
        if (!isLocalPlayer)
            return;

        BoardController.Instance.OnPlayEvent += TellServerIHavePlayed;
        BoardController.Instance.OnReadyEvent += TellServerIAmReady;

        BoardController.Instance.CreateGame(EGameMode.Online, firstPlayerId == _playerId ? ESide.First : ESide.Last, null);
    }

    [ClientRpc]
    private void RpcPlayAt(int position)
    {
        if (!isLocalPlayer)
            return;

        BoardController.Instance.PlayAt(position, true);
    }

    [ClientRpc]
    private void RpcQuitMatch()
    {
        if (!isLocalPlayer)
            return;

        MatchMakerManager.Disconnect();

        GameManager.Instance.GoToMenu();
    }
}
