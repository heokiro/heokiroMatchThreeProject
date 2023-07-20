using UnityEngine;

namespace Match3.Codes
{
    public class MovePieces : MonoBehaviour
    {
        public static MovePieces Instance;
        private GameManager _gameManager;
        
        private NodePiece _pieceBeingMoved;
        private Point _destIndex;
        private Vector2 _mouseStartPos;
        
        private void Awake()
        {
            Instance = this;
        }
        
        void Start()
        {
            _gameManager = GetComponent<GameManager>();
        }
        
             private void Update()
        {
            // 만약 이동 중인 피스가 있다면
            if (_pieceBeingMoved != null)
            {
                Vector2 dir = (Vector2)Input.mousePosition - _mouseStartPos;
                Vector2 nDir = dir.normalized;
                Vector2 aDir = new Vector2(Mathf.Abs(dir.x), Mathf.Abs(dir.y));
        
                _destIndex = Point.ClonePoint(_pieceBeingMoved.Index);
                Point add = Point.Zero;
                // 마우스의 이동 거리가 일정 거리 이상이면
                if (dir.magnitude > 64)
                {
                    // 마우스의 이동 방향에 따라 피스의 이동 방향을 결정
                    if (aDir.x > aDir.y)
                    {
                        add = new Point((nDir.x > 0) ? 1 : -1, 0);
                    }
                    else if (aDir.y > aDir.x)
                    {
                        add = (new Point(0, (nDir.y > 0) ? -1 : 1));
                    }
                }
                _destIndex.AddPoint(add);
            
                Vector2 pos = _gameManager.GetPositionFromPoint(_pieceBeingMoved.Index);
                // 만약 목표 위치가 피스의 초기 위치와 다르다면, 
                // 피스의 위치를 목표 위치에 맞게 갱신
                if (!_destIndex.IsEquals(_pieceBeingMoved.Index))
                    pos += Point.MultiPoint(new Point(add.X, -add.Y), 32).ToVector();
                _pieceBeingMoved.MovePositionTo(pos);
            }
        }
        
        // 피스 이동 시작 메서드
        public void StartMovingPiece(NodePiece piece)
        {
            // 이미 이동중인 피스가 있다면 리턴
            if (_pieceBeingMoved != null) return;
            // 이동을 시작할 피스를 설정
            _pieceBeingMoved = piece;
            // 마우스의 시작 위치를 저장
            _mouseStartPos = Input.mousePosition;
        }
        
        // 피스 이동 종료 메서드
        public void StopMovingPiece()
        {
            // 이동중인 피스가 없다면 리턴
            if (_pieceBeingMoved == null) return;
        
            // 목표 위치가 피스의 초기 위치와 다르다면, 
            // 피스의 위치를 갱신하고 게임 매니저에게 피스 위치 갱신 알림
            if (!_destIndex.IsEquals(_pieceBeingMoved.Index))
            {
                _gameManager.FlipPieces(_pieceBeingMoved.Index, _destIndex, true);
            }
            else
            {
                _gameManager.ResetPiece(_pieceBeingMoved);
            }
        
            // 피스 이동 완료 후, 이동중인 피스를 null로 설정
            _gameManager.ResetPiece(_pieceBeingMoved);
            _pieceBeingMoved = null;
        } 
    }
}
