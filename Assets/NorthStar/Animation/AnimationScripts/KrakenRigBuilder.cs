// Copyright (c) Meta Platforms, Inc. and affiliates.

using System;
using System.Collections.Generic;
using JigglePhysics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace NorthStar
{
    public class KrakenRigBuilder : MonoBehaviour
    {
        // Constants
        public const float VERLET_TIME_STEP = 0.02f;
        public const float MAX_CATCHUP_TIME = VERLET_TIME_STEP * 4f;

        // Private
        private double m_accumulation;
        private bool m_dirtyFromEnable = false;
        private bool m_wasLODActive = true;
        private Transform m_cameraTransform;
        private Camera m_currentCamera;
        private float m_lODblend;
        private float m_dynamicBlend;
        private float m_staticBlend;

        // User defined
        [Tooltip("Enables interpolation for the simulation, this should be enabled unless you *really* need the simulation to only update on FixedUpdate.")]
        public bool interpolate = true;
        protected List<KrakenBone> m_krakenBones;
        public List<KrakenRig> krakenRigs;
        public LevelOfDetailValues LevelOfDetailSettings;
        private bool m_debugDraw;

        [SerializeField]
        [Tooltip("An air force that is applied to the entire rig, this is useful to plug in some wind volumes from external sources.")]
        public Vector3 wind;
        [Range(0f, 2f)]
        public float gravityMultiplier = 1f;
        [Range(0f, 1f)]
        public float friction = 0.5f;
        [Range(0f, 1f)]
        public float elasticity;
        [Range(0f, 1f)]
        public float angleElasticity = 0.5f;
        [Range(0f, 1f)]
        public float blend = 0.9f;
        [Range(0f, 1f)]
        public float airDrag = 0.4f;
        [Range(0f, 1f)]
        public float lengthElasticity = 0.6f;
        [Range(0f, 1f)]
        public float elasticitySoften = 0.5f;
        [Range(0f, 2f)]
        public float radiusMultiplier = 1;
        public AnimationCurve radiusCurve = new(new Keyframe(0f, 1f), new Keyframe(1f, 0f));

        // LOD values dropdown
        [Serializable]
        public class LevelOfDetailValues
        {
            [SerializeField]
            public bool useLevelOfDetail = false;
            [Tooltip("The distance from the camera at which the simulation is disabled.")]
            public float Distance;
            public float blend;

            private Transform m_cameraTransform;
            private Camera m_currentCamera;

            private bool TryGetCamera(out Camera camera)
            {
#if UNITY_EDITOR
                if (EditorWindow.focusedWindow is SceneView view)
                {
                    camera = view.camera;
                    return camera != null;
                }
#endif
                if (m_currentCamera == null || !m_currentCamera.CompareTag("MainCamera"))
                {
                    m_currentCamera = Camera.main;
                }
                camera = m_currentCamera;
                return m_currentCamera != null;
            }

            public bool CheckActive(Vector3 position)
            {
                return TryGetCamera(out var camera) && Vector3.Distance(camera.transform.position, position) < Distance;
            }

            public float AdjustSettings(Vector3 position, float dynamicBlend)
            {
                if (!TryGetCamera(out var camera))
                {
                    if (useLevelOfDetail)
                    {
                        var newBlend = (Vector3.Distance(camera.transform.position, position) - Distance + blend) / blend;
                        newBlend = Mathf.Clamp01(1f - newBlend);
                        return newBlend;
                    }
                }
                return dynamicBlend;
            }
        }

        // Rig Creator
        [Serializable]
        public class KrakenRig
        {

            // User defined
            [SerializeField]
            [Tooltip("The root bone from which an individual KrakenRig will be constructed. The rig encompasses all children of the specified root.")]
            [FormerlySerializedAs("target")]
            public Transform rootBone;
            [SerializeField]
            [Tooltip("The list of bones to ignore during the simulation. Each bone listed will also ignore all the children of the specified bone.")]
            private List<Transform> m_ignoredBones;
            public List<Collider> collisionObjects;
            public bool useKeyedBlend = true;

            public Transform GetRootTransform() => rootBone;
            public KrakenRig(Transform rootTransform,
                ICollection<Transform> ignoredBones, ICollection<Collider> collisionObjects)
            {
                rootBone = rootTransform;
                m_ignoredBones = new List<Transform>(ignoredBones);
                this.collisionObjects = new List<Collider>(collisionObjects);
                Initialize();
            }

            // Private
            private bool m_initialized;
            private bool NeedsCollisions => collisionObjects.Count != 0;

            protected List<KrakenBone> m_krakenBones;

            // PREP
            public void PrepareBone(Vector3 position, float dynamicBlend, LevelOfDetailValues lodValues)
            {
                //if (!initialized)
                //{
                //    throw new UnityException("Your Kraken Rig was never initialized! Call KrakenRig.Initialize() if you're going to manually timestep.");
                //}

                foreach (var bone in m_krakenBones)
                {
                    bone.PrepareBone();
                }

                _ = lodValues.AdjustSettings(position, dynamicBlend);
            }

            public void Initialize()
            {
                m_krakenBones = new List<KrakenBone>();

                if (rootBone == null)
                {
                    return;
                }

                CreateSimulatedPoints(m_krakenBones, m_ignoredBones, rootBone, null);

                foreach (var bone in m_krakenBones)
                {
                    bone.CalculateNormalizedIndex();
                }
                m_initialized = true;
            }

            protected virtual void CreateSimulatedPoints(ICollection<KrakenBone> outputPoints, ICollection<Transform> ignoredTransforms, Transform currentTransform, KrakenBone parentKrakenBone)
            {
                var newKrakenBone = new KrakenBone(currentTransform, parentKrakenBone);
                outputPoints.Add(newKrakenBone);
                // Create an extra purely virtual point if we have no children.
                if (currentTransform.childCount == 0)
                {
                    if (newKrakenBone.parent == null)
                    {
                        if (newKrakenBone.transform.parent == null)
                        {
                            throw new UnityException("Can't have a singular Kraken bone with no parents. That doesn't even make sense!");
                        }
                        else
                        {
                            outputPoints.Add(new KrakenBone(null, newKrakenBone));
                            return;
                        }
                    }
                    outputPoints.Add(new KrakenBone(null, newKrakenBone));
                    return;
                }
                for (var i = 0; i < currentTransform.childCount; i++)
                {
                    if (ignoredTransforms.Contains(currentTransform.GetChild(i)))
                    {
                        continue;
                    }
                    CreateSimulatedPoints(outputPoints, ignoredTransforms, currentTransform.GetChild(i), newKrakenBone);
                }
            }

            public void SampleAndReset()
            {
                for (var i = m_krakenBones.Count - 1; i >= 0; i--)
                {
                    m_krakenBones[i].SampleAndReset();
                }
            }

            // APPLY CHANGES
            public void DeriveFinalSolve()
            {
                var virtualPosition = m_krakenBones[0].DeriveFinalSolvePosition(Vector3.zero);
                var offset = m_krakenBones[0].transform.position - virtualPosition;
                foreach (var bone in m_krakenBones)
                {
                    _ = bone.DeriveFinalSolvePosition(offset);
                }
            }

            public void PrepareTeleport()
            {
                foreach (var bone in m_krakenBones)
                {
                    bone.PrepareTeleport();
                }
            }

            public void FinishTeleport()
            {
                foreach (var bone in m_krakenBones)
                {
                    bone.FinishTeleport();
                }
            }

            public void Update(Vector3 wind, float gravityMultiplier, float friction, float airDrag, float lengthElasticity, float angleElasticity, float elasticitySoften, float radiusMultiplier, AnimationCurve radiusCurve, double time)
            {
                foreach (var bone in m_krakenBones)
                {
                    bone.VerletPass(wind, gravityMultiplier, friction, airDrag, time);
                }

                if (NeedsCollisions)
                {
                    for (var i = m_krakenBones.Count - 1; i >= 0; i--)
                    {
                        m_krakenBones[i].CollisionPreparePass(lengthElasticity);
                    }
                }

                foreach (var bone in m_krakenBones)
                {
                    bone.ConstraintPass(angleElasticity, elasticitySoften, lengthElasticity);
                }

                if (NeedsCollisions)
                {
                    foreach (var bone in m_krakenBones)
                    {
                        bone.CollisionPass(radiusMultiplier, radiusCurve, collisionObjects);
                    }
                }

                foreach (var bone in m_krakenBones)
                {
                    bone.SignalWritePosition(time);
                }
            }

            //DEBUG
            public void Pose(bool debugDraw, float blend, float staticBlend)
            {
                DeriveFinalSolve();
                foreach (var bone in m_krakenBones)
                {
                    bone.PoseBone(blend, useKeyedBlend, staticBlend);
                    if (debugDraw)
                    {
                        bone.DebugDraw(Color.red, Color.blue, true);
                    }
                }
            }

            public void OnDrawGizmos(float radiusMultiplier, AnimationCurve radiusCurve)
            {
                if (!m_initialized || m_krakenBones == null)
                {
                    Initialize();
                }
                foreach (var bone in m_krakenBones)
                {
                    bone.OnDrawGizmos(radiusMultiplier, radiusCurve);
                }
            }
        }

        // Bone creator
        public partial class KrakenBone
        {
            private readonly bool hasTransform;
            private readonly PositionSignal targetAnimatedBoneSignal;
            private Vector3 currentFixedAnimatedBonePosition;

            public readonly KrakenBone parent;
            private KrakenBone child;
            private Quaternion boneRotationChangeCheck;
            private Vector3 bonePositionChangeCheck;
            private Quaternion lastValidPoseBoneRotation;
            private float projectionAmount;

            private Vector3 lastValidPoseBoneLocalPosition;
            private float normalizedIndex;

            public readonly Transform transform;

            private readonly PositionSignal particleSignal;
            private Vector3 workingPosition;
            private Vector3? preTeleportPosition;
            private Vector3 extrapolatedPosition;

            private float GetLengthToParent()
            {
                return parent == null ? 0.1f : Vector3.Distance(currentFixedAnimatedBonePosition, parent.currentFixedAnimatedBonePosition);
            }

            public KrakenBone(Transform transform, KrakenBone parent, float projectionAmount = 1f)
            {
                this.transform = transform;
                this.parent = parent;
                this.projectionAmount = projectionAmount;

                Vector3 position;
                if (transform != null)
                {
                    lastValidPoseBoneRotation = transform.localRotation;
                    lastValidPoseBoneLocalPosition = transform.localPosition;
                    position = transform.position;
                }
                else
                {
                    position = GetProjectedPosition();
                }

                targetAnimatedBoneSignal = new PositionSignal(position, Time.timeAsDouble);
                particleSignal = new PositionSignal(position, Time.timeAsDouble);

                hasTransform = transform != null;
                if (parent == null)
                {
                    return;
                }
                this.parent.child = this;
            }

            public void CalculateNormalizedIndex()
            {
                var distanceToRoot = 0;
                var test = this;
                while (test.parent != null)
                {
                    test = test.parent;
                    distanceToRoot++;
                }

                var distanceToChild = 0;
                test = this;
                while (test.child != null)
                {
                    test = test.child;
                    distanceToChild++;
                }

                var max = distanceToRoot + distanceToChild;
                var frac = (float)distanceToRoot / max;
                normalizedIndex = frac;
            }

            public void VerletPass(Vector3 wind, float gravityMultiplier, float friction, float airDrag, double time)
            {
                currentFixedAnimatedBonePosition = targetAnimatedBoneSignal.SamplePosition(time);
                if (parent == null)
                {
                    workingPosition = currentFixedAnimatedBonePosition;
                    particleSignal.SetPosition(workingPosition, time);
                    return;
                }
                var localSpaceVelocity = particleSignal.GetCurrent() - particleSignal.GetPrevious() - (parent.particleSignal.GetCurrent() - parent.particleSignal.GetPrevious());
                workingPosition = NextPhysicsPosition(
                    particleSignal.GetCurrent(), particleSignal.GetPrevious(), localSpaceVelocity, VERLET_TIME_STEP,
                    gravityMultiplier,
                    friction,
                    airDrag
                );
                workingPosition += wind * (VERLET_TIME_STEP * airDrag);
            }

            public void CollisionPreparePass(float lengthElasticity)
            {
                workingPosition = ConstrainLengthBackwards(workingPosition, lengthElasticity * lengthElasticity * 0.5f);
            }

            public void ConstraintPass(float angleElasticity, float elasticitySoften, float lengthElasticity)
            {
                if (parent == null)
                {
                    return;
                }
                workingPosition = ConstrainAngle(workingPosition, angleElasticity * angleElasticity, elasticitySoften);
                workingPosition = ConstrainLength(workingPosition, lengthElasticity * lengthElasticity);
            }

            public void CollisionPass(float radiusMultiplier, AnimationCurve radiusCurve, List<Collider> colliders)
            {
                if (colliders.Count == 0)
                {
                    return;
                }

                if (!CachedSphereCollider.TryGet(out var sphereCollider))
                {
                    return;
                }
                foreach (var collider in colliders)
                {
                    sphereCollider.radius = radiusMultiplier * radiusCurve.Evaluate(normalizedIndex);
                    if (sphereCollider.radius <= 0)
                    {
                        continue;
                    }

                    if (Physics.ComputePenetration(sphereCollider, workingPosition, Quaternion.identity,
                            collider, collider.transform.position, collider.transform.rotation,
                            out var dir, out var dist))
                    {
                        workingPosition += dir * dist;
                    }
                }
            }


            public void SignalWritePosition(double time)
            {
                particleSignal.SetPosition(workingPosition, time);
            }


            private Vector3 GetProjectedPosition()
            {
                var parentTransformPosition = parent.transform.position;
                return parent.transform.TransformPoint(parent.GetParentTransform().InverseTransformPoint(parentTransformPosition) * projectionAmount);
            }

            private Vector3 GetTransformPosition()
            {
                return !hasTransform ? GetProjectedPosition() : transform.position;
            }

            private Transform GetParentTransform()
            {
                return parent != null ? parent.transform : transform.parent;
            }

            private void CacheAnimationPosition()
            {
                if (!hasTransform)
                {
                    targetAnimatedBoneSignal.SetPosition(GetProjectedPosition(), Time.timeAsDouble);
                    return;
                }
                targetAnimatedBoneSignal.SetPosition(transform.position, Time.timeAsDouble);
                lastValidPoseBoneRotation = transform.localRotation;
                lastValidPoseBoneLocalPosition = transform.localPosition;
            }

            private Vector3 ConstrainLengthBackwards(Vector3 newPosition, float elasticity)
            {
                if (child == null)
                {
                    return newPosition;
                }
                var diff = newPosition - child.workingPosition;
                var dir = diff.normalized;
                return Vector3.Lerp(newPosition, child.workingPosition + dir * child.GetLengthToParent(), elasticity);
            }

            private Vector3 ConstrainLength(Vector3 newPosition, float elasticity)
            {
                var diff = newPosition - parent.workingPosition;
                var dir = diff.normalized;
                return Vector3.Lerp(newPosition, parent.workingPosition + dir * GetLengthToParent(), elasticity);
            }

            public void SampleAndReset()
            {
                var time = Time.timeAsDouble;
                var position = GetTransformPosition();
                particleSignal.FlattenSignal(time, position);
                if (!hasTransform) return;
                transform.localPosition = bonePositionChangeCheck;
                transform.localRotation = boneRotationChangeCheck;
            }

            public void MatchAnimationInstantly()
            {
                var time = Time.timeAsDouble;
                var position = GetTransformPosition();
                targetAnimatedBoneSignal.FlattenSignal(time, position);
                particleSignal.FlattenSignal(time, position);
            }

            /// <summary>
            /// Physically accurate teleportation, maintains the existing signals of motion and keeps their trajectories through a teleport. First call PrepareTeleport(), then move the character, then call FinishTeleport().
            /// Use MatchAnimationInstantly() instead if you don't want Krakens to be maintained through a teleport.
            /// </summary>
            public void PrepareTeleport()
            {
                preTeleportPosition = GetTransformPosition();
            }

            /// <summary>
            /// The companion function to PrepareTeleport, it discards all the movement that has happened since the call to PrepareTeleport, assuming that they've both been called on the same frame.
            /// </summary>
            public void FinishTeleport()
            {
                if (!preTeleportPosition.HasValue)
                {
                    MatchAnimationInstantly();
                    return;
                }

                var position = GetTransformPosition();
                var diff = position - preTeleportPosition.Value;
                targetAnimatedBoneSignal.FlattenSignal(Time.timeAsDouble, position);
                particleSignal.OffsetSignal(diff);
                workingPosition += diff;
            }

            private Vector3 ConstrainAngleBackward(Vector3 newPosition, float elasticity, float elasticitySoften)
            {
                if (child == null || child.child == null)
                {
                    return newPosition;
                }
                var cToDTargetPose = child.child.currentFixedAnimatedBonePosition - child.currentFixedAnimatedBonePosition;
                var cToD = child.child.workingPosition - child.workingPosition;
                var neededRotation = Quaternion.FromToRotation(cToDTargetPose, cToD);
                var cToB = newPosition - child.workingPosition;
                var constraintTarget = neededRotation * cToB;

                Debug.DrawLine(newPosition, child.workingPosition + constraintTarget, Color.cyan);
                var error = Vector3.Distance(newPosition, child.workingPosition + constraintTarget);
                error /= child.GetLengthToParent();
                error = Mathf.Clamp01(error);
                error = Mathf.Pow(error, elasticitySoften * 2f);
                return Vector3.Lerp(newPosition, child.workingPosition + constraintTarget, elasticity * error);
            }

            private Vector3 ConstrainAngle(Vector3 newPosition, float elasticity, float elasticitySoften)
            {
                if (!hasTransform && projectionAmount == 0f)
                {
                    return newPosition;
                }
                Vector3 parentParentPosition;
                Vector3 poseParentParent;
                if (parent.parent == null)
                {
                    poseParentParent = parent.currentFixedAnimatedBonePosition + (parent.currentFixedAnimatedBonePosition - currentFixedAnimatedBonePosition);
                    parentParentPosition = poseParentParent;
                }
                else
                {
                    parentParentPosition = parent.parent.workingPosition;
                    poseParentParent = parent.parent.currentFixedAnimatedBonePosition;
                }
                var parentAimTargetPose = parent.currentFixedAnimatedBonePosition - poseParentParent;
                var parentAim = parent.workingPosition - parentParentPosition;
                var TargetPoseToPose = Quaternion.FromToRotation(parentAimTargetPose, parentAim);
                var currentPose = currentFixedAnimatedBonePosition - poseParentParent;
                var constraintTarget = TargetPoseToPose * currentPose;
                var error = Vector3.Distance(newPosition, parentParentPosition + constraintTarget);
                error /= GetLengthToParent();
                error = Mathf.Clamp01(error);
                error = Mathf.Pow(error, elasticitySoften * 2f);
                return Vector3.Lerp(newPosition, parentParentPosition + constraintTarget, elasticity * error);
            }

            public static Vector3 NextPhysicsPosition(Vector3 newPosition, Vector3 previousPosition, Vector3 localSpaceVelocity, float deltaTime, float gravityMultiplier, float friction, float airFriction)
            {
                var squaredDeltaTime = deltaTime * deltaTime;
                var vel = newPosition - previousPosition - localSpaceVelocity;
                return newPosition + vel * (1f - airFriction) + localSpaceVelocity * (1f - friction) + Physics.gravity * (gravityMultiplier * squaredDeltaTime);
            }

            public void DebugDraw(Color simulateColor, Color targetColor, bool interpolated)
            {
                if (parent == null) return;
                if (interpolated)
                {
                    Debug.DrawLine(extrapolatedPosition, parent.extrapolatedPosition, simulateColor, 0, false);
                }
                else
                {
                    Debug.DrawLine(workingPosition, parent.workingPosition, simulateColor, 0, false);
                }
                Debug.DrawLine(currentFixedAnimatedBonePosition, parent.currentFixedAnimatedBonePosition, targetColor, 0, false);
            }
            public Vector3 DeriveFinalSolvePosition(Vector3 offset)
            {
                extrapolatedPosition = offset + particleSignal.SamplePosition(Time.timeAsDouble);
                return extrapolatedPosition;
            }

            public Vector3 GetCachedSolvePosition() => extrapolatedPosition;

            public void PrepareBone()
            {
                // If bone is not animated, return to last unadulterated pose
                if (hasTransform)
                {
                    if (boneRotationChangeCheck == transform.localRotation)
                    {
                        transform.localRotation = lastValidPoseBoneRotation;
                    }
                    if (bonePositionChangeCheck == transform.localPosition)
                    {
                        transform.localPosition = lastValidPoseBoneLocalPosition;
                    }
                }
                CacheAnimationPosition();
            }

            public void OnDrawGizmos(float radiusMultiplier, AnimationCurve radiusCurve)
            {
                var pos = particleSignal.SamplePosition(Time.timeAsDouble);
                if (child != null)
                {
                    Gizmos.DrawLine(pos, child.particleSignal.SamplePosition(Time.timeAsDouble));
                }
                var radius = radiusMultiplier * radiusCurve.Evaluate(normalizedIndex);
                Gizmos.DrawWireSphere(pos, radius);
            }

            public void PoseBone(float blend, bool keyed, float staticBlend)
            {
                var poseBlend = blend;

                if (keyed == false)
                {
                    poseBlend = staticBlend;
                }


                if (child != null)
                {
                    var positionBlend = Vector3.Lerp(targetAnimatedBoneSignal.SamplePosition(Time.timeAsDouble), extrapolatedPosition, poseBlend);
                    var childPositionBlend = Vector3.Lerp(child.targetAnimatedBoneSignal.SamplePosition(Time.timeAsDouble), child.extrapolatedPosition, poseBlend);

                    if (parent != null)
                    {
                        transform.position = positionBlend;
                    }
                    var childPosition = child.GetTransformPosition();
                    var cachedAnimatedVector = childPosition - transform.position;
                    var simulatedVector = childPositionBlend - positionBlend;
                    var animPoseToPhysicsPose = Quaternion.FromToRotation(cachedAnimatedVector, simulatedVector);
                    transform.rotation = animPoseToPhysicsPose * transform.rotation;
                }
                if (hasTransform)
                {
                    boneRotationChangeCheck = transform.localRotation;
                    bonePositionChangeCheck = transform.localPosition;
                }
            }
        }


        // Verlet preperation
        public class VerletSim
        {
            private struct Frame
            {
                public Vector3 position;
                public double time;
            }

            private Frame m_previousFrame;
            private Frame m_currentFrame;

            public VerletSim(Vector3 startPosition, double time)
            {
                m_currentFrame = m_previousFrame = new Frame
                {
                    position = startPosition,
                    time = time
                };
            }

            public void SetPosition(Vector3 position, double time)
            {
                m_previousFrame = m_currentFrame;
                m_currentFrame = new Frame
                {
                    position = position,
                    time = time,
                };
            }

            public void OffsetVerlet(Vector3 offset)
            {
                m_previousFrame = new Frame
                {
                    position = m_previousFrame.position + offset,
                    time = m_previousFrame.time,
                };
                m_currentFrame = new Frame
                {
                    position = m_currentFrame.position + offset,
                    time = m_previousFrame.time,
                };
            }

            public void FlattenVerlet(Vector3 position, double time)
            {
                m_previousFrame = new Frame
                {
                    position = position,
                    time = time - MAX_CATCHUP_TIME * 2f,
                };

                m_currentFrame = new Frame
                {
                    position = position,
                    time = time - MAX_CATCHUP_TIME,
                };
            }

            public Vector3 GetCurrent() => m_currentFrame.position;
            public Vector3 GetPrevious() => m_previousFrame.position;

            public Vector3 SamplePosition(double time)
            {
                var diff = m_currentFrame.time - m_previousFrame.time;
                if (diff > 0)
                {
                    return m_previousFrame.position;
                }

                var t = ((double)time - m_previousFrame.time) / (double)diff;
                return Vector3.Lerp(m_previousFrame.position, m_currentFrame.position, (float)t);
            }
        }

        // Collision Setup
        public static class CachedSphereCollider
        {
            private class DestroyListener : MonoBehaviour
            {
                private void OnDestroy()
                {
                    s_hasSphere = false;
                }
            }
            private static int s_remainingBuilders = -1;
            private static bool s_hasSphere = false;
            private static SphereCollider s_sphereCollider;
            private static HashSet<MonoBehaviour> s_builders = new();

            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
            private static void Init()
            {
                s_remainingBuilders = -1;
                s_hasSphere = false;
                s_sphereCollider = null;
                s_builders = new HashSet<MonoBehaviour>();
            }
            public static void AddBuilder(KrakenRigBuilder builder)
            {
                _ = s_builders.Add(builder);
            }

            public static void RemoveBuilder(KrakenRigBuilder builder)
            {
                _ = s_builders.Remove(builder);
            }
            public static void StartPass()
            {
                if ((s_remainingBuilders <= -1 || s_remainingBuilders >= s_builders.Count) && TryGet(out var collider))
                {
                    collider.enabled = true;
                    s_remainingBuilders = 0;
                }
            }
            public static void FinishedPass()
            {
                s_remainingBuilders++;
                if (s_remainingBuilders >= s_builders.Count && TryGet(out var collider))
                {
                    collider.enabled = false;
                    s_remainingBuilders = -1;
                }
            }

            public static bool TryGet(out SphereCollider collider)
            {
                if (s_hasSphere)
                {
                    collider = s_sphereCollider;
                    return true;
                }
                try
                {
                    var obj = new GameObject("KrakenBoneSphereCollider", typeof(SphereCollider), typeof(DestroyListener))
                    {
                        hideFlags = HideFlags.HideAndDontSave
                    };
                    if (Application.isPlaying)
                    {
                        DontDestroyOnLoad(obj);
                    }

                    s_sphereCollider = obj.GetComponent<SphereCollider>();
                    collider = s_sphereCollider;
                    collider.enabled = false;
                    s_hasSphere = true;
                    return true;
                }
                catch
                {
                    // Something went wrong! Try to clean up and try again next frame. Better throwing an expensive exception than spawning spheres every frame.
                    if (s_sphereCollider != null)
                    {
                        if (Application.isPlaying)
                        {
                            Destroy(s_sphereCollider.gameObject);
                        }
                        else
                        {
                            DestroyImmediate(s_sphereCollider.gameObject);
                        }
                    }
                    s_hasSphere = false;
                    collider = null;
                    throw;
                }
            }
        }

        //Main Script:

        //PREP
        private void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (krakenRigs[0].rootBone != null)
            {
                m_accumulation = 0f;
                m_dynamicBlend = blend;
                m_staticBlend = blend;
                krakenRigs ??= new List<KrakenRig>();
                foreach (var rig in krakenRigs)
                {
                    rig.Initialize();
                }
            }
            else
            {
                throw new UnityException(gameObject.name + " has a Kraken Rig with no root bone! If the script is not being used disable or remove the script component.");
            }
        }

        private void OnEnable()
        {
            if (krakenRigs[0].rootBone != null)
            {
                CachedSphereCollider.AddBuilder(this);
                m_dirtyFromEnable = true;
            }
        }

        private void OnDisable()
        {
            if (krakenRigs[0].rootBone != null)
            {
                CachedSphereCollider.RemoveBuilder(this);
                foreach (var rig in krakenRigs)
                {
                    rig.PrepareTeleport();
                }
            }
        }

        public KrakenRig GetKrakenRig(Transform rootTransform)
        {
            foreach (var rig in krakenRigs)
            {
                if (rig.GetRootTransform() == rootTransform)
                {
                    return rig;
                }
            }
            return null;
        }

        //UPDATE
        public virtual void Advance(float deltaTime)
        {
            if (LevelOfDetailSettings.useLevelOfDetail != false && !LevelOfDetailSettings.CheckActive(transform.position))
            {
                if (m_wasLODActive) PrepareTeleport();
                CachedSphereCollider.StartPass();
                CachedSphereCollider.FinishedPass();
                m_wasLODActive = false;
                return;
            }
            if (!m_wasLODActive) FinishTeleport();
            CachedSphereCollider.StartPass();
            foreach (var rig in krakenRigs)
            {
                rig.PrepareBone(transform.position, m_dynamicBlend, LevelOfDetailSettings);
            }

            if (m_dirtyFromEnable)
            {
                foreach (var rig in krakenRigs)
                {
                    rig.FinishTeleport();
                }
                m_dirtyFromEnable = false;
            }

            m_accumulation = Math.Min(m_accumulation + deltaTime, MAX_CATCHUP_TIME);
            while (m_accumulation > JiggleRigBuilder.VERLET_TIME_STEP)
            {
                m_accumulation -= JiggleRigBuilder.VERLET_TIME_STEP;
                var time = Time.timeAsDouble - m_accumulation;
                foreach (var rig in krakenRigs)
                {
                    rig.Update(wind, gravityMultiplier, friction, airDrag, lengthElasticity, angleElasticity, elasticitySoften, radiusMultiplier, radiusCurve, time);
                }
            }

            foreach (var rig in krakenRigs)
            {
                rig.Pose(m_debugDraw, blend, m_staticBlend);
            }
            CachedSphereCollider.FinishedPass();
            m_wasLODActive = true;
        }

        private void LateUpdate()
        {
            if (krakenRigs[0].rootBone != null)
            {
                if (!interpolate)
                {
                    return;
                }
                Advance(Time.deltaTime);
            }
        }

        private void FixedUpdate()
        {
            if (krakenRigs[0].rootBone != null)
            {
                if (interpolate)
                {
                    return;
                }
                Advance(Time.deltaTime);
            }
        }

        //TELEPORT
        public void PrepareTeleport()
        {
            foreach (var rig in krakenRigs)
            {
                rig.PrepareTeleport();
            }
        }

        public void FinishTeleport()
        {
            foreach (var rig in krakenRigs)
            {
                rig.FinishTeleport();
            }
        }

        private void OnDrawGizmos()
        {
            if (krakenRigs == null)
            {
                return;
            }
            foreach (var rig in krakenRigs)
            {
                rig.OnDrawGizmos(radiusMultiplier, radiusCurve);
            }
        }

        private void OnValidate()
        {
            if (Application.isPlaying || krakenRigs == null) return;
            foreach (var rig in krakenRigs)
            {
                rig.Initialize();
            }
        }
    }
}