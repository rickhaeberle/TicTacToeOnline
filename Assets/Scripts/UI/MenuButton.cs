using UnityEngine;
using UnityEngine.EventSystems;

public class MenuButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{

    public AudioClip MouseOverSfx;
    public AudioClip MouseClickSfx;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (MouseOverSfx == null)
            return;

        SFXPlayer.Instance.Play(MouseOverSfx, 0.25f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (MouseClickSfx == null)
            return;

        SFXPlayer.Instance.Play(MouseClickSfx);
    }
}
