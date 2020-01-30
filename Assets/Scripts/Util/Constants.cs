public abstract class Constants
{

    public const int NUMBER_OF_PLAYERS = 2;

    public abstract class Marker
    {
        public const char X = 'X';
        public const char O = 'O';
    }

    public abstract class Board
    {
        public const int SIZE = 9;
    }

    public abstract class Scene
    {
        public const string MENU = "Menu";
        public const string GAME = "Game";
    }
}
