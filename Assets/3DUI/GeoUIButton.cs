using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(BoxCollider))]
public class GeoUIButton : GeoUIPanel, IGeoButton
{
    //this could be on base
    private readonly static int HoverAdd = Shader.PropertyToID("_HoverAdd");

    [ReadOnly]
    [DisableContextMenu]
    public BoxCollider m_collider = null;

    //public MeshRenderer OutlineRenderer = null;

    //Interface
    public UnityEvent SetSelected => m_setSelected;
    public UnityEvent SetHovered => m_setHovered;
    public UnityEvent SetUnhovered => m_setUnhovered;

    //Fulfil interface with private member
    [SerializeField]
    private UnityEvent m_setSelected;

    [SerializeField]
    private UnityEvent m_setHovered;

    [SerializeField]
    private UnityEvent m_setUnhovered;

    [SerializeField]
    private float m_hoverSizeAdd = 0;

    [SerializeField]
    private Color m_hoverColor = Color.white;

    //Tweening
    [ReadOnly]
    private int m_hoverSizeTween = -99;

    [SerializeField]
    private float m_hoverSizeTweenDuration = 0.3f;

    [SerializeField]
    private LeanTweenType m_onHoverEase = LeanTweenType.easeInBounce;

    [SerializeField]
    private LeanTweenType m_onUnhoverEase = LeanTweenType.easeInBounce;

    protected override void OnEnable()
    {
        base.OnEnable();

        if (m_collider == null)
        {
            m_collider = GetComponent<BoxCollider>();
        }

        // OutlineRenderer.gameObject.SetActive(false);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        // DestroyImmediate(OutlineRenderer.material);
    }

    protected override void OnValidate()
    {
        base.OnValidate();
        m_collider.size = new Vector3(m_width * 2f + 1f, 1f, m_breadth * 2f + 1f);
    }

    protected override void InstantiateMaterial(bool forceInstantiate)
    {
        base.InstantiateMaterial(forceInstantiate);

        //Account for the additional new outline renderer
        // OutlineRenderer.material = new Material(m_materialInstance);
    }

    public void OnSetHovered()
    {
        LeanTween.cancel(m_hoverSizeTween);


        Color currentColor = m_materialInstance.GetColor("_ColorA");
        float currentAdd = m_materialInstance.GetFloat(HoverAdd);

        m_hoverSizeTween = LeanTween.value(currentAdd, m_hoverSizeAdd, m_hoverSizeTweenDuration).setEase(m_onHoverEase).setOnUpdate((float val) => {
            m_materialInstance.SetFloat(HoverAdd, val);
            m_materialInstance.SetColor("_ColorA", Color.Lerp(currentColor, m_hoverColor, (val - currentAdd) / (m_hoverSizeAdd - currentAdd)));
        }).setOnComplete(() => { }).id;
    }

    public void OnSetUnhovered()
    {
        LeanTween.cancel(m_hoverSizeTween);

        RefreshMaterialProperties(forceInstantiate: true);

        float currentAdd = m_materialInstance.GetFloat(HoverAdd);

        m_hoverSizeTween = LeanTween.value(currentAdd, 0f, m_hoverSizeTweenDuration).setEase(m_onUnhoverEase).setOnUpdate((float val) => {
            m_materialInstance.SetFloat(HoverAdd, val);
        }).setOnComplete(() => {
            // OutlineRenderer.gameObject.SetActive(false);
            // Destroy(OutlineRenderer.material);
        }).id;
    }
}
