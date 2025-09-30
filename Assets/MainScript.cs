using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Video;

public class MainScript : MonoBehaviour
{
    public UDPConnection udpConnection;
    public VideoPlayer WallVideoPlayer;
    public GameObject SettingsUI;
    public TMP_InputField IPAddressInput;
    public TMP_Text StatusInfo;

    string currentdirect;
    string videopath;
    string[] videocontents;
    int currentvideoindex = 0;
    bool isSettings = false;
    string ipAddress = "127.0.0.1";

    void Start()
    {

        SettingsUI.gameObject.SetActive(isSettings);

        currentdirect = Directory.GetCurrentDirectory();
        
        #if UNITY_EDITOR
    videopath = Path.Combine(currentdirect, "Videos");
#elif UNITY_STANDALONE_WIN
    videopath = Path.Combine(currentdirect, "Videos");
#elif UNITY_ANDROID
    videopath = Path.Combine(Application.persistentDataPath, "Videos");
#else
    videopath = Path.Combine(currentdirect, "Videos");
#endif

        Debug.Log("Video Path: " + videopath);

        if (!Directory.Exists(videopath)) Directory.CreateDirectory(videopath);

        videocontents = Directory.GetDirectories(videopath);

        if (videocontents.Length == 0)
        {
            Debug.LogError("No video folders found in Videos directory. Baliw ka talaga");
            return;
        }

        StatusInfo.text = "";

        Cursor.visible = false;

        StartCoroutine(PlayVideos());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Cursor.visible = true;
        }
    }

    IEnumerator PlayVideos()
    {
        string indexvideo = videocontents[currentvideoindex];

        string wallvidpath = Path.Combine(indexvideo, "wall.mp4");

        if (!File.Exists(wallvidpath))
        {
            Debug.LogError($"wall.mp4 is missing in folder: {indexvideo}");
            yield break;
        }
        else
        {
            WallVideoPlayer.url = wallvidpath;
        }

        if (WallVideoPlayer.isPlaying)
        {
            WallVideoPlayer.Stop();
        }

        WallVideoPlayer.Play();
    }

    public void onNextButton()
    {
        // udpConnection.SendData("next", ipAddress);
        udpConnection.SendBroadcast("next");

        currentvideoindex++;
        if (currentvideoindex >= videocontents.Length) currentvideoindex = 0;

        StartCoroutine(PlayVideos());
    }

    public void onPreviousButton()
    {
        // udpConnection.SendData("previous", ipAddress);
        udpConnection.SendBroadcast("previous");

        currentvideoindex--;
        if (currentvideoindex < 0) currentvideoindex = videocontents.Length - 1;

        StartCoroutine(PlayVideos());
    }

    public void onSettingsButton()
    {
        isSettings = !isSettings;
        SettingsUI.gameObject.SetActive(isSettings);
    }

    public void onIPSaveButton()
    {
        ipAddress = IPAddressInput.text;
        StatusInfo.text = "Connected to: " + ipAddress;
    }
}
