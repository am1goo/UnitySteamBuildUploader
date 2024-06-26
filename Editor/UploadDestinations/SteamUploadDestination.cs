﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public class SteamUploadDestination : ASteamBuildDestination
    {
        private bool m_createAppFile = true;
        private bool m_createDepotFile = true;
        private bool m_uploadToSteam = true;

        private SteamBuildConfig m_currentConfig;
        private SteamBuildDepot m_buildDepot;

        private string m_destinationBranch;
        private string m_filePath;
        private bool m_wasBuildSuccessful;


        public SteamUploadDestination(SteamBuildWindow window) : base(window)
        {

        }

        public override void OnGUIExpanded()
        {
            // Config
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Config:", GUILayout.Width(120));
                SteamBuildWindowUtil.ConfigPopup.DrawPopup(ref m_currentConfig);
            }

            // Depot
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Depot:", GUILayout.Width(120));
                SteamBuildWindowUtil.DepotPopup.DrawPopup(m_currentConfig, ref m_buildDepot);
            }

            // Branch
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Branch:", GUILayout.Width(120));
                SteamBuildWindowUtil.BranchPopup.DrawPopup(m_currentConfig, ref m_destinationBranch);
            }

            // Tools
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Create AppFile:", GUILayout.Width(120));
                m_createAppFile = EditorGUILayout.Toggle(m_createAppFile);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Create DepotFile:", GUILayout.Width(120));
                m_createDepotFile = EditorGUILayout.Toggle(m_createDepotFile);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Upload To Steam:", GUILayout.Width(120));
                m_uploadToSteam = EditorGUILayout.Toggle(m_uploadToSteam);
            }
        }

        public override void OnGUICollapsed()
        {
            SteamBuildWindowUtil.ConfigPopup.DrawPopup(ref m_currentConfig);
            SteamBuildWindowUtil.DepotPopup.DrawPopup(m_currentConfig, ref m_buildDepot);
            SteamBuildWindowUtil.BranchPopup.DrawPopup(m_currentConfig, ref m_destinationBranch);
        }

        public override IEnumerator Upload(string filePath, string buildDescription)
        {
            m_filePath = filePath;
            Debug.Log("Uploading " + m_filePath);

            m_wasBuildSuccessful = false;
            m_uploadInProgress = true;

            if (m_createAppFile)
            {
                Debug.Log("Creating new app file");
                m_progressDescription = "Creating App Files";
                m_uploadProgress = 0;
                yield return SteamSDK.Instance.CreateAppFiles(m_currentConfig.App, m_buildDepot.Depot,
                    m_destinationBranch,
                    buildDescription, m_filePath);
            }

            if (m_createDepotFile)
            {
                Debug.Log("Creating new depot file");
                m_progressDescription = "Creating Depot File";
                m_uploadProgress = 0;
                yield return SteamSDK.Instance.CreateDepotFiles(m_buildDepot.Depot);
            }

            if (m_uploadToSteam)
            {
                Debug.Log("Uploading to steam");
                m_progressDescription = "Uploading to Steam";
                m_uploadProgress = 0;
                yield return SteamSDK.Instance.Upload(m_currentConfig.App,
                    successful => m_wasBuildSuccessful = successful);
            }
        }

        public override string ProgressTitle()
        {
            return "Uploading to Steamworks";
        }

        public override bool IsSetup()
        {
            return m_currentConfig != null && m_buildDepot != null && !string.IsNullOrEmpty(m_destinationBranch);
        }

        public override bool WasUploadSuccessful()
        {
            return m_wasBuildSuccessful;
        }

        public override Dictionary<string, object> Serialize()
        {
            Dictionary<string, object> data = new Dictionary<string, object>
            {
                ["m_createAppFile"] = m_createAppFile,
                ["m_createDepotFile"] = m_createDepotFile,
                ["m_uploadToSteam"] = m_uploadToSteam,
                ["m_currentConfig"] = m_currentConfig?.Name,
                ["m_buildDepot"] = m_buildDepot?.Name,
                ["m_destinationBranch"] = m_destinationBranch
            };

            return data;
        }

        public override void Deserialize(Dictionary<string, object> data)
        {
            m_createAppFile = (bool)data["m_createAppFile"];
            m_createDepotFile = (bool)data["m_createDepotFile"];
            m_uploadToSteam = (bool)data["m_uploadToSteam"];
            m_destinationBranch = (string)data["m_destinationBranch"];

            List<SteamBuildConfig> buildConfigs = SteamBuildWindowUtil.ConfigPopup.GetAllData();
            for (int i = 0; i < buildConfigs.Count; i++)
            {
                if (buildConfigs[i].Name == (string)data["m_currentConfig"])
                {
                    m_currentConfig = buildConfigs[i];
                    for (int j = 0; j < m_currentConfig.Depots.Count; j++)
                    {
                        if (m_currentConfig.Depots[j].Name == (string)data["m_buildDepot"])
                        {
                            m_buildDepot = m_currentConfig.Depots[j];
                            break;
                        }
                    }

                    break;
                }
            }
        }
    }
}