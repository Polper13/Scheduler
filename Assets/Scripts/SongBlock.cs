using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using SFB;
using System.Collections;
using System;
using System.Linq;
using Unity.VisualScripting;

public class SongBlock : Block
{
    [SerializeField] Button chooseFileButton;
    [SerializeField] Button settingsButton;
    [SerializeField] TMP_Text durationText;
    [SerializeField] TMP_Text songTitleText;
    [SerializeField] TMP_Text songArtistText;
    [SerializeField] TMP_Text filePathText;
    [SerializeField] SongBlockSettings settings = new SongBlockSettings(false, 1);
    Page page = null;

    AudioClip audioClip = null;
    string songTitle = "none";
    string songArtist = "none";
    string filePath = null;
    public bool isPlaying = false;

    public override void printInfo()
    {
        Debug.Log("SongBlock: " + songTitle);
    }
    
    public static void create(GameObject container, GameObject songBlockPrefab)
    {
        GameObject songBlockGameObject = Instantiate(songBlockPrefab, container.transform);

        GameObject page = container.transform.parent.parent.gameObject;
        SongBlock songBlock = songBlockGameObject.GetComponent<SongBlock>();
        songBlock.blockList = page.GetComponent<Page>().blockList;
        songBlock.blockList.Add(songBlock);
        songBlock.page = page.GetComponent<Page>();

        // set sibling index (order of displayingl in block container) according to index in the list
        songBlockGameObject.transform.SetSiblingIndex(songBlock.blockList.Count - 1);

        // setup
        songBlock.initialize();
    }

    private void initialize()
    {
        chooseFileButton.onClick.AddListener(selectMp3File);
        settingsButton.onClick.AddListener(openSettingsMenu);
        upButton.onClick.AddListener(moveUp);
        downButton.onClick.AddListener(moveDown);
        closeButton.onClick.AddListener(destroy);
    }

    // updates the displayed info about the song
    private void updateSongInfo()
    {
        durationText.text = duration.ToString(@"mm\:ss");
        songArtistText.text = songArtist;
        songTitleText.text = songTitle;
        filePathText.text = filePath;
    }

    private void openSettingsMenu()
    {
        SongBlockSettingsMenu reference = Resources.FindObjectsOfTypeAll<SongBlockSettingsMenu>().FirstOrDefault();
        if (reference == null) { Debug.LogWarning("Couldnt find SongBlockSettingsMenu"); return; }

        reference.openMenu(settings); 
    }

    private void mute(bool isMuted)
    {
        settings.muted = isMuted;
    }

    // checks if current time is in this blocks duration range
    public bool shouldPlayNow()
    {
        TimeSpan currentTime = DateTime.Now.TimeOfDay;
        if (currentTime > startTime && currentTime < startTime + duration)
        {
            return true;
        }
        return false;
    }

    public void updateAudioSourceSettings(AudioSource audioSource)
    {
        audioSource.mute = settings.muted;
        audioSource.volume = settings.volume;
    }

    public void play(Page page)
    {
        if (audioClip == null)
        {
            Debug.LogWarning("Trying to play SongBlock without clip selected");
            return;
        }

        TimeSpan now = DateTime.Now.TimeOfDay;
        double offset = (now - startTime).TotalSeconds;

        isPlaying = true;
        page.audioSource.clip = audioClip;
        page.audioSource.time = (float)offset;
        page.audioSource.Play();
        page.playingBlock = this;
    }

    public void stop(Page page)
    {
        isPlaying = false;
        page.playingBlock = null;
        page.audioSource.Stop();
        page.audioSource.clip = null;
    }

    private void destroy()
    {
        if (page != null && page.playingBlock == this)
        {
            stop(page);
        }

        if (blockList != null && blockList.Contains(this))
        {
            blockList.Remove(this);
        }

        if (chooseFileButton != null) { chooseFileButton.onClick.RemoveAllListeners(); }
        if (settingsButton != null) { settingsButton.onClick.RemoveAllListeners(); }
        if (upButton != null) { upButton.onClick.RemoveAllListeners(); }
        if (downButton != null) { downButton.onClick.RemoveAllListeners(); }
        if (closeButton != null) { closeButton.onClick.RemoveAllListeners(); }

        updateWholePageTiming();
        Destroy(this.gameObject);
    }

    private void selectMp3File()
    {
        // Open file dialog
        var paths = StandaloneFileBrowser.OpenFilePanel("Select an MP3 File", "", "mp3", false);

        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            string filePath = paths[0];
            StartCoroutine(loadAudioClip(filePath));
        }
    }

    private IEnumerator loadAudioClip(string filePath)
    {
        string fileUri = "file:///" + Uri.EscapeDataString(filePath.Replace('\\', '/')); 
        UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(fileUri, AudioType.MPEG);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            audioClip = DownloadHandlerAudioClip.GetContent(request);
            Debug.Log("MP3 file loaded successfully! " + filePath);

            loadMetadata(filePath);
            updateSongInfo();

            // update blocks timing after this one changed its duration
            updateWholePageTiming();
        }
        else
        {
            Debug.LogError("Failed to load MP3 file: " + filePath + '\n' + request.error);
        }
    }

    private void loadMetadata(string filePath)
    {
        try
        {
            var file = TagLib.File.Create(filePath);

            duration = file.Properties.Duration;
            songArtist = file.Tag.FirstPerformer ?? "unknown";
            songTitle = file.Tag.Title ?? "unknown";
            this.filePath = filePath;
        }
        catch (Exception ex)
        {
            Debug.LogError("failed to read metadata: " + ex.Message);
        }
    }
}
