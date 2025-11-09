using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

[ExecuteAlways]
public class GeoUIElement : MonoBehaviour
{
    //Basically initialise this as a UI element.

    //The base GEOUI Resizeable material
    [SerializeField]
    private Material m_materialBase = null;
    protected Material m_materialInstance;

    [ReadOnly]
    [DisableContextMenu]
    public MeshRenderer Renderer = null;

    public bool IsSphere = false;

    [SerializeField]
    protected float m_width;

    [SerializeField]
    [DisableIf("IsSphere")]
    protected float m_breadth;

    [SerializeField]
    [DisableIf("IsSphere")]
    protected float m_height;


    protected virtual void OnEnable()
    {
        if (Renderer == null)
        {
            Renderer = GetComponent<MeshRenderer>();
        }
        InstantiateMaterial();
    }

    protected virtual void InstantiateMaterial(bool forceInstantiante = false)
    {
        if (m_materialInstance == null || forceInstantiante)
        {
            //Set a new trnasient material for the renderer
            m_materialInstance = new Material(m_materialBase);

            if (Renderer != null)
            {
                Renderer.material = m_materialInstance;
            }
        }
    }

    protected virtual void OnDestroy()
    {
        DestroyImmediate(m_materialInstance);
    }

    protected virtual void OnValidate()
    {
        RefreshMaterialProperties();
    }

    [Button]
    protected virtual void RefreshMaterialProperties(bool forceInstantiate = false)
    {
        InstantiateMaterial(forceInstantiate);

        m_materialInstance.SetFloat("_Width", m_width);
        m_materialInstance.SetFloat("_Breadth", m_breadth);
        m_materialInstance.SetFloat("_Height", m_height);
    }
}
