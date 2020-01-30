using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardController : MonoBehaviour
{
    public static BoardController Instance;

    public List<Button> Buttons;

    public GameObject EndGamePanel;
    public Text EndGameText;
    public GameObject InfoPanel;
    public Text TurnText;
    public Button RematchButton;
    public Button BackToMenuButton;

    public GameObject AladdinPiece;
    public GameObject AbuPiece;
    public GameObject GeniePiece;

    public List<AudioClip> PlaySounds;
    public AudioClip GameOverClip;

    public event Action<int> OnPlayEvent;
    public event Action OnReadyEvent;

    private Game _game;
    private bool _gameOver;

    private Dictionary<int, GameObject> _pieces;

    private bool _amIP1;
    private Player _player1;

    private EGameMode _gameMode;
    private ESide _side;
    private EDifficulty? _difficulty;

    private void Awake()
    {
        Instance = this;

        _pieces = new Dictionary<int, GameObject>();

        for (int i = 0; i < Buttons.Count; i++)
        {
            int idx = i;
            Buttons[i].onClick.AddListener(() => OnButtonClicked(idx));
            Buttons[i].interactable = false;
        }

        RematchButton.onClick.AddListener(() => Rematch());
        BackToMenuButton.onClick.AddListener(() => BackToMenu());

        EndGamePanel.SetActive(false);
    }

    public void CreateGame(EGameMode gameMode, ESide side, EDifficulty? difficulty)
    {
        PrepareGame();

        _game = new Game();
        _game.CreateGame(gameMode);

        _game.RegisterHumanPlayer(1);

        if (gameMode == EGameMode.VsAI)
        {
            _game.RegisterAIPlayer(2, difficulty.Value);

        }
        else
        {
            _game.RegisterHumanPlayer(2);

        }

        if (side == ESide.Random)
            side = UnityEngine.Random.value < 0.5f ? ESide.First : ESide.Last;

        _gameMode = gameMode;
        _side = side;
        _difficulty = difficulty;

        float firstPlayerId = side == ESide.First ? 1 : 2;

        _game.StartGame(firstPlayerId);

        _amIP1 = gameMode == EGameMode.Online ? side == ESide.First : gameMode == EGameMode.VsAI ? !_game.ItsAITurn() : true;
        _player1 = _game.GetCurrentPlayer();

        UpdateTurnText(_amIP1);
        SetButtonsInteractable(_amIP1);

        CheckAITurn();
    }

    public void PlayAt(int position, bool isServer)
    {
        if (_gameOver)
            return;

        bool canPlayAt = _game.CanPlayAt(position);
        if (!canPlayAt)
            return;

        _game.PlayAt(position);

        AudioClip clip = PlaySounds[UnityEngine.Random.Range(0, PlaySounds.Count)];
        SFXPlayer.Instance.Play(clip);

        Player player = _game.GetCurrentPlayer();
        bool itsMyTurn = _gameMode != EGameMode.VsHuman ? _amIP1 ? player == _player1 : player != _player1 : true;

        SetButtonsInteractable(itsMyTurn);
        UpdateTurnText(itsMyTurn);

        UpdateBoard();

        CheckEndGame();

        CheckAITurn();

        if (!isServer)
        {
            OnPlayEvent?.Invoke(position);
        }
    }

    private void CheckAITurn()
    {
        if (_gameOver)
            return;

        if (_game.ItsAITurn())
        {

            SetButtonsInteractable(false);

            StartCoroutine(MakeAIPlayRoutine());
        }
    }

    private IEnumerator MakeAIPlayRoutine()
    {
        yield return new WaitForSeconds(0.65f);

        int position = _game.MakeAIPlay();
        PlayAt(position, false);
    }

    private void UpdateTurnText(bool myTurn)
    {
        TurnText.text = _game.GetTurn(_player1.Id, myTurn);

    }

    private void PrepareGame()
    {
        ClearPieces();

        InfoPanel.SetActive(true);

        EndGamePanel.SetActive(false);
        RematchButton.interactable = true;
        RematchButton.GetComponentInChildren<Text>().text = "PLAY AGAIN";

        _gameOver = false;
    }

    private void OnButtonClicked(int position)
    {
        PlayAt(position, false);
    }

    private void UpdateBoard()
    {
        Board board = _game.GetBoard();

        for (int i = 0; i < Buttons.Count; i++)
        {
            char marker = board.GetMarkerAt(i);
            if (marker == default)
                continue;

            if (_pieces.ContainsKey(i))
                continue;

            Button btn = Buttons[i];

            GameObject piece = null;

            bool isVsAI = _gameMode == EGameMode.VsAI;
            if (isVsAI)
            {
                bool isP1AI = _player1 is AIPlayer;

                if (Constants.Marker.X == marker)
                {
                    piece = isP1AI ? GeniePiece : AladdinPiece;

                }
                else
                {
                    piece = isP1AI ? AladdinPiece : GeniePiece;

                }

            }
            else
            {
                piece = Constants.Marker.X == marker ? AladdinPiece : AbuPiece;

            }

            GameObject go = Instantiate(piece, btn.transform);
            _pieces[i] = go;
        }
    }

    private void Rematch()
    {
        OnReadyEvent?.Invoke();

        if (_gameMode == EGameMode.Online)
        {
            RematchButton.interactable = false;
            RematchButton.GetComponentInChildren<Text>().text = "Waiting...";

        }
        else
        {
            ESide side = _side == ESide.First ? ESide.Last : ESide.First;
            CreateGame(_gameMode, side, _difficulty);

        }
    }

    private void BackToMenu()
    {
        if (_gameMode == EGameMode.Online)
            GameManager.Instance.QuitOnlineMatch();

        GameManager.Instance.GoToMenu();
    }

    private void CheckEndGame()
    {
        float? winnerId = _game.GetWinner();
        if (winnerId != null)
        {

            string winText = string.Empty;

            if (_gameMode == EGameMode.VsAI)
            {
                bool isP1AI = _player1 is AIPlayer;

                if (isP1AI)
                {
                    winText = winnerId == _player1.Id ? "YOU LOSE" : "YOU WON";

                }
                else
                {
                    winText = winnerId == _player1.Id ? "YOU WON" : "YOU LOSE";

                }

            }
            else if (_gameMode == EGameMode.VsHuman)
            {
                winText = winnerId == _player1.Id ? "P1 WON" : "P2 WON";

            }
            else if (_gameMode == EGameMode.Online)
            {
                bool amIP1 = _side == ESide.First;
                if (amIP1)
                {
                    winText = winnerId == _player1.Id ? "YOU WON" : "YOU LOSE";

                }
                else
                {
                    winText = winnerId == _player1.Id ? "YOU LOSE" : "YOU WON";
                }
            }

            GameOver(winText);
        }
        else
        {
            bool itsDraw = _game.ItsADraw();
            if (itsDraw)
            {
                GameOver("DRAW");

            }

        }
    }

    private void GameOver(string message)
    {
        _gameOver = true;

        SetButtonsInteractable(false);

        InfoPanel.SetActive(false);

        if (GameOverClip != null)
            SFXPlayer.Instance.Play(GameOverClip);

        FadeNonWinningPieces();

        StartCoroutine(ShowResultsRoutine(message));
    }

    private void FadeNonWinningPieces()
    {
        float? winnerId = _game.GetWinner();
        if (winnerId != null)
        {

            List<int> winningPositions = _game.GetWinningPositions();
            if (winningPositions != null)
            {

                foreach (int position in _pieces.Keys)
                {
                    if (!winningPositions.Contains(position))
                    {
                        GameObject go = _pieces[position];

                        Image img = go.GetComponent<Image>();
                        Color color = img.color;
                        color.a = 0.2f;
                        img.color = color;
                    }
                }
            }
        }
    }

    private IEnumerator ShowResultsRoutine(string message)
    {
        yield return new WaitForSeconds(0.5f);

        EndGamePanel.SetActive(true);
        EndGameText.text = message;
    }

    private void SetButtonsInteractable(bool interactable)
    {
        for (int i = 0; i < Buttons.Count; i++)
        {
            bool empty = _game.CanPlayAt(i);
            if (empty)
            {
                Buttons[i].interactable = interactable;
            }
            else
            {
                Buttons[i].interactable = false;
            }
        }
    }

    private void ClearPieces()
    {
        foreach (GameObject piece in _pieces.Values)
        {
            Destroy(piece.gameObject);
        }

        _pieces = new Dictionary<int, GameObject>();
    }
}
