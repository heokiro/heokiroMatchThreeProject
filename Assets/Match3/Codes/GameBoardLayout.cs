using UnityEngine;

namespace Match3.Codes
{
    [System.Serializable]
    public class GameBoardLayout
    {
        [System.Serializable]
        public struct GameBoardRow
        {
            public bool[] row;
        }

        public Grid grid;
        public GameBoardRow[] rows = new GameBoardRow[10];
    }
}
