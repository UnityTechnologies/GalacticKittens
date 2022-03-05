using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class OnUIButtonHover : MonoBehaviour
{    
    [SerializeField]
    Sprite m_normalSprite;

    [SerializeField]
    Sprite m_hoverSprite;

    Image buttonImageUI;

    void Start()
    {
        buttonImageUI = GetComponent<Image>();
    }

    public void OnHover(bool isHover)
    {        
        buttonImageUI.sprite = isHover ? m_hoverSprite : m_normalSprite;
    }
}
