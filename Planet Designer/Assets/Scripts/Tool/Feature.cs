using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public abstract class Feature : MonoBehaviour
{
    [SerializeField] private bool enableBrush;
    [SerializeField] protected bool selectable;
    [SerializeField] protected bool selected;
    [SerializeField] protected Object inspectObject;

    public bool EnableBrush => enableBrush;
    public bool Selectable => selectable;
    public bool Selected { get { return selected; } set { selected = value; } }

    private void OnMouseOver()
    {
        if (Input.GetMouseButton(0))
            Select();
    }

    [ContextMenu("Select")]
    public void Select()
    {
        FeatureManager.Instance.Select(this);
    }

    public abstract void Regenerate();

    public abstract void WhileSelected();

    public abstract bool CheckClickedOn();

    public virtual void OnSelected()
    {
        Debug.Log("Feature selected: " + gameObject.name);
        EditorMenu.Instance.SetHelpText(gameObject.name + " Selected");

        #if UNITY_EDITOR
        Selection.activeObject = inspectObject;
        #endif
    }

    public virtual void OnDeselected()
    {
        Debug.Log("Feature deselected: " + gameObject.name);
        EditorMenu.Instance.SetHelpText("");

        #if UNITY_EDITOR
        Selection.activeObject = null;
        #endif
    }

}
