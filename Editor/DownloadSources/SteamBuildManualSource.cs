﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public class SteamBuildManualSource : ASteamBuildSource
    {
        private string m_finalSourcePath;
        private string m_enteredFilePath;
        private string m_unzipDirectory;

        public SteamBuildManualSource(SteamBuildWindow steamBuildWindow)
        {
        }

        public override void OnGUIExpanded()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("File Path:", GUILayout.Width(120));
                string newPath = GUILayout.TextField(m_enteredFilePath);

                if (GUILayout.Button("...", GUILayout.Width(50), GUILayout.MaxWidth(500)))
                {
                    newPath = EditorUtility.OpenFilePanel("Build Folder", "", "");
                }

                if (GUILayout.Button("Show", GUILayout.Width(50)))
                {
                    EditorUtility.RevealInFinder(m_enteredFilePath);
                }

                if (newPath != m_enteredFilePath && !string.IsNullOrEmpty(newPath))
                {
                    m_enteredFilePath = newPath;
                }
            }
        }

        public override void OnGUICollapsed()
        {
            if (GUILayout.Button(m_enteredFilePath, GUILayout.Width(50), GUILayout.MaxWidth(500)))
            {
                string newPath = EditorUtility.OpenFilePanel("Build Folder", "", "");
                if (newPath != m_enteredFilePath && !string.IsNullOrEmpty(newPath))
                {
                    m_enteredFilePath = newPath;
                }
            }
        }

        public override IEnumerator GetSource()
        {
            // Decide where we want to download to
            m_progressDescription = "Preparing...";
            m_downloadProgress = 0;
            string directoryPath = Application.persistentDataPath + "/ManualBuilds";
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Make copy to avoid sharing conflicts
            string copyPath = directoryPath + "/" + Path.GetFileName(m_enteredFilePath);
            if (!File.Exists(copyPath))
            {
                File.Copy(m_enteredFilePath, copyPath);
            }

            // NOTE:
            // Originally this tool would allow uploading folders and would zip them but that required a different package.
            // Then it was made to unzip and rezip in order to proceed which is stupid
            // SO only accept .zip since that's what steam needs

            if (Path.GetExtension(copyPath) == ".zip")
            {
                // Given zipped file. Unzip it
                m_unzipDirectory = copyPath;
                Debug.Log("copyPath " + copyPath);
            }
            else
            {
                throw new Exception(string.Format("Unable to parse extension: {0}. Give .zip!",
                    Path.GetExtension(copyPath)));
            }

            m_finalSourcePath = m_unzipDirectory;
            Debug.Log("Retrieved Build: " + m_finalSourcePath);

            m_progressDescription = "Done!";
            yield return null;
        }

        public override string SourceFilePath()
        {
            return m_finalSourcePath;
        }

        public override float DownloadProgress()
        {
            return m_downloadProgress;
        }

        public override string ProgressTitle()
        {
            return "Getting Local Source";
        }

        public override string ProgressDescription()
        {
            return m_progressDescription;
        }

        public override bool IsSetup()
        {
            return !string.IsNullOrEmpty(m_enteredFilePath);
        }

        public override string GetBuildDescription()
        {
            // Windows #44 Release
            string description = "";

            string fileName = Path.GetFileNameWithoutExtension(m_enteredFilePath);
            if (fileName.Contains("windows"))
            {
                description += "Windows ";
            }
            else if (fileName.Contains("windows"))
            {
                description += "Mac ";
            }

            if (fileName.LastIndexOf("-") > 0)
            {
                if (int.TryParse(fileName.Substring(fileName.LastIndexOf("-") + 1), out int i))
                {
                    description += "#" + i + " ";
                }
            }

            if (fileName.Contains("development"))
            {
                description += "Dev";
            }
            else
            {
                description += "Release";
            }

            return description;
        }

        public override Dictionary<string, object> Serialize()
        {
            return new Dictionary<string, object>();
        }

        public override void Deserialize(Dictionary<string, object> data)
        {

        }
    }
}