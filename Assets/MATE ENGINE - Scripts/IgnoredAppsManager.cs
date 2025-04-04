﻿using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NAudio.CoreAudioApi;

public class AllowedAppsManager : MonoBehaviour
{
    public TMP_Dropdown runningAppsDropdown;
    public Button addToAllowedListButton;
    public Transform allowedAppsListContent;
    public GameObject allowedAppItemPrefab;

    private MMDeviceEnumerator enumerator;
    private MMDevice defaultDevice;

    private List<string> currentRunningAppNames = new List<string>();
    private List<string> allowedApps => FindFirstAvatar()?.allowedApps;

    void Start()
    {
        enumerator = new MMDeviceEnumerator();
        UpdateDefaultDevice();

        addToAllowedListButton.onClick.AddListener(() =>
        {
            if (runningAppsDropdown.options.Count == 0) return;

            string selectedApp = runningAppsDropdown.options[runningAppsDropdown.value].text;
            if (!allowedApps.Contains(selectedApp))
            {
                allowedApps.Add(selectedApp);
                UpdateAllowedListUI();
            }
        });

        RefreshRunningAppsDropdown();
        UpdateAllowedListUI();
    }

    void UpdateDefaultDevice()
    {
        defaultDevice?.Dispose();
        defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
    }

    void RefreshRunningAppsDropdown()
    {
        currentRunningAppNames = GetRunningAudioAppNames();
        runningAppsDropdown.ClearOptions();
        runningAppsDropdown.AddOptions(
            currentRunningAppNames.Select(app => new TMP_Dropdown.OptionData(app)).ToList()
        );
    }

    void UpdateAllowedListUI()
    {
        foreach (Transform child in allowedAppsListContent)
            Destroy(child.gameObject);

        foreach (var app in allowedApps)
        {
            var item = Instantiate(allowedAppItemPrefab, allowedAppsListContent);

            var label = item.GetComponentsInChildren<TextMeshProUGUI>()
                            .FirstOrDefault(t => t.transform.parent == item.transform);
            if (label != null) label.text = app;

            var button = item.transform.Find("Button")?.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() =>
                {
                    allowedApps.Remove(app);
                    UpdateAllowedListUI();
                });
            }
        }
    }

    List<string> GetRunningAudioAppNames()
    {
        var appNames = new HashSet<string>();
        try
        {
            var sessions = defaultDevice.AudioSessionManager.Sessions;
            for (int i = 0; i < sessions.Count; i++)
            {
                var session = sessions[i];
                int processId = (int)session.GetProcessID;
                if (processId == 0) continue;

                try
                {
                    var process = Process.GetProcessById(processId);
                    string name = process.ProcessName.ToLowerInvariant();
                    appNames.Add(name);
                }
                catch { continue; }
            }
        }
        catch { }

        return appNames.OrderBy(n => n).ToList();
    }

    AvatarAnimatorController FindFirstAvatar()
    {
        return FindObjectOfType<AvatarAnimatorController>();
    }

    void OnDestroy()
    {
        enumerator?.Dispose();
        defaultDevice?.Dispose();
    }

    public void RefreshAppListOnMenuOpen()
    {
        RefreshRunningAppsDropdown();
        UpdateAllowedListUI();
    }


    public void RefreshUI()
    {
        UpdateDefaultDevice();               // Make sure device is current
        RefreshRunningAppsDropdown();       // Refresh dropdown entries
        UpdateAllowedListUI();              // Refresh the visible list
    }


}


