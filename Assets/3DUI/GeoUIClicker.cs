using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
public class GeoUIClicker : GeoUIElement, IGeoButton
{
    //this could be on base
    private readonly static int HoverAdd = Shader.PropertyToID("_HoverAdd");

    [ReadOnly]
    [DisableContextMenu]
    public SphereCollider m_collider = null;

    public ParticleSystem m_hoverSystem = null;

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

    protected override void OnValidate()
    {
        base.OnValidate();
        m_collider.radius = m_width + 1 * 0.5f;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if (m_collider == null)
        {
            m_collider = GetComponent<SphereCollider>();
        }

        // OutlineRenderer.gameObject.SetActive(false);
    }

    public void OnSetHovered()
    {
        LeanTween.cancel(m_hoverSizeTween);

        m_hoverSystem.gameObject.SetActive(true);

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

        m_hoverSystem.gameObject.SetActive(false);

        RefreshMaterialProperties(forceInstantiate: true);

        float currentAdd = m_materialInstance.GetFloat(HoverAdd);

        m_hoverSizeTween = LeanTween.value(currentAdd, 0f, m_hoverSizeTweenDuration).setEase(m_onUnhoverEase).setOnUpdate((float val) => {
            m_materialInstance.SetFloat(HoverAdd, val);
        }).setOnComplete(() => { }).id;
    }
}
