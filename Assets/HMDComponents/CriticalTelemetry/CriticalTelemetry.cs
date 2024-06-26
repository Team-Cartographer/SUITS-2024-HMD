using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

public class CriticalTelemetry : MonoBehaviour
{
    public TMP_Text bpmText;
    public TMP_Text battLife;
    public TMP_Text oxyTime;
    public Canvas criticalTelemetryCanvas;

    public ConnectionHandler connection;

    bool updatingTelemetry = false;
    [SerializeField] float apiCallTimeInterval = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        bpmText.gameObject.SetActive(true);
        battLife.gameObject.SetActive(true);
        oxyTime.gameObject.SetActive(true);
        updatingTelemetry = false;
    }

    void Update()
    {
        if (connection == null)
        {
            Debug.LogError("Connection handler is not assigned.");
            return;
        }

        GatewayConnection conn = connection.GetConnection();
        if (conn == null)
        {
            Debug.LogError("Failed to get a valid connection.");
            return;
        }

        if (!updatingTelemetry) StartCoroutine(AttemptTelemetryUpdate(conn));
    }

    IEnumerator AttemptTelemetryUpdate(GatewayConnection conn)
    {
        updatingTelemetry = true;
        string telem = conn.GetTELEMETRYJsonString();
        if (string.IsNullOrEmpty(telem))
        {
            //Debug.LogError("Telemetry data is empty or null.");
        }
        else 
        {
            try
            {
                JObject jo = JObject.Parse(telem);

                UpdateBPM(jo);
                UpdateBatteryLife(jo);
                UpdateOxygenTime(jo);
            }
            catch (JsonReaderException ex)
            {
                //Debug.LogError($"Failed to parse JSON: {ex.Message}. Data: {telem}");
            }
        }
        yield return new WaitForSecondsRealtime(apiCallTimeInterval);
        updatingTelemetry = false;
    }

    void UpdateBPM(JObject jo)
    {
        bpmText.text = "BPM: ";
        float bpm1 = jo["telemetry"][$"eva{ FindObjectOfType<EVAController>().GetEVANumber() }"]["heart_rate"].ToObject<float>();
        bpmText.text += bpm1;

        bpmText.color = (bpm1 > 160 || bpm1 < 50) ? Color.red : Color.green;
    }

    void UpdateBatteryLife(JObject jo)
    {
        battLife.text = "Remaining Battery Time: ";
        float batlife = jo["telemetry"][$"eva{ FindObjectOfType<EVAController>().GetEVANumber() }"]["batt_time_left"].ToObject<float>();

        battLife.color = batlife < 3600 ? Color.red : Color.green;

        int bhours = (int)batlife / 3600;
        batlife %= 3600;
        int bminutes = (int)batlife / 60;
        batlife %= 60;

        battLife.text += $"{bhours:D2}:{bminutes:D2}:{(int)batlife:D2}";

    }

    void UpdateOxygenTime(JObject jo)
    {
        oxyTime.text = "Remaining Oxygen Time: ";
        float oxytime = jo["telemetry"][$"eva{ FindObjectOfType<EVAController>().GetEVANumber() }"]["oxy_time_left"].ToObject<float>();

        oxyTime.color = oxytime < 3600 ? Color.red : Color.green;

        int ohours = (int)oxytime / 3600;
        oxytime %= 3600;
        int ominutes = (int)oxytime / 60;
        oxytime %= 60;

        oxyTime.text += $"{ohours:D2}:{ominutes:D2}:{(int)oxytime:D2}";
    }

    public void OpenCriticalTelemetry()
    {
        criticalTelemetryCanvas.gameObject.SetActive(true);
    }
    public void CloseCriticalTelemetry()
    {
        criticalTelemetryCanvas.gameObject.SetActive(false);
    }
    public void OpenBPM()
    {
        bpmText.gameObject.SetActive(true);
    }
    public void CloseBPM()
    {
        bpmText.gameObject.SetActive(false);
    }
    public void OpenBatteryLife()
    {
        battLife.gameObject.SetActive(true);
    }
    public void CloseBatteryLife()
    {
        battLife.gameObject.SetActive(false);
    }
    public void OpenOxyTime()
    {
        oxyTime.gameObject.SetActive(true);
    }
    public void CloseOxyTime()
    {
        oxyTime.gameObject.SetActive(false);
    }
}
