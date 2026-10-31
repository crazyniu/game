using System.Collections;
using UnityEngine;

namespace PerchSort.View
{
    public sealed class FallingFeathers : MonoBehaviour
    {
        public static void Play(Transform parent, Vector3 position, float speed)
        {
            var effect = new GameObject("Home feathers").AddComponent<FallingFeathers>();
            effect.transform.SetParent(parent,false); effect.transform.localPosition = position;
            effect.StartCoroutine(effect.Animate(speed));
        }
        private IEnumerator Animate(float speed)
        {
            var feathers = new SpriteRenderer[5];
            for (int i = 0; i < feathers.Length; i++) feathers[i] = ViewArt.Sprite("Feather",transform,ViewArt.Get("fx/feather"),Vector3.zero,Vector3.one*.23f,35);
            for (float elapsed = 0; elapsed < .7f; elapsed += Time.unscaledDeltaTime*speed)
            {
                float t = elapsed/.7f;
                for (int i = 0; i < feathers.Length; i++)
                {
                    feathers[i].transform.localPosition = new Vector3((i-2)*t*.23f+Mathf.Sin(t*6+i)*.12f,-t*t*.9f,0);
                    feathers[i].transform.localRotation = Quaternion.Euler(0,0,i*40+t*80);
                    feathers[i].color = new Color(1,1,1,1-t);
                }
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}
