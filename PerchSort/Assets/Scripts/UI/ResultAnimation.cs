using UnityEngine;
using UnityEngine.UI;
namespace PerchSort.UI
{
    public sealed class ResultAnimation:MonoBehaviour
    {
        private Image[] stars;private readonly Image[] rays=new Image[3];private RectTransform trophy;private Vector2 trophyRest;private readonly Image[] flashes=new Image[3];
        private readonly Image[] papers=new Image[120];
        private readonly Vector2[] origins=new Vector2[120],velocities=new Vector2[120];
        private readonly float[] spins=new float[120];
        private int earned,sounded;private float start;private bool reduced;
        public void Build(UiTheme theme,Image[] rewards)
        {
            stars=rewards;trophy=(RectTransform)transform.Find("Win decoration");trophyRest=trophy.anchoredPosition;
            for(int i=0;i<3;i++){rays[i]=theme.Image("Star rays",stars[i].transform,theme.Art("glow_rays"),Vector2.zero,Vector2.one*250);rays[i].transform.SetAsFirstSibling();}
            for(int i=0;i<3;i++)flashes[i]=theme.Image("Star flash",stars[i].transform,View.ViewArt.Get("fx/sparkle"),Vector2.zero,Vector2.one*210);
            Color[] colors={new Color(1,.353f,.353f),new Color(1,.824f,.247f),new Color(.42f,.816f,.42f),new Color(.31f,.659f,1),new Color(1,.498f,.784f),new Color(.663f,.482f,1)};
            var paperMaterial=new Material(Shader.Find("Perch/UI Confetti"));
            for(int i=0;i<papers.Length;i++)
            {
                papers[i]=theme.Image("Confetti "+i,transform,View.ViewArt.Get($"fx/confetti_{i%3+1:00}"),Vector2.zero,Vector2.one*Random.Range(20,34));papers[i].color=colors[i%6];papers[i].material=paperMaterial;
                float side=i<60?-1:1;origins[i]=new Vector2(side*510,900+Random.Range(-50,50));
                velocities[i]=new Vector2(-side*Random.Range(30,470),Random.Range(-300,-50));spins[i]=Random.Range(-360,360);
            }
        }
        public void Play(int count,bool lessMotion)
        {
            earned=count;reduced=lessMotion;start=Time.unscaledTime;sounded=0;
            for(int i=0;i<3;i++){stars[i].gameObject.SetActive(i<earned);stars[i].transform.localScale=Vector3.zero;}
            for(int i=0;i<papers.Length;i++)papers[i].gameObject.SetActive(!reduced||i%15==0);
        }
        private void Update()
        {
            float elapsed=Time.unscaledTime-start;
            trophy.anchoredPosition=trophyRest+Vector2.up*(reduced?0:Mathf.Sin(elapsed*2)*10);
            for(int i=0;i<earned;i++)
            {
                float t=Mathf.Clamp01((elapsed-i*.35f)/.35f);float p=t-1;float scale=reduced?1:t<.65f?Mathf.Lerp(0,1.2f,t/.65f):Mathf.Lerp(1.2f,1,(t-.65f)/.35f);
                stars[i].transform.localScale=Vector3.one*scale;stars[i].color=new Color(1,1,1,reduced?t:1);
                rays[i].rectTransform.localRotation=Quaternion.Euler(0,0,reduced?0:elapsed*25);
                rays[i].color=new Color(1,1,1,.35f);
                flashes[i].color=new Color(1,1,1,reduced?0:Mathf.Sin(t*Mathf.PI)*.8f);
                if(elapsed>=i*.35f&&sounded<=i){Audio.AudioManager.Instance.Play((Audio.Sfx)((int)Audio.Sfx.Star1+i));sounded=i+1;}
            }
            for(int i=0;i<papers.Length;i++)
            {
                if(reduced){papers[i].rectTransform.anchoredPosition=new Vector2((i%2==0?-1:1)*430,780-i*11);continue;}
                papers[i].gameObject.SetActive(elapsed<2.2f);
                papers[i].rectTransform.anchoredPosition=origins[i]+velocities[i]*elapsed+Vector2.down*(250*elapsed*elapsed)+Vector2.right*(Mathf.Sin(elapsed*5+i)*22);
                papers[i].rectTransform.localRotation=Quaternion.Euler(0,0,i*37+elapsed*spins[i]);
            }
        }
    }
}
