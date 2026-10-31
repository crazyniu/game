using System;
using System.Collections;
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
                else muted=!(audioManager.SfxEnabled && audioManager.MusicEnabled);
                ApplyMute();
                var board=new GameObject("First-level runtime board");board.transform.SetParent(safeWorld,false);
                Board=board.AddComponent<FirstLevelBoard>();Board.Initialize(acceptedAssets);
                Board.Failed+=OnViewFailure;
                Hud=safeHud.gameObject.AddComponent<FirstLevelHud>();
                Hud.Initialize(acceptedAssets,safeHud,TogglePause,Undo,Restart,Home,PlayFresh,ToggleMute,()=>muted);
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
            Flow?.Dispose();
            Board.gameObject.SetActive(true);
            var initial=State.CreateInitial();initial.ValidateInitialLevelContract(1);
            Flow=new FirstLevelFlow(initial,Board);
            Board.Bind(Flow);
            Flow.PresentationFailed+=OnFlowFailure;
            Hud.Bind(Flow,()=>Board.LastHomedSpecies);
            homeVisible=false;Hud.SetHomeVisible(false);
            audioManager.SwitchMusic(MusicPage.Game);IsReady=true;
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
                (Flow.Phase==FirstLevelPhase.Created && pendingEntrance!=null)))return;
            CancelPendingEntrance();
            Flow?.Dispose();Flow=null;
            Board.Cancel();Board.gameObject.SetActive(false);
            Hud.Bind(null,()=>Board.LastHomedSpecies);Hud.SetHomeVisible(true);
            homeVisible=true;IsReady=false;audioManager.SwitchMusic(MusicPage.Menu);
        }

        public void Restart(){if(homeVisible || pendingEntrance!=null)PlayFresh();else Flow?.Restart();}
        public void Undo()=>Flow?.Undo();
        public void TogglePause()
        {
            if(Flow==null)return;
            if(Flow.Phase==FirstLevelPhase.Paused)Flow.ResumePause();
            else Flow.RequestPause();
        }

        public void ToggleMute()
        {
            muted=!muted;ApplyMute();Hud.RefreshAudio();
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
