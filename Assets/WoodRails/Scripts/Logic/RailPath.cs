using BansheeGz.BGSpline.Components;
using BansheeGz.BGSpline.Curve;
using UnityEngine;

namespace WoodRails
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(BGCurve))]
    public class RailPath : MonoBehaviour
    {
        public RailAnchor begin;

        public RailAnchor end;

        public BGCurve Curve { get; private set; }

        public BGCcMath Math { get; private set; }


        // Exécuté avant Start()
        void Awake()
        {
            Curve = GetComponent<BGCurve>();
            Math = GetComponent<BGCcMath>();
        }
    }
}