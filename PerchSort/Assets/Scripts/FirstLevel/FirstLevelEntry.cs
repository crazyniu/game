using System;
using System.Collections;
using System.IO;
using PerchSort.Audio;
using PerchSort.Core.FirstLevel;
using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.FirstLevelRuntime
{
    public sealed class FirstLevelEntry : MonoBehaviour
    {
        [SerializeField] private FirstLevelAssets acceptedAssets;
        [SerializeField] private Camera gameCamera;
        [SerializeField] private Transform safeWorld;
        [SerializeField] private RectTransform safeHud;
        [SerializeField] private SpriteRenderer backdrop;
        private Vector2Int lastOutput;
        private Rect lastSafe;
        private float lastCanvasScale;
        private bool muted, homeVisible, started, engineWarm;
        private Coroutine pendingEntrance;
        private int entranceVersion;
        private AudioManager audioManager;
        private FirstLevelPersistence persistence;
        // Tests/capture set this before Start and restore null afterward.
        public static string SaveDirectoryOverride { get; set; }
        public string SaveDirectory => string.IsNullOrEmpty(SaveDirectoryOverride) ?
            Path.Combine(Application.persistentDataPath,"first-level-v4","bird-home-peer-v4") :
            Path.GetFullPath(SaveDirectoryOverride);
        public FirstLevelFlow Flow { get; private set; }
        public FirstLevelBoard Board { get; private set; }
        public FirstLevelHud Hud { get; private set; }
        public bool IsReady { get; private set; }
        public bool AudioMuted => muted;

        public void Configure(FirstLevelAssets assets, Camera camera, Transform world,
            RectTransform hud, SpriteRenderer background)
        {
            acceptedAssets=assets;gameCamera=camera;safeWorld=world;safeHud=hud;backdrop=background;
        }

        private void Start()
        {
            try
            {
                var initial=State.CreateInitial();initial.ValidateInitialLevelContract(1);
                acceptedAssets.ValidateOrThrow(initial.SpeciesCount);
                audioManager=AudioManager.Instance;
                if(audioManager==null)audioManager=new GameObject("First-level audio").AddComponent<AudioManager>();
                persistence=new FirstLevelPersistence(SaveDirectory);
                ApplyMute();
                var board=new GameObject("First-level runtime board");board.transform.SetParent(safeWorld,false);
                Board=board.AddComponent<FirstLevelBoard>();Board.Initialize(acceptedAssets);
                Board.Failed+=OnViewFailure;
                Hud=safeHud.gameObject.AddComponent<FirstLevelHud>();
                Hud.Initialize(acceptedAssets,safeHud,TogglePause,Undo,Restart,Home,PlayFresh,ToggleMute,()=>muted);
                Hud.BindRetrySave(RetrySave);
                started=true;ApplySafeArea();PlayFresh();
            }
            catch(Exception exception)
            {
                IsReady=false;Debug.LogException(exception,this);enabled=false;
            }
        }

        public void PlayFresh()
        {
            if(!started)return;
            CancelPendingEntrance();
            if(homeVisible && Flow!=null)
            {
                Board.gameObject.SetActive(true);Hud.Bind(Flow,()=>Board.LastHomedSpecies);
                homeVisible=false;Hud.SetHomeVisible(false);IsReady=true;
                audioManager.SwitchMusic(MusicPage.Game);
                Flow.ResumeFromBackground();
                return;
            }
            if(Flow==null)BindLoadedSession(persistence.Load());
        }

        private void BindLoadedSession(FirstLevelLoadResult loaded)
        {
            Flow?.Dispose();
            muted=loaded.AudioMuted;ApplyMute();
            Board.gameObject.SetActive(true);
            Flow=new FirstLevelFlow(loaded.Session ?? new Session(State.CreateInitial()),
                Board,persistence,loaded.HasSnapshot,loaded.IsReadOnly?loaded.Notice:null);
            Board.Bind(Flow);Flow.PresentationFailed+=OnFlowFailure;
            Hud.Bind(Flow,()=>Board.LastHomedSpecies);Hud.RefreshAudio();
            Hud.ShowUnsavedNotice(loaded.Notice);
            homeVisible=false;Hud.SetHomeVisible(false);IsReady=true;
            audioManager.SwitchMusic(MusicPage.Game);
            if(engineWarm)Flow.Start();
            else pendingEntrance=StartCoroutine(StartAfterTwoFrames(Flow,entranceVersion));
        }

        private IEnumerator StartAfterTwoFrames(FirstLevelFlow expected, int version)
        {
            long generation=expected.Generation;
            // Created is already a locked loading phase. Do not start LitMotion on
            // the cold scene's first unscaled delta or override Unity's clock.
            yield return null;
            yield return null;
            if(!isActiveAndEnabled || version!=entranceVersion || Flow!=expected ||
                expected.Generation!=generation || expected.Phase!=FirstLevelPhase.Created)yield break;
            pendingEntrance=null;engineWarm=true;expected.Start();
        }

        private void CancelPendingEntrance()
        {
            entranceVersion++;
            if(pendingEntrance!=null)StopCoroutine(pendingEntrance);
            pendingEntrance=null;
        }

        public void Home()
        {
            if(Flow!=null && !(Flow.IsInteractive || Flow.Phase==FirstLevelPhase.Paused ||
                Flow.Phase==FirstLevelPhase.Won || Flow.Phase==FirstLevelPhase.Faulted ||
                Flow.Phase==FirstLevelPhase.SaveError ||
                (Flow.Phase==FirstLevelPhase.Created && pendingEntrance!=null)))return;
            CancelPendingEntrance();
            Flow?.Suspend(); // Keep the same pending candidate/Undo stack, including uncertain saves.
            Board.Cancel();Board.gameObject.SetActive(false);
            Hud.Bind(null,()=>Board.LastHomedSpecies);Hud.SetHomeVisible(true);
            homeVisible=true;IsReady=false;audioManager.SwitchMusic(MusicPage.Menu);
        }

        public void Restart()
        {
            if(homeVisible){PlayFresh();return;}
            if(pendingEntrance!=null && Flow!=null && Flow.Phase==FirstLevelPhase.Created)
            {
                CancelPendingEntrance();
                pendingEntrance=StartCoroutine(StartAfterTwoFrames(Flow,entranceVersion));
                return;
            }
            Flow?.Restart();
        }
        public void Undo()=>Flow?.Undo();
        public void TogglePause()
        {
            if(Flow==null)return;
            if(Flow.Phase==FirstLevelPhase.Paused)Flow.ResumePause();
            else Flow.RequestPause();
        }

        public void ToggleMute()
        {
            if(Flow!=null && Flow.HasPendingSave)return;
            muted=!muted;ApplyMute();Hud.RefreshAudio();
            try
            {
                var outcome=persistence.SaveAudioMuted(muted);
                if(outcome==FirstLevelCommitOutcome.Committed)Hud.ShowUnsavedNotice(null);
                else
                {
                    Hud.ShowUnsavedNotice("Sound setting is not saved.");
                    if(outcome==FirstLevelCommitOutcome.Uncertain)
                        Flow?.ReportExternalSaveError("The sound setting save is uncertain. Retry to verify it.");
                }
            }
            catch(Exception)
            {
                Hud.ShowUnsavedNotice("Sound setting is not saved.");
                Flow?.ReportExternalSaveError("The sound setting save needs verification. Retry or return Home.");
            }
        }

        public void RetrySave()
        {
            if(Flow==null)return;
            if(Flow.HasPendingSave)
            {
                if(Flow.RetrySave())RefreshSettingsNotice();
                return;
            }
            if(persistence.HasPendingCandidate)
            {
                var outcome=persistence.RetryPendingAudio();
                if(outcome!=FirstLevelCommitOutcome.Uncertain)
                {
                    Flow.ResumeAfterSettingsVerification();RefreshSettingsNotice();
                }
                return;
            }
            // Read-only unknown/corrupt/conflicting data is only re-read here.
            // Do not discard it, restart, or replace it with the initial board.
            var retryStore=new FirstLevelPersistence(SaveDirectory);
            var loaded=retryStore.Load();
            if(loaded.IsReadOnly)
            {
                Flow.ReportExternalSaveError(loaded.Notice ?? "Save remains read-only. Return Home or exit.");
                return;
            }
            CancelPendingEntrance();persistence=retryStore;BindLoadedSession(loaded);
        }

        private void RefreshSettingsNotice()
        {
            Hud.ShowUnsavedNotice(muted==persistence.AudioMuted?null:"Sound setting is not saved.");
        }
        private void ApplyMute()
        {
            audioManager.SetSfxEnabled(!muted);audioManager.SetMusicEnabled(!muted);
            audioManager.SetVolumes(1,1);
        }

        private void OnViewFailure(Exception exception)=>Flow?.ReportPresentationFailure(exception);
        private void OnFlowFailure(Exception exception)
        {
            Debug.LogException(exception,this);
            Hud?.ShowError("Unable to present this level. Replay to recover.");
        }

        private void LateUpdate()
        {
            if(!started)return;
            var output=OutputSize;
            float scale=safeHud.GetComponentInParent<Canvas>().scaleFactor;
            if(output!=lastOutput || lastSafe!=Screen.safeArea || scale!=lastCanvasScale)ApplySafeArea();
        }
        private Vector2Int OutputSize=>gameCamera.targetTexture!=null ?
            new Vector2Int(gameCamera.targetTexture.width,gameCamera.targetTexture.height):
            new Vector2Int(Screen.width,Screen.height);

        public void ApplySafeArea()
        {
            var output=OutputSize;
            float vw=Mathf.Min(1f,9f/16f*output.y/output.x);
            gameCamera.rect=new Rect((1-vw)*.5f,0,vw,1);
            var viewport=new Rect(output.x*(1-vw)*.5f,0,output.x*vw,output.y);
            var systemSafe=gameCamera.targetTexture!=null?new Rect(0,0,output.x,output.y):Screen.safeArea;
            var safe=Rect.MinMaxRect(Mathf.Max(viewport.xMin,systemSafe.xMin),Mathf.Max(viewport.yMin,systemSafe.yMin),
                Mathf.Min(viewport.xMax,systemSafe.xMax),Mathf.Min(viewport.yMax,systemSafe.yMax));
            gameCamera.orthographicSize=Mathf.Max(9.6f,5.4f*viewport.height/viewport.width);
            float perPixel=2*gameCamera.orthographicSize/viewport.height;
            float s=Mathf.Min(safe.width/1080,safe.height/1920);
            var center=safe.center-viewport.center;
            safeWorld.localPosition=new Vector3(center.x,center.y,0)*perPixel;
            safeWorld.localScale=Vector3.one*s*100*perPixel;
            float canvasScale=safeHud.GetComponentInParent<Canvas>().scaleFactor;
            safeHud.anchoredPosition=center/canvasScale;safeHud.localScale=Vector3.one*s/canvasScale;
            float bg=Mathf.Max(viewport.width*perPixel/backdrop.sprite.bounds.size.x,
                viewport.height*perPixel/backdrop.sprite.bounds.size.y);
            backdrop.transform.localScale=Vector3.one*bg;
            lastOutput=output;lastSafe=Screen.safeArea;lastCanvasScale=canvasScale;
        }

        private void OnApplicationPause(bool pause)
        {
            if(!IsReady || Flow==null)return;
            if(pause)Flow.Suspend();else Flow.ResumeFromBackground();
        }
        private void OnApplicationFocus(bool focus)
        {
            // Batch capture has no interactive window and is not a focus-lifecycle test.
            if(Application.isBatchMode || !IsReady || Flow==null)return;
            if(focus)Flow.ResumeFromBackground();else Flow.Suspend();
        }
        private void OnDestroy()
        {
            CancelPendingEntrance();
            if(Board!=null)Board.Failed-=OnViewFailure;
            Flow?.Dispose();Flow=null;IsReady=false;
        }
    }
}
