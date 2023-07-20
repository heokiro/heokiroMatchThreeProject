using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Match3.Codes
{
    public class MatchedPiece : MonoBehaviour
    {
        // 조각이 떨어지는 상태인지를 나타내는 변수
        public bool isFalling;

        // 물리 시뮬레이션에 사용되는 속도와 중력
        private readonly float _movementSpeed = 16f;
        private readonly float _gravityForce = 32f;
        private Vector2 _movementDirection;
        private RectTransform _pieceRectTransform;
        private Image _pieceImage;

        public void Initialize(Sprite pieceSprite, Vector2 startPosition)
        {
            isFalling = true;

            // 위쪽으로 움직이는 이동 방향을 초기화하고 무작위 x 성분을 추가
            _movementDirection = Vector2.up;
            _movementDirection.x = Random.Range(-1.0f, 1.0f);
            _movementDirection *= _movementSpeed / 2;

            _pieceImage = GetComponent<Image>();
            _pieceRectTransform = GetComponent<RectTransform>();
            _pieceImage.sprite = pieceSprite;
            _pieceRectTransform.anchoredPosition = startPosition;
        }

        void Update()
        {
            if (!isFalling) return;
            // 조각에 중력을 적용
            _movementDirection.y -= Time.deltaTime * _gravityForce;
            _movementDirection.x = Mathf.Lerp(_movementDirection.x, 0, Time.deltaTime);
            _pieceRectTransform.anchoredPosition += _movementDirection * (Time.deltaTime * _movementSpeed);

            // 조각이 화면 밖으로 나가면, 조각의 위치 업데이트를 멈춤
            if (_pieceRectTransform.position.x < 64f || _pieceRectTransform.position.x > Screen.width + 64f ||
                _pieceRectTransform.position.y < -64f || _pieceRectTransform.position.y > Screen.height + 64f)
                isFalling = false;
        }
    }
}