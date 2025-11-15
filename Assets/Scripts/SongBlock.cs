using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using SFB;
using System.Collections;
using System;
using System.IO;
using System.Linq;


public class SongBlock : Block
{
    public override string type => "SongBlock";
    [SerializeField] Button chooseFileButton;
    [SerializeField] Button settingsButton;
    [SerializeField] TMP_Text durationText;
    [SerializeField] TMP_Text songTitleText;
    [SerializeField] TMP_Text songArtistText;
    [SerializeField] TMP_Text filePathText;
    [SerializeField] Slider progressBar;
    public SongBlockSettings settings = new SongBlockSettings(false, 1, 0, 0);
    Page page = null;

    AudioClip audioClip = null;
    string songTitle = "none";
    string songArtist = "none";
    public string filePath { get; private set; }
    public bool isPlaying = false;

    public override void printInfo()
    {
        Debug.Log("SongBlock: " + songTitle);
    }

    public override BlockData toBlockData()
    {
        return new SongBlockData
        {
            type = this.type,
            fileName = filePath,
            settings = new SongBlockSettings
            (
                this.settings.muted,
                this.settings.volume,
                this.settings.fadeIn,
                this.settings.fadeOut
            )
        };
    }

    public static SongBlock create(GameObject container, GameObject songBlockPrefab)
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
        songBlock.updateTiming();
        songBlock.initialize();

        return songBlock;
    }

    public static SongBlock create(GameObject container, GameObject songBlockPrefab, string filePath, SongBlockSettings settings)
    {
        SongBlock songBlock = SongBlock.create(container, songBlockPrefab);

        if (!filePath.EndsWith('/'))
        {
            songBlock.StartCoroutine(songBlock.loadAudioClip(filePath));
        }
        else { Debug.LogWarning("Skipped loading song: filePath null"); }

        songBlock.settings = new SongBlockSettings
        (
            settings.muted,
            settings.volume,
            settings.fadeIn,
            settings.fadeOut
        );

        return songBlock;
    }

    private void initialize()
    {
        chooseFileButton.onClick.AddListener(selectMp3File);
        settingsButton.onClick.AddListener(openSettingsMenu);
        upButton.onClick.AddListener(moveUp);
        downButton.onClick.AddListener(moveDown);
        closeButton.onClick.AddListener(destroy);
    }

    void Update()
    {
        progressBar.gameObject.SetActive(isPlaying && page.playingTurnedOn);
        if (isPlaying && page.playingTurnedOn)
        {
            double offset;
            TimeSpan now = DateTime.Now.TimeOfDay;

            if (relativeTiming == false) { offset = (now - startTime).TotalSeconds; }
            else { offset = (now - page.playingStartTime - startTime).TotalSeconds; }

            progressBar.value = (float)(offset / duration.TotalSeconds);
        }
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

        reference.openMenu(this, page.playingTurnedOn, audioClip != null); 
    }

    private void mute(bool isMuted)
    {
        settings.muted = isMuted;
    }

    // checks if current time is in this blocks duration range
    public bool shouldPlayNow()
    {
        TimeSpan now = DateTime.Now.TimeOfDay;
        TimeSpan calculatedStartTime = relativeTiming ? page.playingStartTime + startTime : startTime;

        if (now > calculatedStartTime && now < calculatedStartTime + duration)
        {
            return true;
        }
        return false;
    }

    public void updateAudioSourceSettings(AudioSource audioSource)
    {
        audioSource.mute = settings.muted;
        // audioSource.volume = settings.volume; - updated by fadeAudio coroutine
    }

    public void play()
    {
        if (audioClip == null)
        {
            Debug.LogWarning("Trying to play SongBlock without clip selected");
            return;
        }

        double offset;
        TimeSpan now = DateTime.Now.TimeOfDay;

        if (relativeTiming == false) { offset = (now - startTime).TotalSeconds; }
        else { offset = (now - page.playingStartTime - startTime).TotalSeconds; }
        
        isPlaying = true;
        page.audioSource.clip = audioClip;
        page.audioSource.time = (float)offset;
        page.audioSource.Play();
        page.playingBlock = this;

        StartCoroutine(fadeAudio());
    }

    public void playPreview(float progress)
    {
        if (audioClip == null)
        {
            Debug.LogWarning("Trying to play SongBlock without clip selected");
            return;
        }

        float offset = progress * audioClip.length;

        isPlaying = true;
        page.audioSource.clip = audioClip;
        page.audioSource.time = offset;
        page.audioSource.Play();
        page.playingBlock = this;

        StartCoroutine(fadeAudio());
    }

    public void movePreview(float progress)
    {
        if (page.audioSource.clip == null) { return; }

        page.audioSource.time = progress * audioClip.length;
    }

    public float getPreviewProgress()
    {
        if (page.audioSource.clip == null) { return 0f; }

        float time = page.audioSource.time;
        float length = audioClip.length;
        return time / length;
    }

    public float getAudioClipLength()
    {
        if (audioClip == null) { return 0f; }

        return audioClip.length;
    }

    public void stop()
    {
        isPlaying = false;
        page.playingBlock = null;
        page.audioSource.Stop();
        page.audioSource.clip = null;
    }

    private IEnumerator fadeAudio()
    {
        while (isPlaying)
        {
            TimeSpan now = DateTime.Now.TimeOfDay;
            float elapsedSeconds = (float)page.audioSource.time;
            float durationSeconds = (float)duration.TotalSeconds;

            // fade in
            if (elapsedSeconds >= 0 && elapsedSeconds < settings.fadeIn)
            {
                page.audioSource.volume = elapsedSeconds / settings.fadeIn * settings.volume;
                // Debug.Log("fadeIn: " + page.audioSource.volume);
            }
            // fade out
            else if (elapsedSeconds > durationSeconds - settings.fadeOut && elapsedSeconds <= durationSeconds)
            {
                page.audioSource.volume = (durationSeconds - elapsedSeconds) / settings.fadeOut * settings.volume;
                // Debug.Log("fadeOut: " + page.audioSource.volume);
            }
            else { page.audioSource.volume = settings.volume; }


            yield return null;
        }
    }

    public override void destroy()
    {
        if (page != null && page.playingBlock == this)
        {
            stop();
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
            StartCoroutine(loadAudioClip(paths[0]));
        }
    }

    public IEnumerator loadAudioClip(string filePath)
    {
        string fileUri = "file:///" + Uri.EscapeDataString(filePath.Replace('\\', '/')); 
        UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(fileUri, AudioType.MPEG);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            audioClip = DownloadHandlerAudioClip.GetContent(request);
            Debug.Log("MP3 file loaded successfully! " + filePath);

            this.filePath = filePath;
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
        }
        catch (Exception ex)
        {
            Debug.LogError("failed to read metadata: " + ex.Message);
        }
    }
}
