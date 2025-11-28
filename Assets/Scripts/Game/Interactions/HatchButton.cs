using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HatchButton : Interactable {

    protected override void ShowInteractMessage () {
        Ship ship = FindObjectOfType<Ship> ();
        string action = (ship.HatchOpen) ? "cerrar" : "abrir";
        string contextualMessage = $"Presiona F para {action} escotilla";
        GameUI.DisplayInteractionInfo (contextualMessage);
    }

    protected override void Interact () {
        base.Interact ();
        ShowInteractMessage ();
    }

    void OnValidate () {
        interactMessage = "#set from script#";
    }
}