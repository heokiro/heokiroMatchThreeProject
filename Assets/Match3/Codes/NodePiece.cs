using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Match3.Codes
{
  public class NodePiece : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
  {
    public int value;
    public Point Index;

    [HideInInspector] public Vector2 pos;
    [HideInInspector] public NodePiece flipped;
    [HideInInspector] public RectTransform rect;

    private bool _isUpdating;
    private Image _image;

    public void Initialize(int v, Point p, Sprite piece)
    {
      flipped = null;
      _image = GetComponent<Image>();
      rect = GetComponent<RectTransform>();

      value = v;
      SetIndex(p);
      _image.sprite = piece;
    }

    public void SetIndex(Point p)
    {
      Index = p;
      ResetPosition();
      UpdateName();
    }
    public void ResetPosition()
    {
      pos = new Vector2(128 * Index.X, -128 * Index.Y);
    }

    public bool UpdatePiece()
    {
      if (Vector3.Distance(rect.anchoredPosition, pos) > 1)
      {
        MovePositionTo(pos);
        _isUpdating = true;
        return true;
      }
      else
      {
        rect.anchoredPosition = pos;
        _isUpdating = false;
        return false;
      }
    }
  
    public void MovePosition(Vector2 move)
    {
      rect.anchoredPosition += move * Time.deltaTime * 32f;
    }

    public void MovePositionTo(Vector2 move)
    {
      rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, move, Time.deltaTime * 32f);
    }

    void UpdateName()
    {
      transform.name = "Node [" + Index.X + ", " + Index.Y + "]";
    }

    public void OnPointerDown(PointerEventData eventData)
    {
      if (_isUpdating) return;
      MovePieces.Instance.StartMovingPiece(this);
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
     MovePieces.Instance.StopMovingPiece();
    }
  }
}
