using UnityEngine;
using UnityEngine.UI;

public class LiarBarCardSlot : MonoBehaviour
{
    ELiarBarCardType _cardType;
    Image _image;
    Outline _outline;
    bool _isSelected;
    bool _isPlayed;

    public ELiarBarCardType CardType { get => _cardType; }
    public bool IsSelected { get => _isSelected; }
    public bool IsPlayed { get => _isPlayed; }

    void Start()
    {
        _image = GetComponent<Image>();
        _outline = GetComponent<Outline>();
    }

    public void Init(ELiarBarCardType cardType, Sprite sprite)
    {
        if (_image == null)
            _image = GetComponent<Image>();
        if (_outline == null)
            _outline = GetComponent<Outline>();

        _cardType = cardType;
        _image.sprite = sprite;
        _image.color = Color.white;
        _outline.enabled = false;
        _isSelected = false;
        _isPlayed = false;
        _image.gameObject.SetActive(true);
    }

    public void SetSelected(bool selected)
    {
        _isSelected = selected;
        _image.color = selected ? Color.blue : Color.white;
    }

    public void SetPlayed(bool played)
    {
        _isPlayed = played;
        _image.gameObject.SetActive(!played);
        _outline.enabled = false;
        _isSelected = false;
    }

    public void SetOutline(bool onCursor)
    {
        _outline.enabled = onCursor;
    }
}