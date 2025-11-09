using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

//Time response deserialisation
//from authoritative backend
[Serializable]
public class TimeResponse
{
    public string dateTime;
    public string year;
    public string month;
    public string day;
    public string dayOfWeek;
}

public class AssetSchedulePeriod
{
    public string Key;
    public DateTime StartTime;
    public DateTime EndTime;

    [JsonConstructor]
    public AssetSchedulePeriod(string key, DateTime start, DateTime end)
    {
        Key = key;
        StartTime = start;
        EndTime = end;
    }
}

public class AssetSchedule
{
    //This schedule would be read from JSON
    public Dictionary<string, AssetSchedulePeriod> schedulePeriods = new Dictionary<string, AssetSchedulePeriod>();
}

//Available runs
//Basically a place to store asset bundle downloads
[Serializable]
public class AssetManifest
{
    public string Version;
    //List of asset bundles keyed against a date key
    public Dictionary<string, AssetBundleInfo[]> Bundles = new Dictionary<string, AssetBundleInfo[]>();
}

//Manifest has list of each required menu selection and points to URL where it can be downloaded
//Once downloaded - the level set is parsed and assigned to the appropriate selection key
//When the corresponding menu selection is pressed and calls for "selectionX" - the level set with dictionary key "selectionX" is loaded
[Serializable]
public class AssetBundleInfo
{
    public string ID;
    public string URL;
}

[ExecuteAlways]
public class DataManager : MonoBehaviour
{
    private const string m_manifestURL = "https://petwords-content-delivery.netlify.app/asset_bundle_manifest.json";
    private const string m_scheduleURL = "https://petwords-content-delivery.netlify.app/asset_delivery_schedule.json";

    //This has to be hosted online
    private AssetSchedule m_assetSchedule;
    private AssetManifest m_manifest;
    private TimeResponse m_timeResponse;

    public AssetBundle m_coreBundle { get; set; }
    public AssetBundle m_quarterlyBundle { get; set; }

    //Available pets
    //They need to be user specific
    //So user pet data needs to be stored somewhere
    //For now it has to be local
    public List<Pet> m_userPetData = new List<Pet>();

    //The keyed level sets that are polled when a menu selection is pressed
    //Asset bundles are parsed, and the level set in inserted with the corresponding selection key
    public SerializableDictionary<string, BossDungeon> m_availableLevelSets = new SerializableDictionary<string, BossDungeon>();

    public void Start()
    {
        if (Application.isPlaying)
        {
            DoFetchRoutine();
        }
    }

    [Button]
    public void DoFetchRoutine()
    {
        StartCoroutine(FetchRoutine());
    }

    private IEnumerator FetchRoutine()
    {
        //Get latest schedule
        TryFetchSchedule();

        while (m_assetSchedule == null)
        {
            Debug.LogFormat($"Updating schedule.");
            yield return null;
        }

        //Get authoritative time
        GetTime();

        while (m_timeResponse == null)
        {
            Debug.LogFormat($"Getting time response.");
            yield return null;
        }

        DateTime utcNow = DateTime.Parse(m_timeResponse.dateTime).ToUniversalTime();

        //Use authoritative time - to get a period from the schedule
        AssetSchedulePeriod authoritativePeriod = FetchPeriodFromDate(utcNow);

        while (authoritativePeriod == null)
        {
            Debug.LogFormat($"Defining authoritative period.");
            yield return null;
        }

        //We fetch the manifest for the specific authoratitive period we are in
        TryFetchManifest(authoritativePeriod.Key);

        while (m_manifest == null)
        {
            yield return null;
        }

        //If we are running the game
        //Try to load each bundle
        //And assign it to the selections

        if (Application.isPlaying)
        {
            foreach (var periodBundle in m_manifest.Bundles)
            {
                //The authoritative period key - matches the bundle key
                //We should try to download it
                if (periodBundle.Key == authoritativePeriod.Key)
                {
                    foreach (var bundle in periodBundle.Value)
                    {
                        TryGetBundle(bundle);
                    }
                }
            }

            while (m_coreBundle == null || m_quarterlyBundle == null)
            {
                Debug.LogFormat($"Still loading bundles.");
                yield return null;
            }

            yield return StartCoroutine(LoadBundle(m_coreBundle, true));
            yield return StartCoroutine(LoadBundle(m_quarterlyBundle));
        }
    }

    //This is the public entry point for getting a specific downloaded level set
    public BossDungeon GetLevelSetByID(string key)
    {
        Debug.LogFormat($"Attemptiong to retrieve level set with key {key}");
        return m_availableLevelSets[key];
    }

    public static string SanitizeJson(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return json;
        }

        // Remove BOM if present
        if (json[0] == '\uFEFF')
        {
            json = json.Substring(1);
        }

        // Trim leading/trailing whitespace and line breaks
        json = json.Trim();

        return json;
    }

    private AssetSchedulePeriod FetchPeriodFromDate(DateTime dateTime)
    {
        //Match the time response to the schedule ID
        foreach (KeyValuePair<string, AssetSchedulePeriod> kvp in m_assetSchedule.schedulePeriods)
        {
            AssetSchedulePeriod period = kvp.Value;

            if (dateTime >= period.StartTime && dateTime <= period.EndTime)
            {
                //Fetch the proper asset bundle
                return period;
            }
        }

        return null;
    }

    [Button]
    public void TryFetchSchedule()
    {
        StartCoroutine(FetchSchedule(result => {
            m_assetSchedule = result;
        }));
    }

    //Get the manifest (list of asset bundles we want to load) from the hosted manifest
    private IEnumerator FetchSchedule(Action<AssetSchedule> onComplete)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(m_scheduleURL))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to fetch schedule: {webRequest.error}");
                yield break;
            }

            //Parse the json to text
            string json = webRequest.downloadHandler.text;
            Debug.Log($"Raw JSON:\n{json}");

            AssetSchedule schedule = null;

            //Try to parse the json to our schedule template
            try
            {
                schedule = JsonConvert.DeserializeObject<AssetSchedule>(SanitizeJson(json));
            }
            catch ( Exception exception )
            {
                Debug.LogError($"Failed to parse JSON: {exception.Message}");
            }

            if (schedule != null)
            {
                Debug.LogFormat($"Schedule has {schedule.schedulePeriods.Count} periods.");

                foreach (KeyValuePair<string, AssetSchedulePeriod> period in schedule.schedulePeriods)
                {
                    Debug.LogFormat($"Schedule period {period.Key} starts {period.Value.StartTime} end {period.Value.EndTime}");
                }

                onComplete.Invoke(schedule);
            }
            else
            {
                onComplete.Invoke(null);
            }
        }
    }

    [Button]
    public void TryFetchManifest(string dateKey)
    {
        StartCoroutine(FetchManifest(result => {
            m_manifest = result;
        }));
    }

    //Get the manifest (list of asset bundles we want to load) from the hosted manifest
    private IEnumerator FetchManifest(Action<AssetManifest> onComplete)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(m_manifestURL))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to fetch manifest: {webRequest.error}");
                yield break;
            }

            //Parse the json to text
            string json = webRequest.downloadHandler.text;
            //Debug.Log($"Raw JSON:\n{json}");

            AssetManifest manifest = null;

            //Try to parse the json to our asset manifest template
            try
            {
                manifest = JsonConvert.DeserializeObject<AssetManifest>(SanitizeJson(json));
            }
            catch ( Exception exception )
            {
                Debug.LogError($"Failed to parse JSON: {exception.Message}");
            }

            if (manifest != null)
            {
                Debug.Log($"Manifest version: {manifest.Version}");
                Debug.Log($"Manifest has {manifest.Bundles.Count} entries");

                Debug.LogFormat($"_____________________________");

                foreach (var period in manifest.Bundles)
                {
                    Debug.LogFormat($"Bundle period: {period.Key}");

                    foreach (AssetBundleInfo bundleInfo in period.Value)
                    {
                        Debug.LogFormat($"Bundle ID: {bundleInfo.ID}");
                        Debug.LogFormat($"Bundle ID: {bundleInfo.URL}");
                    }
                    Debug.LogFormat($"_____________________________");
                }

                onComplete.Invoke(manifest);
            }
            else
            {
                onComplete.Invoke(null);
            }
        }
    }

    [Button]
    private void GetTime()
    {
        //Get time response
        StartCoroutine(GetNetworkTime(result => {
            m_timeResponse = result;
        }));
    }

    private IEnumerator GetNetworkTime(Action<TimeResponse> onComplete)
    {
        using (UnityWebRequest req = UnityWebRequest.Get("https://timeapi.io/api/Time/current/zone?timeZone=Europe/London"))
        {
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                string json = req.downloadHandler.text;
                TimeResponse data = JsonUtility.FromJson<TimeResponse>(json);

                DateTime utcNow = DateTime.Parse(data.dateTime).ToUniversalTime();
                Debug.Log($"Network UTC time: {utcNow}");

                onComplete.Invoke(data);
            }
            else
            {
                Debug.Log("Cannot retrieve authoritative time.");
                onComplete.Invoke(null);
            }
        }
    }

    //Call this with your parsed bundle info
    public void TryGetBundle(AssetBundleInfo bundleInfo)
    {
        StartCoroutine(GetBundle(bundleInfo));
    }

    public IEnumerator LoadBundle(AssetBundle bundle, bool isCore = false)
    {
        AssetBundleRequest bundleRequest = bundle.LoadAllAssetsAsync();

        while (!bundleRequest.isDone)
        {
            Debug.LogFormat($"Loading bundle assets {bundleRequest.progress}");
            yield return null;
        }

        //For quarterly content - actually do the assignment
        if (!isCore)
        {
            //Populate an object array with all the objects inside the assetbundle
            Object[] allAssets = bundleRequest.allAssets;

            foreach (Object asset in allAssets)
            {
                Debug.Log($"{asset.name} ({asset.GetType().Name})");

                if (asset is BossDungeon levelSet)
                {
                    Debug.LogFormat($"Bundle has a scriptable object with {levelSet.Levels.Count} levels");
                    Debug.LogFormat($"Level set has selection ID {levelSet.SelectionID}");
                    //Debug.LogFormat($"Level set has rubrik {levelSet.ScoringRubrik.name}");
                    //The level set IDs are set so that they match up to their menu selection
                    m_availableLevelSets.Add(levelSet.SelectionID, levelSet);

                    Debug.LogFormat($"{m_availableLevelSets.Count} available level sets");
                }
            }
        }
    }

    private IEnumerator GetBundle(AssetBundleInfo bundleInfo)
    {
        using (UnityWebRequest webRequest = UnityWebRequestAssetBundle.GetAssetBundle(bundleInfo.URL, 0))
        {
            //Wait until the request has a result
            yield return webRequest.SendWebRequest();

            //If the result was not successful - bail
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to download bundle '{bundleInfo.ID}': {webRequest.error}");
                yield break;
            }

            //Get the asset bundle from the request
            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(webRequest);

            if (bundle == null)
            {
                Debug.LogError($"Bundle '{bundleInfo.ID}' is empty or invalid");
                yield break;
            }

            Debug.Log($"Successfully loaded bundle '{bundleInfo.ID}'");

            //Hold all the asset names
            string[] assetNames = bundle.GetAllAssetNames();

            //Print them
            foreach (var name in assetNames)
            {
                Debug.Log($"Asset: {name}");
            }

            if (bundleInfo.ID == "quarterly")
            {
                m_quarterlyBundle = bundle;
            }
            else
            {
                m_coreBundle = bundle;
            }
        }
    }
}
