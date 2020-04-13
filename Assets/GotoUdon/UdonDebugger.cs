using System;
using UnityEngine;
using VRC.Udon;

/**
 * Each object that uses udon will have this component added when in play mode
 */
[RequireComponent(typeof(UdonBehaviour))]
public class UdonDebugger : MonoBehaviour
{
    private bool clickToInteract = true;
    public UdonBehaviour[] behaviours;

    void Start()
    {
        behaviours = GetComponents<UdonBehaviour>();
    }

    void OnMouseUp()
    {
        if (clickToInteract)
            ForAll(behaviour => behaviour.Interact());
    }


    private void ForAll(Action<UdonBehaviour> action)
    {
        foreach (UdonBehaviour behaviour in behaviours)
        {
            action(behaviour);
        }
    }
}