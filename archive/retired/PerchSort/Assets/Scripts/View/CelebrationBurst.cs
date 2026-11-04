using System.Collections;
using UnityEngine;

namespace PerchSort.View
{
    public sealed class CelebrationBurst : MonoBehaviour
    {
        private Sprite shadeArt;

        public static void Play(Transform parent, Vector3 position, float speed)
        {
            var burst = new GameObject("Celebration particles").AddComponent<CelebrationBurst>();
            burst.transform.SetParent(parent, false);
            burst.transform.localPosition = position;
            burst.StartCoroutine(burst.Animate(speed));
        }
        private IEnumerator Animate(float speed)
        {
            shadeArt = Sprite.Create(Texture2D.whiteTexture,new Rect(0,0,1,1),Vector2.one*.5f,1);
            var shade = ViewArt.Sprite("Celebration shade",transform,shadeArt,-transform.localPosition,new Vector3(10.8f,19.2f,1),39);
            var art = new[] { ViewArt.Get("fx/confetti_01"), ViewArt.Get("fx/confetti_02"), ViewArt.Get("fx/confetti_03") };
            var particles = new SpriteRenderer[96];
            var origins = new Vector3[particles.Length];
            var velocities = new Vector3[particles.Length];
            var spins = new float[particles.Length];
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i] = ViewArt.Sprite("Confetti",transform,art[Random.Range(0,3)],Vector3.zero,Vector3.one * Random.Range(.14f,.23f),40);
                float side = i % 2 == 0 ? -1 : 1;
                origins[i] = new Vector3(side*5.1f,2.2f+Random.Range(-.3f,.3f),0);
                velocities[i] = new Vector3(-side*Random.Range(.5f,6f),Random.Range(-5f,-.5f),0);
                spins[i] = Random.Range(-360f,360f);
            }
            for (float elapsed = 0; elapsed < 2f; elapsed += Time.unscaledDeltaTime * speed)
            {
                float t = elapsed / 2f;
                shade.color = new Color(.13f,.22f,.11f,.25f * Mathf.Sin(t*Mathf.PI));
                for (int i = 0; i < particles.Length; i++)
                {
                    particles[i].transform.localPosition = origins[i] + velocities[i] * elapsed + Vector3.down * (3.5f * elapsed * elapsed);
                    particles[i].transform.localRotation = Quaternion.Euler(0,0,i*37 + elapsed*spins[i]);
                    particles[i].color = new Color(1,1,1,1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.65f,1,t)));
                }
                yield return null;
            }
            Destroy(gameObject);
        }
        private void OnDestroy() => Destroy(shadeArt);
    }
}
