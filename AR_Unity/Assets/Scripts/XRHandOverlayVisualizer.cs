using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.SubsystemsImplementation;

public class XRHandOverlayVisualizer : MonoBehaviour
{
    public enum HandSide { Left, Right }

    [Header("Settings")]
    public HandSide handSide = HandSide.Right;
    public Material capsuleMat;
    public float capsuleRadius = 0.003f;
    public bool showCapsule = true;
    public Material jointMat;
    public float jointRadius = 0.005f;
    public bool showJoint = true;

    private XRHandSubsystem handSubsystem;
    private List<CapsuleVisual> capsuleVisuals;
    private List<JointVisual> jointVisuals;

    private static readonly List<(XRHandJointID start, XRHandJointID end)> CapsuleLinks = new()
    {
        // Boundaries and finger bones
        (XRHandJointID.Wrist, XRHandJointID.ThumbMetacarpal),
        (XRHandJointID.Wrist, XRHandJointID.LittleMetacarpal),
        (XRHandJointID.ThumbMetacarpal, XRHandJointID.ThumbProximal),
        (XRHandJointID.ThumbProximal, XRHandJointID.ThumbDistal),
        (XRHandJointID.ThumbDistal, XRHandJointID.ThumbTip),

        (XRHandJointID.IndexProximal, XRHandJointID.IndexIntermediate),
        (XRHandJointID.IndexIntermediate, XRHandJointID.IndexDistal),
        (XRHandJointID.IndexDistal, XRHandJointID.IndexTip),

        (XRHandJointID.MiddleProximal, XRHandJointID.MiddleIntermediate),
        (XRHandJointID.MiddleIntermediate, XRHandJointID.MiddleDistal),
        (XRHandJointID.MiddleDistal, XRHandJointID.MiddleTip),

        (XRHandJointID.RingProximal, XRHandJointID.RingIntermediate),
        (XRHandJointID.RingIntermediate, XRHandJointID.RingDistal),
        (XRHandJointID.RingDistal, XRHandJointID.RingTip),

        (XRHandJointID.LittleMetacarpal, XRHandJointID.LittleProximal),
        (XRHandJointID.LittleProximal, XRHandJointID.LittleIntermediate),
        (XRHandJointID.LittleIntermediate, XRHandJointID.LittleDistal),
        (XRHandJointID.LittleDistal, XRHandJointID.LittleTip),

        // Webbing links
        (XRHandJointID.ThumbProximal, XRHandJointID.IndexProximal),
        (XRHandJointID.IndexProximal, XRHandJointID.MiddleProximal),
        (XRHandJointID.MiddleProximal, XRHandJointID.RingProximal),
        (XRHandJointID.RingProximal, XRHandJointID.LittleProximal)
    };

    void Start()
    {
        var subsystems = new List<XRHandSubsystem>();
        SubsystemManager.GetSubsystems(subsystems);
        if (subsystems.Count > 0) handSubsystem = subsystems[0];
        else Debug.LogWarning("XRHandSubsystem not found.");

        capsuleVisuals = new List<CapsuleVisual>();
        foreach (var link in CapsuleLinks)
            capsuleVisuals.Add(new CapsuleVisual(gameObject, link.start, link.end, capsuleMat));

        jointVisuals = new List<JointVisual>();
        foreach (XRHandJointID id in System.Enum.GetValues(typeof(XRHandJointID)))
            if (id != XRHandJointID.Invalid && id != XRHandJointID.EndMarker)
                jointVisuals.Add(new JointVisual(gameObject, id, jointMat));
    }

    void LateUpdate()
    {
        if (handSubsystem == null || !handSubsystem.running) return;

        XRHand hand = (handSide == HandSide.Left) ? handSubsystem.leftHand : handSubsystem.rightHand;
        if (!hand.isTracked)
        {
            HideAll();
            return;
        }

        foreach (var c in capsuleVisuals) c.Update(hand, showCapsule, capsuleRadius);
        foreach (var j in jointVisuals) j.Update(hand, showJoint, jointRadius);
    }

    public void ForceVisualReset()
    {
        // Hide all visuals now
        HideAll();

        // The next LateUpdate will re-show them using fresh joint data
        // because it re-runs Update() on each visual
    }

    void HideAll()
    {
        capsuleVisuals.ForEach(c => c.Hide());
        jointVisuals.ForEach(j => j.Hide());
    }

    class CapsuleVisual
    {
        GameObject go;
        XRHandJointID start, end;
        MeshRenderer renderer;

        public CapsuleVisual(GameObject root, XRHandJointID a, XRHandJointID b, Material mat)
        {
            start = a; end = b;
            go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.transform.SetParent(root.transform);
            GameObject.Destroy(go.GetComponent<CapsuleCollider>());
            renderer = go.GetComponent<MeshRenderer>();
            if (mat) renderer.material = mat;
            go.SetActive(false);
        }

        public void Update(XRHand hand, bool show, float radius)
        {
            if (!show)
            {
                go.SetActive(false);
                return;
            }

            var js = hand.GetJoint(start);
            var je = hand.GetJoint(end);
            if (!js.TryGetPose(out var ps) || !je.TryGetPose(out var pe))
            {
                go.SetActive(false);
                return;
            }

            go.SetActive(true);
            Vector3 a = ps.position, b = pe.position;
            go.transform.position = (a + b) * 0.5f;
            go.transform.up = b - a;
            float dist = Vector3.Distance(a, b);
            go.transform.localScale = new Vector3(radius * 2, dist * 0.5f, radius * 2);
        }

        public void Hide() => go.SetActive(false);
    }

    class JointVisual
    {
        GameObject go;
        XRHandJointID id;
        MeshRenderer renderer;

        public JointVisual(GameObject root, XRHandJointID id, Material mat)
        {
            this.id = id;
            go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.SetParent(root.transform);
            GameObject.Destroy(go.GetComponent<SphereCollider>());
            renderer = go.GetComponent<MeshRenderer>();
            if (mat) renderer.material = mat;
            go.SetActive(false);
        }

        public void Update(XRHand hand, bool show, float radius)
        {
            if (!show)
            {
                go.SetActive(false);
                return;
            }

            var j = hand.GetJoint(id);
            if (!j.TryGetPose(out var p)) { go.SetActive(false); return; }

            go.SetActive(true);
            go.transform.position = p.position;
            go.transform.localScale = Vector3.one * radius * 2;
        }

        public void Hide() => go.SetActive(false);
    }
}
