using System;
using System.Collections.Generic;
using UnityEngine;

namespace Arcanism.CharacterUI
{
    /* NOT a camera for your face. Put your duck lips away. */
    class FaceCamera : MonoBehaviour
    {

        protected void Update()
        {
            // Duplicated code from the "NamePlate" component, but don't want to be adding that to random things just for this isolated camera facing code
            if (GameData.PlayerControl.FPV.gameObject.activeSelf)
                transform.LookAt(GameData.CamControl.FPV.transform.position);
            else if (!GameData.PlayerControl.DroneMode)
                transform.LookAt(GameData.GameCamPos.position);
            else
                transform.LookAt(GameData.PlayerControl.DroneCam.transform.position);
        }
    }
}
