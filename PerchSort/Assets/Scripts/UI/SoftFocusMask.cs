using UnityEngine;
using UnityEngine.UI;
namespace PerchSort.UI
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class SoftFocusMask:MaskableGraphic,ICanvasRaycastFilter
    {
        public Rect Hole;
        private Material focusMaterial;
        protected override void Awake(){base.Awake();focusMaterial=new Material(Shader.Find("Perch/UI Focus"));material=focusMaterial;}
        public void SetHole(Vector2 center,Vector2 size){Hole=new Rect(center-size/2,size);focusMaterial.SetVector("_Hole",new Vector4(center.x,center.y,size.x/2,size.y/2));}
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var r=rectTransform.rect;
            vh.AddVert(new Vector3(r.xMin,r.yMin),Color.white,Vector2.zero);vh.AddVert(new Vector3(r.xMax,r.yMin),Color.white,Vector2.zero);
            vh.AddVert(new Vector3(r.xMax,r.yMax),Color.white,Vector2.zero);vh.AddVert(new Vector3(r.xMin,r.yMax),Color.white,Vector2.zero);vh.AddTriangle(0,1,2);vh.AddTriangle(0,2,3);
        }
        public bool IsRaycastLocationValid(Vector2 screen,Camera camera){RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,screen,camera,out var local);return !Hole.Contains(local);}
        protected override void OnDestroy(){base.OnDestroy();Destroy(focusMaterial);}
    }
}
