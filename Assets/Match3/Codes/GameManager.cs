using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace Match3.Codes
{
    public class GameManager : MonoBehaviour
    {
        public GameBoardLayout boardLayout;
    
        [Header("UI Elements")]
        public Sprite[] pieces;
        public RectTransform gameBoard;
        public RectTransform matchedBoard;

        [Header("Prefabs")] 
        public GameObject nodePiece;
        public GameObject matchedPiece;
    
        private int _width;
        private int _height;
        private int[] _pieceFillCounts;
        private Node[,] _board;

        private List<NodePiece> _piecesToUpdate;
        private List<FlippedPieces> _flippedPieces;
        private List<NodePiece> _deletedPieces;
        private List<MatchedPiece> _matchedPieces;

        private Random _random;


        private void Start()
        {
            StartGame();
        }
    
        private void Update()
        {
            // 업데이트가 완료된 피스 리스트
            List<NodePiece> finishedUpdating = new List<NodePiece>();
            foreach (var piece in _piecesToUpdate)
            {
                if (!piece.UpdatePiece()) finishedUpdating.Add(piece);
            }

            foreach (var piece in finishedUpdating)
            {
                // 스왑된 피스 정보 가져오기
                FlippedPieces flip = GetFlipped(piece);
                NodePiece flippedPiece = null;
            
                int x = piece.Index.X;
                _pieceFillCounts[x] = Mathf.Clamp(_pieceFillCounts[x] - 1, 0, _width);
            
                // 연결된 피스들의 리스트
                List<Point> connectedPieces = IsConnected(piece.Index, true);
                bool wasFlipped = (flip != null);

                // 피스가 스왑되었다면, 스왑된 피스와 연결된 피스들을 연결된 피스들의 리스트에 추가
                if (wasFlipped)
                {
                    flippedPiece = flip.GetOtherPiece(piece);
                    AddPoints(ref connectedPieces, IsConnected(flippedPiece.Index, true));
                
                }
                
                // 연결된 피스가 없다면, 피스를 원래 위치로 되돌리기
                if (connectedPieces.Count == 0)
                {
                    if (wasFlipped)
                        FlipPieces(piece.Index, flippedPiece.Index, false);
                }
                else
                {
                    // 연결된 피스들을 매치 처리하고, 빈 공간에 새로운 피스를 채우기
                    foreach (var pnt in connectedPieces)
                    {
                        MatchPiece(pnt);
                        Node node = GetNodeAtPoint(pnt);
                        NodePiece np = node.GetPiece();
                        if (np != null)
                        {
                            np.gameObject.SetActive(false);
                            _deletedPieces.Add(np);
                        }
                        node.SetPiece(null);
                    }

                    ApplyGravityToBoard();
                }
                
                // 스왑 정보와 업데이트 리스트에서 해당 피스 정보 제거
                _flippedPieces.Remove(flip);
                _piecesToUpdate.Remove(piece);
            }
        }

        // 보드에 중력 적용 메서드
        void ApplyGravityToBoard()
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = (_height - 1); y >= 0; y--)
                {
                    Point p = new Point(x, y);
                    Node node = GetNodeAtPoint(p);
                    int val = GetValueAtPoint(p);
                    if (val != 0) continue;

                    // 위쪽에서 아래로 빈 공간에 새로운 피스를 채우기
                    for (int ny = (y - 1); ny >= -1; ny--)
                    {
                        Point next = new Point(x, ny);
                        int nextVal = GetValueAtPoint(next);
                        if (nextVal == 0) continue;
                        if (nextVal != -1)
                        {
                            // 위쪽의 피스를 현재 빈 공간으로 이동
                            Node nextNode = GetNodeAtPoint(next);
                            NodePiece nextPiece = nextNode.GetPiece();
                        
                            // 빈 공간 채우기
                            node.SetPiece(nextPiece);
                            _piecesToUpdate.Add(nextPiece);
                        
                            nextNode.SetPiece(null);
                        }
                        else
                        {
                            // 새로운 피스 생성
                            int newVal = FillPiece();
                            NodePiece piece;
                            Point fallPnt = new Point(x, -1 - _pieceFillCounts[x]);
                            if (_deletedPieces.Count > 0)
                            {
                                // 삭제 된 피스 재사용
                                NodePiece revived = _deletedPieces[0];
                                revived.gameObject.SetActive(true);
                                piece = revived;
                                _deletedPieces.RemoveAt(0);
                            }
                            else
                            {
                                // 새로운 피스 생성
                                GameObject obj = Instantiate(nodePiece, gameBoard);
                                NodePiece n = obj.GetComponent<NodePiece>();
                                piece = n;
                            }
                            // 새로운 피스 초기화
                            piece.Initialize(newVal, p, pieces[newVal - 1]);
                            piece.rect.anchoredPosition = GetPositionFromPoint(fallPnt);

                            // 빈 공간에 새로운 피스 세팅
                            Node hole = GetNodeAtPoint(p);
                            hole.SetPiece(piece);
                            ResetPiece(piece);
                            _pieceFillCounts[x]++;
                        }

                        break;
                    }
                }
            }
        }

        // 스왑된 피스 정보 가져오는 메서드

        FlippedPieces GetFlipped(NodePiece p)
        {
            FlippedPieces flip = null;
            foreach (var t in _flippedPieces)
            {
                if (t.GetOtherPiece(p) != null)
                {
                    flip = t;
                }
            }

            return flip;
        }

        void StartGame()
        {
            _width = 8;
            _height = 10;
            _pieceFillCounts = new int[_width];
            string seed = GetRandomSeed();
            _random = new Random(seed.GetHashCode());
            _piecesToUpdate = new List<NodePiece>();
            _flippedPieces = new List<FlippedPieces>();
            _deletedPieces = new List<NodePiece>();
            _matchedPieces = new List<MatchedPiece>();
        
            InitializedBoard();
            VerifyBoard();
            InstantiateBoard();
        }

        void InitializedBoard()
        {
            _board = new Node[_width, _height];
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    _board[x, y] = new Node((boardLayout.rows[y].row[x]) ? -1 : FillPiece(), new Point(x, y));
                }
            }
        }

        void VerifyBoard()
        {
            // 보드에 3개 이상 연결된 피스가 없도록 조정
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    Point p = new Point(x, y);
                    int val = GetValueAtPoint(p);
                    if (val <= 0) continue;

                    // 제거할 피스의 값들
                    var remove = new List<int>();
                    while (IsConnected(p, true).Count > 0)
                    {
                        val = GetValueAtPoint(p);
                        if(!remove.Contains(val))
                            remove.Add(val);
                    
                        SetValueAtPoint(p, NewValue(ref remove));
                    }
                }
            }
        }

        void InstantiateBoard()
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    Node node = GetNodeAtPoint(new Point(x, y));
                
                    int val = _board[x, y].value;
                    if (val <= 0) continue;
                    GameObject p = Instantiate(nodePiece, gameBoard);
                    NodePiece piece = p.GetComponent<NodePiece>();
                    RectTransform rect = p.GetComponent<RectTransform>();
                    rect.anchoredPosition = new Vector2(128 * x, -128 * y);
                    piece.Initialize(val, new Point(x, y), pieces[val - 1]);
                    node.SetPiece(piece);
                }
            }
        }

        public void ResetPiece(NodePiece piece)
        {
            piece.ResetPosition();
            piece.flipped = null;
            _piecesToUpdate.Add(piece);
        
        }

        public void FlipPieces(Point one, Point two, bool main)
        {
            if (GetValueAtPoint(one) < 0) return;
        
            Node nodeOne = GetNodeAtPoint(one);
            NodePiece pieceOne = nodeOne.GetPiece();
            if (GetValueAtPoint(two) > 0)
            {
                Node nodeTwo = GetNodeAtPoint(two);
                NodePiece pieceTwo = nodeTwo.GetPiece();
                nodeOne.SetPiece(pieceTwo);
                nodeTwo.SetPiece(pieceOne);
            
                if (main)
                    _flippedPieces.Add(new FlippedPieces(pieceOne, pieceTwo));

                pieceOne.flipped = pieceTwo;
                pieceTwo.flipped = pieceOne;
            
                _piecesToUpdate.Add(pieceOne);
                _piecesToUpdate.Add(pieceTwo);
            }
            else
            {
                ResetPiece(pieceOne);
            }
        }

        void MatchPiece(Point p)
        {
            // 매치 처리할 피스 리스트
            List<MatchedPiece> availablePieces = new List<MatchedPiece>();
            foreach (var t in _matchedPieces)
            {
                if (!t.isFalling) availablePieces.Add(t);
            }

            MatchedPiece set;
            if (availablePieces.Count > 0)
                set = availablePieces[0];
            else
            {
                // 매치 처리할 피스 생성
                GameObject match = GameObject.Instantiate(matchedPiece, matchedBoard);
                MatchedPiece mPiece = match.GetComponent<MatchedPiece>();
                set = mPiece;
                _matchedPieces.Add(mPiece);
            }
            
            // 매치 처리할 피스 초기화
            int val = GetValueAtPoint(p) - 1;
            if (set != null && val >= 0 && val < pieces.Length)
                set.Initialize(pieces[val], GetPositionFromPoint(p));
        }

        List<Point> IsConnected(Point p, bool main)
        {
            // 연결된 피스의 좌표 리스트
            List<Point> connected = new List<Point>();
            int val = GetValueAtPoint(p);
            Point[] directions =
            {
                Point.Up,
                Point.Right,
                Point.Down,
                Point.Left
            };
            
            // 각 방향으로 연결된 피스 체크
            foreach (var dir in directions)
            {
                List<Point> line = new List<Point>();

                int same = 0;
                for (int i = 1; i < 3; i++)
                {
                    Point check = Point.AddPoint(p, Point.MultiPoint(dir, i));
                    if (GetValueAtPoint(check) == val)
                    {
                        line.Add(check);
                        same++;
                    }
                }

                // 같은 피스가 2개 이상이면 연결된 피스에 추가
                if (same > 1)
                {
                    AddPoints(ref connected, line);
                }
            }

            // 현재 피스가 중간에 있는 경우 체크
            for (int i = 0; i < 2; i++)
            {
                List<Point> line = new List<Point>();

                int same = 0;
                Point[] check = { Point.AddPoint(p, directions[i]), Point.AddPoint(p, directions[i + 2])};
                foreach (var next in check)
                {
                    if (GetValueAtPoint(next) == val)
                    {
                        line.Add(next);
                        same++;
                    }
                }

                if (same > 1)
                {
                    AddPoints(ref connected, line);
                }
            }

            // 2x2 체크
            for (int i = 0; i < 4; i++)
            {
                List<Point> square = new List<Point>();

                int same = 0;
                int next = i + 1;
                if (next >= 4)
                {
                    next -= 4;
                }

                Point[] check =
                {
                    Point.AddPoint(p, directions[i]), Point.AddPoint(p, directions[next]),
                    Point.AddPoint(p, Point.AddPoint(directions[i], directions[next]))
                };
                foreach (var point in check)
                {
                    if (GetValueAtPoint(point) == val)
                    {
                        square.Add(point);
                        same++;
                    }
                }

                if (same > 2)
                {
                    AddPoints(ref connected, square);
                }
            }

            // 연결된 피스가 2개 이상이면 재귀적으로 체크
            if (main)
            {
                for (int i = 0; i < connected.Count; i++)
                {
                    AddPoints(ref connected, IsConnected(connected[i], false));
                }
            }

            return connected;
        }

        // 연결된 피스 추가 메서드
        void AddPoints(ref List<Point> points, List<Point> add)
        {
            foreach (Point p in add)
            {
                bool b = true;

                foreach (var t in points)
                {
                    if (t.IsEquals(p))
                    {
                        b = false;
                        break;
                    }
                }
            
                if (b) points.Add(p);
            }
        }

        int FillPiece()
        {
            var val = _random.Next(0, 100) / (100 / pieces.Length) + 1;
            return val;
        }

        int GetValueAtPoint(Point p)
        {
            if (p.X < 0 || p.X >= _width || p.Y < 0 || p.Y >= _height) return -1;
            return _board[p.X, p.Y].value;
        }

        void SetValueAtPoint(Point p, int v)
        {
            _board[p.X, p.Y].value = v;
        }

        Node GetNodeAtPoint(Point p)
        {
            return _board[p.X, p.Y];
        }

        int NewValue(ref List<int> remove)
        {
            List<int> available = new List<int>();
            for(int i = 0; i < pieces.Length; i++)
                available.Add(i + 1);

            foreach (var i in remove)
                available.Remove(i);

            if (available.Count <= 0) return 0;
        
            return available[_random.Next(0, available.Count)];
        }

        string GetRandomSeed()
        {
            // Guid(글로벌 고유 식별자)를 생성하여 문자열로 변환
            string seed = System.Guid.NewGuid().ToString();
            return seed;
        }


        public Vector2 GetPositionFromPoint(Point p)
        {
            return new Vector2(128 * p.X, -128 * p.Y);
        }
    }

    [System.Serializable]
    public class Node
    {
        public int value; // 0 = blank, 1 = stone_blue, 2 = green, 3 = pink, 4 = yellow, -1 = hole
        public Point Index;
        NodePiece _piece;

        public Node(int v, Point i)
        {
            value = v;
            Index = i;
        }

        public void SetPiece(NodePiece p)
        {
            _piece = p;
            value = _piece == null ? 0 : _piece.value;
            if (_piece == null) return;
            _piece.SetIndex((Index));
        }

        public NodePiece GetPiece()
        {
            return _piece;
        }
    }

    [System.Serializable]
    public class FlippedPieces
    {
        public NodePiece one;
        public NodePiece two;

        public FlippedPieces(NodePiece o, NodePiece t)
        {
            one = o;
            two = t;
        }

        public NodePiece GetOtherPiece(NodePiece p)
        {
            if (p == one)
                return two;
            else if (p == two)
                return one;
            else
                return null;
        }
    }
}