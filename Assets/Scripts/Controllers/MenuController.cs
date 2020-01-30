using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.Types;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public GameObject HomePanel;
    public GameObject InternetPanel;
    public GameObject SearchingPanel;
    public GameObject LanPanel;
    public GameObject WaitingPanel;
    public GameObject GameModePanel;
    public GameObject DifficultyPanel;
    public GameObject SidePanel;
    public GameObject CreditsPanel;

    public GameObject ConnectingPanel;

    public InputField InternetRoomNameInput;
    public GameObject AvailableInternetRoomsPanel;

    public InputField LanRoomNameInput;
    public GameObject AvailableLanRoomsPanel;

    public Button JoinRoomButtonPrefab;

    public float TimeToUpdateMatchList = 2f;

    private float _timeToUpdateMatchListTimer;

    private bool _searchingInternetMatch = false;
    private bool _searchingLanMatch = false;

    private EGameMode? _gameMode;
    private EDifficulty? _difficulty;
    private ESide? _side;

    private void Awake()
    {
        _gameMode = null;
        _difficulty = null;
        _side = null;

        ConnectingPanel.SetActive(false);

        GoToHomeMenu();
    }

    private void Update()
    {
        if (_searchingInternetMatch)
        {
            _timeToUpdateMatchListTimer += Time.deltaTime;
            if (_timeToUpdateMatchListTimer >= TimeToUpdateMatchList)
            {
                _timeToUpdateMatchListTimer = 0;

                MatchMakerManager.SearchRoomOnline();
                UpdateInternetMathesPanel();
            }
        }

        if (_searchingLanMatch)
        {
            _timeToUpdateMatchListTimer += Time.deltaTime;
            if (_timeToUpdateMatchListTimer >= TimeToUpdateMatchList)
            {
                _timeToUpdateMatchListTimer = 0;
                UpdateLanMathesPanel();
            }
        }
    }

    private void GoToPanel(GameObject panel)
    {
        HomePanel.SetActive(HomePanel == panel);
        InternetPanel.SetActive(InternetPanel == panel);
        SearchingPanel.SetActive(SearchingPanel == panel);
        LanPanel.SetActive(LanPanel == panel);
        WaitingPanel.SetActive(WaitingPanel == panel);
        GameModePanel.SetActive(GameModePanel == panel);
        DifficultyPanel.SetActive(DifficultyPanel == panel);
        SidePanel.SetActive(SidePanel == panel);
        CreditsPanel.SetActive(CreditsPanel == panel);
    }

    private void UpdateInternetMathesPanel()
    {
        foreach (Transform child in AvailableInternetRoomsPanel.transform)
        {
            Destroy(child.gameObject);
        }

        List<Room> rooms = MatchMakerManager.GetOnlineRooms();
        foreach (Room room in rooms)
        {
            Button button = Instantiate(JoinRoomButtonPrefab, AvailableInternetRoomsPanel.transform, false);
            button.GetComponentInChildren<Text>().text = room.Name;
            button.onClick.AddListener(() =>
            {
                ConnectingPanel.SetActive(true);

                button.interactable = false;
                OnInternetJoinRoomClick(room.NetworkID);
            });
        }
    }

    private void UpdateLanMathesPanel()
    {
        foreach (Transform child in AvailableLanRoomsPanel.transform)
        {
            Destroy(child.gameObject);
        }

        List<Room> rooms = MatchMakerManager.GetLocalRooms();
        foreach (Room room in rooms)
        {
            Button button = Instantiate(JoinRoomButtonPrefab, AvailableLanRoomsPanel.transform, false);
            button.GetComponentInChildren<Text>().text = $"{room.Name} ({room.Address})";
            button.onClick.AddListener(() =>
            {
                ConnectingPanel.SetActive(true);

                button.interactable = false;
                OnLanJoinRoomClick(room.Address);
            });
        }
    }

    public void GoToHomeMenu()
    {
        GoToPanel(HomePanel);

        ConnectingPanel.SetActive(false);

        LanRoomNameInput.text = string.Empty;
        InternetRoomNameInput.text = string.Empty;
    }

    #region Home
    public void OnHomeInternetButtonClick()
    {
        GoToPanel(InternetPanel);

        _searchingInternetMatch = true;
        _timeToUpdateMatchListTimer = TimeToUpdateMatchList;

        //MatchMakerManager.SearchRoomOnline();
        UpdateInternetMathesPanel();
    }

    public void OnHomeLanButtonClick()
    {
        GoToPanel(LanPanel);

        MatchMakerManager.StartSearchRoomLocal();

        _searchingLanMatch = true;
        _timeToUpdateMatchListTimer = TimeToUpdateMatchList;

        UpdateLanMathesPanel();
    }

    public void OnHomePracticeButtonClick()
    {
        GoToPanel(GameModePanel);
    }

    public void OnHomeCreditsButtonClick()
    {
        GoToPanel(CreditsPanel);
    }

    public void OnHomeQuitButtonClick()
    {
        Application.Quit();
    }
    #endregion

    #region Internet
    public void OnInternetCreateMatchButtonClick()
    {
        _searchingInternetMatch = false;

        string name = InternetRoomNameInput.text;
        if (string.IsNullOrEmpty(name))
            name = "Match " + Random.Range(100000, 1000000);

        MatchMakerManager.CreateRoomOnline(name);

        GoToPanel(SearchingPanel);
    }

    public void OnInternetBackButtonClick()
    {
        _searchingInternetMatch = false;

        MatchMakerManager.SearchRoomOnline();

        GoToPanel(HomePanel);
    }

    private void OnInternetJoinRoomClick(NetworkID networkID)
    {
        _searchingInternetMatch = false;
        MatchMakerManager.StartOnlineMatch(networkID);
    }
    #endregion

    #region Searching
    public void OnSearchingCancelButtonClick()
    {
        GoToPanel(HomePanel);

        MatchMakerManager.CloseRoomOnline();

    }
    #endregion

    #region Lan
    public void OnLanCreateRoomButtonClick()
    {
        _searchingLanMatch = false;

        string name = LanRoomNameInput.text;
        if (string.IsNullOrEmpty(name))
            name = "Match " + Random.Range(100000, 1000000);

        MatchMakerManager.StopSearchRoomLocal();
        MatchMakerManager.CreateRoomLocal(name);

        GoToPanel(WaitingPanel);
    }

    public void OnLanCancelButtonClick()
    {
        _searchingLanMatch = false;
        MatchMakerManager.StopSearchRoomLocal();

        GoToPanel(HomePanel);
    }

    private void OnLanJoinRoomClick(string address)
    {
        _searchingLanMatch = false;
        MatchMakerManager.StartLocalMatch(address);
    }
    #endregion

    #region Waiting
    public void OnWaitingCancelButtonClick()
    {
        GoToPanel(LanPanel);

        MatchMakerManager.CloseRoomLocal();
        MatchMakerManager.StartSearchRoomLocal();

        _searchingLanMatch = true;
    }
    #endregion

    #region GameMode
    public void OnGameModeVsHumanClick()
    {
        _gameMode = EGameMode.VsHuman;
        StartOfflineGame();
    }

    public void OnGameModeVsIAClick()
    {
        _gameMode = EGameMode.VsAI;
        GoToPanel(DifficultyPanel);
    }

    public void OnGameModeBackButtonClick()
    {
        _gameMode = null;
        GoToPanel(HomePanel);
    }
    #endregion

    #region Difficulty
    public void OnDifficultyEasyButtonClick()
    {
        _difficulty = EDifficulty.Easy;
        GoToPanel(SidePanel);
    }

    public void OnDifficultyMediumButtonClick()
    {
        _difficulty = EDifficulty.Medium;
        GoToPanel(SidePanel);
    }

    public void OnDifficultyHardButtonClick()
    {
        _difficulty = EDifficulty.Hard;
        GoToPanel(SidePanel);
    }

    public void OnDifficultyBackButtonClick()
    {
        _difficulty = null;
        GoToPanel(GameModePanel);
    }
    #endregion

    #region Side
    public void OnSidePlayFirstButtonClick()
    {
        _side = ESide.First;
        StartOfflineGame();
    }

    public void OnSidePlayLastButtonClick()
    {
        _side = ESide.Last;
        StartOfflineGame();
    }

    public void OnSideRandomButtonClick()
    {
        _side = ESide.Random;
        StartOfflineGame();
    }

    public void OnSideBackButtonClick()
    {
        _side = null;
        GoToPanel(DifficultyPanel);
    }
    #endregion

    #region Credits
    public void OnCreditsBackButtonClick()
    {
        GoToPanel(HomePanel);
    }
    #endregion

    private void StartOfflineGame()
    {
        GameManager.Instance.StartOfflineGame(_gameMode, _difficulty, _side);
    }
}
