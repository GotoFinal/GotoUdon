using System;
using System.Reflection;
using GotoUdon.Utils;
using UnityEngine;
using VRC.Core;
using VRCSDK2;

namespace GotoUdon.VRC.SDK
{
    public class PublishAutomation : MonoBehaviour
    {
        private bool _clicked;
        private int _delay;
        private FieldInfo _cachedField;

        private void Update()
        {
#if UNITY_EDITOR
            if (_clicked || APIUser.CurrentUser == null) return;
            try
            {
                RuntimeWorldCreation worldCreation = FindObjectOfType<RuntimeWorldCreation>();
                if (worldCreation == null || string.IsNullOrWhiteSpace(worldCreation.blueprintName.text)) return;
                PipelineManager pipelineManager = worldCreation.pipelineManager;
                if (!pipelineManager.completedSDKPipeline || !APIUser.Exists(pipelineManager.user)) return;
                _clicked = true;
                worldCreation.uploadButton.onClick.Invoke();
            }
            catch (Exception e)
            {
                _clicked = true;
                GotoLog.Exception("Failed automated publish", e);
            }
#endif
        }
    }
}