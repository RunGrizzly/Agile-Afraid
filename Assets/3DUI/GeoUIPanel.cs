using TMPro;
using UnityEngine;
using UnityEngine.Events;
public class GeoUIPanel : GeoUIElement
{
    [SerializeField]
    private TextMeshProUGUI m_panelTextBox = null;

    protected override void OnValidate()
    {
        //Do the element resize
        base.OnValidate();

        //Resize the text box
        m_panelTextBox.canvas.GetComponent<RectTransform>().sizeDelta = new Vector2(m_width * 2 + 1, m_breadth * 2 + 1);
    }
}
