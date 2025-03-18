// Copyright (c) Meta Platforms, Inc. and affiliates.

using System;
using System.Collections.Generic;
using JigglePhysics;
using UnityEditor;
using UnityEngine;

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
        public bool Interpolate = true;
        protected List<KrakenBone> m_krakenBones;
        public List<KrakenRig> KrakenRigs;
        public LevelOfDetailValues LevelOfDetailSettings;
        private bool m_debugDraw;

        [SerializeField]
        [Tooltip("An air force that is applied to the entire rig, this is useful to plug in some wind volumes from external sources.")]
        public Vector3 Wind;
        [Range(0f, 2f)]
        public float GravityMultiplier = 1f;
        [Range(0f, 1f)]
        public float Friction = 0.5f;
        [Range(0f, 1f)]
        public float Elasticity;
        [Range(0f, 1f)]
        public float AngleElasticity = 0.5f;
        [Range(0f, 1f)]
        public float Blend = 0.9f;
        [Range(0f, 1f)]
        public float AirDrag = 0.4f;
        [Range(0f, 1f)]
        public float LengthElasticity = 0.6f;
        [Range(0f, 1f)]
        public float ElasticitySoften = 0.5f;
        [Range(0f, 2f)]
        public float RadiusMultiplier = 1;
        public AnimationCurve RadiusCurve = new(new Keyframe(0f, 1f), new Keyframe(1f, 0f));

        // LOD values dropdown
        [Serializable]
        public class LevelOfDetailValues
        {
            [SerializeField]
            public bool UseLevelOfDetail = false;
            [Tooltip("The distance from the camera at which the simulation is disabled.")]
            public float Distance;
            public float Blend;

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
                    if (UseLevelOfDetail)
                    {
                        var newBlend = (Vector3.Distance(camera.transform.position, position) - Distance + Blend) / Blend;
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
            public Transform RootBone;
            [SerializeField]
            [Tooltip("The list of bones to ignore during the simulation. Each bone listed will also ignore all the children of the specified bone.")]
            private List<Transform> m_ignoredBones;
            public List<Collider> CollisionObjects;
            public bool UseKeyedBlend = true;

            public Transform GetRootTransform() => RootBone;
            public KrakenRig(Transform rootTransform,
                ICollection<Transform> ignoredBones, ICollection<Collider> collisionObjects)
            {
                RootBone = rootTransform;
                m_ignoredBones = new List<Transform>(ignoredBones);
                CollisionObjects = new List<Collider>(collisionObjects);
                Initialize();
            }

            // Private
            private bool m_initialized;
            private bool NeedsCollisions => CollisionObjects.Count != 0;

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

                if (RootBone == null)
                {
                    return;
                }

                CreateSimulatedPoints(m_krakenBones, m_ignoredBones, RootBone, null);

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
                    if (newKrakenBone.Parent == null)
                    {
                        if (newKrakenBone.Transform.parent == null)
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
                var offset = m_krakenBones[0].Transform.position - virtualPosition;
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
                        bone.CollisionPass(radiusMultiplier, radiusCurve, CollisionObjects);
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
                    bone.PoseBone(blend, UseKeyedBlend, staticBlend);
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
            private readonly bool m_hasTransform;
            private readonly PositionSignal m_targetAnimatedBoneSignal;
            private Vector3 m_currentFixedAnimatedBonePosition;

            public readonly KrakenBone Parent;
            private KrakenBone m_child;
            private Quaternion m_boneRotationChangeCheck;
            private Vector3 m_bonePositionChangeCheck;
            private Quaternion m_lastValidPoseBoneRotation;
            private float m_projectionAmount;

            private Vector3 m_lastValidPoseBoneLocalPosition;
            private float m_normalizedIndex;

            public readonly Transform Transform;

            private readonly PositionSignal m_particleSignal;
            private Vector3 m_workingPosition;
            private Vector3? m_preTeleportPosition;
            private Vector3 m_extrapolatedPosition;

            private float GetLengthToParent()
            {
                return Parent == null ? 0.1f : Vector3.Distance(m_currentFixedAnimatedBonePosition, Parent.m_currentFixedAnimatedBonePosition);
            }

            public KrakenBone(Transform transform, KrakenBone parent, float projectionAmount = 1f)
            {
                Transform = transform;
                Parent = parent;
                m_projectionAmount = projectionAmount;

                Vector3 position;
                if (transform != null)
                {
                    m_lastValidPoseBoneRotation = transform.localRotation;
                    m_lastValidPoseBoneLocalPosition = transform.localPosition;
                    position = transform.position;
                }
                else
                {
                    position = GetProjectedPosition();
                }

                m_targetAnimatedBoneSignal = new PositionSignal(position, Time.timeAsDouble);
                m_particleSignal = new PositionSignal(position, Time.timeAsDouble);

                m_hasTransform = transform != null;
                if (parent == null)
                {
                    return;
                }
                Parent.m_child = this;
            }

            public void CalculateNormalizedIndex()
            {
                var distanceToRoot = 0;
                var test = this;
                while (test.Parent != null)
                {
                    test = test.Parent;
                    distanceToRoot++;
                }

                var distanceToChild = 0;
                test = this;
                while (test.m_child != null)
                {
                    test = test.m_child;
                    distanceToChild++;
                }

                var max = distanceToRoot + distanceToChild;
                var frac = (float)distanceToRoot / max;
                m_normalizedIndex = frac;
            }

            public void VerletPass(Vector3 wind, float gravityMultiplier, float friction, float airDrag, double time)
            {
                m_currentFixedAnimatedBonePosition = m_targetAnimatedBoneSignal.SamplePosition(time);
                if (Parent == null)
                {
                    m_workingPosition = m_currentFixedAnimatedBonePosition;
                    m_particleSignal.SetPosition(m_workingPosition, time);
                    return;
                }
                var localSpaceVelocity = m_particleSignal.GetCurrent() - m_particleSignal.GetPrevious() - (Parent.m_particleSignal.GetCurrent() - Parent.m_particleSignal.GetPrevious());
                m_workingPosition = NextPhysicsPosition(
                    m_particleSignal.GetCurrent(), m_particleSignal.GetPrevious(), localSpaceVelocity, VERLET_TIME_STEP,
                    gravityMultiplier,
                    friction,
                    airDrag
                );
                m_workingPosition += wind * (VERLET_TIME_STEP * airDrag);
            }

            public void CollisionPreparePass(float lengthElasticity)
            {
                m_workingPosition = ConstrainLengthBackwards(m_workingPosition, lengthElasticity * lengthElasticity * 0.5f);
            }

            public void ConstraintPass(float angleElasticity, float elasticitySoften, float lengthElasticity)
            {
                if (Parent == null)
                {
                    return;
                }
                m_workingPosition = ConstrainAngle(m_workingPosition, angleElasticity * angleElasticity, elasticitySoften);
                m_workingPosition = ConstrainLength(m_workingPosition, lengthElasticity * lengthElasticity);
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
                    sphereCollider.radius = radiusMultiplier * radiusCurve.Evaluate(m_normalizedIndex);
                    if (sphereCollider.radius <= 0)
                    {
                        continue;
                    }

                    if (Physics.ComputePenetration(sphereCollider, m_workingPosition, Quaternion.identity,
                            collider, collider.transform.position, collider.transform.rotation,
                            out var dir, out var dist))
                    {
                        m_workingPosition += dir * dist;
                    }
                }
            }


            public void SignalWritePosition(double time)
            {
                m_particleSignal.SetPosition(m_workingPosition, time);
            }


            private Vector3 GetProjectedPosition()
            {
                var parentTransformPosition = Parent.Transform.position;
                return Parent.Transform.TransformPoint(Parent.GetParentTransform().InverseTransformPoint(parentTransformPosition) * m_projectionAmount);
            }

            private Vector3 GetTransformPosition()
            {
                return !m_hasTransform ? GetProjectedPosition() : Transform.position;
            }

            private Transform GetParentTransform()
            {
                return Parent != null ? Parent.Transform : Transform.parent;
            }

            private void CacheAnimationPosition()
            {
                if (!m_hasTransform)
                {
                    m_targetAnimatedBoneSignal.SetPosition(GetProjectedPosition(), Time.timeAsDouble);
                    return;
                }
                m_targetAnimatedBoneSignal.SetPosition(Transform.position, Time.timeAsDouble);
                m_lastValidPoseBoneRotation = Transform.localRotation;
                m_lastValidPoseBoneLocalPosition = Transform.localPosition;
            }

            private Vector3 ConstrainLengthBackwards(Vector3 newPosition, float elasticity)
            {
                if (m_child == null)
                {
                    return newPosition;
                }
                var diff = newPosition - m_child.m_workingPosition;
                var dir = diff.normalized;
                return Vector3.Lerp(newPosition, m_child.m_workingPosition + dir * m_child.GetLengthToParent(), elasticity);
            }

            private Vector3 ConstrainLength(Vector3 newPosition, float elasticity)
            {
                var diff = newPosition - Parent.m_workingPosition;
                var dir = diff.normalized;
                return Vector3.Lerp(newPosition, Parent.m_workingPosition + dir * GetLengthToParent(), elasticity);
            }

            public void SampleAndReset()
            {
                var time = Time.timeAsDouble;
                var position = GetTransformPosition();
                m_particleSignal.FlattenSignal(time, position);
                if (!m_hasTransform) return;
                Transform.localPosition = m_bonePositionChangeCheck;
                Transform.localRotation = m_boneRotationChangeCheck;
            }

            public void MatchAnimationInstantly()
            {
                var time = Time.timeAsDouble;
                var position = GetTransformPosition();
                m_targetAnimatedBoneSignal.FlattenSignal(time, position);
                m_particleSignal.FlattenSignal(time, position);
            }

            /// <summary>
            /// Physically accurate teleportation, maintains the existing signals of motion and keeps their trajectories through a teleport. First call PrepareTeleport(), then move the character, then call FinishTeleport().
            /// Use MatchAnimationInstantly() instead if you don't want Krakens to be maintained through a teleport.
            /// </summary>
            public void PrepareTeleport()
            {
                m_preTeleportPosition = GetTransformPosition();
            }

            /// <summary>
            /// The companion function to PrepareTeleport, it discards all the movement that has happened since the call to PrepareTeleport, assuming that they've both been called on the same frame.
            /// </summary>
            public void FinishTeleport()
            {
                if (!m_preTeleportPosition.HasValue)
                {
                    MatchAnimationInstantly();
                    return;
                }

                var position = GetTransformPosition();
                var diff = position - m_preTeleportPosition.Value;
                m_targetAnimatedBoneSignal.FlattenSignal(Time.timeAsDouble, position);
                m_particleSignal.OffsetSignal(diff);
                m_workingPosition += diff;
            }

            private Vector3 ConstrainAngleBackward(Vector3 newPosition, float elasticity, float elasticitySoften)
            {
                if (m_child == null || m_child.m_child == null)
                {
                    return newPosition;
                }
                var cToDTargetPose = m_child.m_child.m_currentFixedAnimatedBonePosition - m_child.m_currentFixedAnimatedBonePosition;
                var cToD = m_child.m_child.m_workingPosition - m_child.m_workingPosition;
                var neededRotation = Quaternion.FromToRotation(cToDTargetPose, cToD);
                var cToB = newPosition - m_child.m_workingPosition;
                var constraintTarget = neededRotation * cToB;

                Debug.DrawLine(newPosition, m_child.m_workingPosition + constraintTarget, Color.cyan);
                var error = Vector3.Distance(newPosition, m_child.m_workingPosition + constraintTarget);
                error /= m_child.GetLengthToParent();
                error = Mathf.Clamp01(error);
                error = Mathf.Pow(error, elasticitySoften * 2f);
                return Vector3.Lerp(newPosition, m_child.m_workingPosition + constraintTarget, elasticity * error);
            }

            private Vector3 ConstrainAngle(Vector3 newPosition, float elasticity, float elasticitySoften)
            {
                if (!m_hasTransform && m_projectionAmount == 0f)
                {
                    return newPosition;
                }
                Vector3 parentParentPosition;
                Vector3 poseParentParent;
                if (Parent.Parent == null)
                {
                    poseParentParent = Parent.m_currentFixedAnimatedBonePosition + (Parent.m_currentFixedAnimatedBonePosition - m_currentFixedAnimatedBonePosition);
                    parentParentPosition = poseParentParent;
                }
                else
                {
                    parentParentPosition = Parent.Parent.m_workingPosition;
                    poseParentParent = Parent.Parent.m_currentFixedAnimatedBonePosition;
                }
                var parentAimTargetPose = Parent.m_currentFixedAnimatedBonePosition - poseParentParent;
                var parentAim = Parent.m_workingPosition - parentParentPosition;
                var targetPoseToPose = Quaternion.FromToRotation(parentAimTargetPose, parentAim);
                var currentPose = m_currentFixedAnimatedBonePosition - poseParentParent;
                var constraintTarget = targetPoseToPose * currentPose;
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
                if (Parent == null) return;
                if (interpolated)
                {
                    Debug.DrawLine(m_extrapolatedPosition, Parent.m_extrapolatedPosition, simulateColor, 0, false);
                }
                else
                {
                    Debug.DrawLine(m_workingPosition, Parent.m_workingPosition, simulateColor, 0, false);
                }
                Debug.DrawLine(m_currentFixedAnimatedBonePosition, Parent.m_currentFixedAnimatedBonePosition, targetColor, 0, false);
            }
            public Vector3 DeriveFinalSolvePosition(Vector3 offset)
            {
                m_extrapolatedPosition = offset + m_particleSignal.SamplePosition(Time.timeAsDouble);
                return m_extrapolatedPosition;
            }

            public Vector3 GetCachedSolvePosition() => m_extrapolatedPosition;

            public void PrepareBone()
            {
                // If bone is not animated, return to last unadulterated pose
                if (m_hasTransform)
                {
                    if (m_boneRotationChangeCheck == Transform.localRotation)
                    {
                        Transform.localRotation = m_lastValidPoseBoneRotation;
                    }
                    if (m_bonePositionChangeCheck == Transform.localPosition)
                    {
                        Transform.localPosition = m_lastValidPoseBoneLocalPosition;
                    }
                }
                CacheAnimationPosition();
            }

            public void OnDrawGizmos(float radiusMultiplier, AnimationCurve radiusCurve)
            {
                var pos = m_particleSignal.SamplePosition(Time.timeAsDouble);
                if (m_child != null)
                {
                    Gizmos.DrawLine(pos, m_child.m_particleSignal.SamplePosition(Time.timeAsDouble));
                }
                var radius = radiusMultiplier * radiusCurve.Evaluate(m_normalizedIndex);
                Gizmos.DrawWireSphere(pos, radius);
            }

            public void PoseBone(float blend, bool keyed, float staticBlend)
            {
                var poseBlend = blend;

                if (!keyed)
                {
                    poseBlend = staticBlend;
                }


                if (m_child != null)
                {
                    var positionBlend = Vector3.Lerp(m_targetAnimatedBoneSignal.SamplePosition(Time.timeAsDouble), m_extrapolatedPosition, poseBlend);
                    var childPositionBlend = Vector3.Lerp(m_child.m_targetAnimatedBoneSignal.SamplePosition(Time.timeAsDouble), m_child.m_extrapolatedPosition, poseBlend);

                    if (Parent != null)
                    {
                        Transform.position = positionBlend;
                    }
                    var childPosition = m_child.GetTransformPosition();
                    var cachedAnimatedVector = childPosition - Transform.position;
                    var simulatedVector = childPositionBlend - positionBlend;
                    var animPoseToPhysicsPose = Quaternion.FromToRotation(cachedAnimatedVector, simulatedVector);
                    Transform.rotation = animPoseToPhysicsPose * Transform.rotation;
                }
                if (m_hasTransform)
                {
                    m_boneRotationChangeCheck = Transform.localRotation;
                    m_bonePositionChangeCheck = Transform.localPosition;
                }
            }
        }


        // Verlet preperation
        public class VerletSim
        {
            private struct Frame
            {
                public Vector3 Position;
                public double Time;
            }

            private Frame m_previousFrame;
            private Frame m_currentFrame;

            public VerletSim(Vector3 startPosition, double time)
            {
                m_currentFrame = m_previousFrame = new Frame
                {
                    Position = startPosition,
                    Time = time
                };
            }

            public void SetPosition(Vector3 position, double time)
            {
                m_previousFrame = m_currentFrame;
                m_currentFrame = new Frame
                {
                    Position = position,
                    Time = time,
                };
            }

            public void OffsetVerlet(Vector3 offset)
            {
                m_previousFrame = new Frame
                {
                    Position = m_previousFrame.Position + offset,
                    Time = m_previousFrame.Time,
                };
                m_currentFrame = new Frame
                {
                    Position = m_currentFrame.Position + offset,
                    Time = m_previousFrame.Time,
                };
            }

            public void FlattenVerlet(Vector3 position, double time)
            {
                m_previousFrame = new Frame
                {
                    Position = position,
                    Time = time - MAX_CATCHUP_TIME * 2f,
                };

                m_currentFrame = new Frame
                {
                    Position = position,
                    Time = time - MAX_CATCHUP_TIME,
                };
            }

            public Vector3 GetCurrent() => m_currentFrame.Position;
            public Vector3 GetPrevious() => m_previousFrame.Position;

            public Vector3 SamplePosition(double time)
            {
                var diff = m_currentFrame.Time - m_previousFrame.Time;
                if (diff > 0)
                {
                    return m_previousFrame.Position;
                }

                var t = ((double)time - m_previousFrame.Time) / (double)diff;
                return Vector3.Lerp(m_previousFrame.Position, m_currentFrame.Position, (float)t);
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
            if (KrakenRigs[0].RootBone != null)
            {
                m_accumulation = 0f;
                m_dynamicBlend = Blend;
                m_staticBlend = Blend;
                KrakenRigs ??= new List<KrakenRig>();
                foreach (var rig in KrakenRigs)
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
            if (KrakenRigs[0].RootBone != null)
            {
                CachedSphereCollider.AddBuilder(this);
                m_dirtyFromEnable = true;
            }
        }

        private void OnDisable()
        {
            if (KrakenRigs[0].RootBone != null)
            {
                CachedSphereCollider.RemoveBuilder(this);
                foreach (var rig in KrakenRigs)
                {
                    rig.PrepareTeleport();
                }
            }
        }

        public KrakenRig GetKrakenRig(Transform rootTransform)
        {
            foreach (var rig in KrakenRigs)
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
            if (LevelOfDetailSettings.UseLevelOfDetail && !LevelOfDetailSettings.CheckActive(transform.position))
            {
                if (m_wasLODActive) PrepareTeleport();
                CachedSphereCollider.StartPass();
                CachedSphereCollider.FinishedPass();
                m_wasLODActive = false;
                return;
            }
            if (!m_wasLODActive) FinishTeleport();
            CachedSphereCollider.StartPass();
            foreach (var rig in KrakenRigs)
            {
                rig.PrepareBone(transform.position, m_dynamicBlend, LevelOfDetailSettings);
            }

            if (m_dirtyFromEnable)
            {
                foreach (var rig in KrakenRigs)
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
                foreach (var rig in KrakenRigs)
                {
                    rig.Update(Wind, GravityMultiplier, Friction, AirDrag, LengthElasticity, AngleElasticity, ElasticitySoften, RadiusMultiplier, RadiusCurve, time);
                }
            }

            foreach (var rig in KrakenRigs)
            {
                rig.Pose(m_debugDraw, Blend, m_staticBlend);
            }
            CachedSphereCollider.FinishedPass();
            m_wasLODActive = true;
        }

        private void LateUpdate()
        {
            if (KrakenRigs[0].RootBone != null)
            {
                if (!Interpolate)
                {
                    return;
                }
                Advance(Time.deltaTime);
            }
        }

        private void FixedUpdate()
        {
            if (KrakenRigs[0].RootBone != null)
            {
                if (Interpolate)
                {
                    return;
                }
                Advance(Time.deltaTime);
            }
        }

        //TELEPORT
        public void PrepareTeleport()
        {
            foreach (var rig in KrakenRigs)
            {
                rig.PrepareTeleport();
            }
        }

        public void FinishTeleport()
        {
            foreach (var rig in KrakenRigs)
            {
                rig.FinishTeleport();
            }
        }

        private void OnDrawGizmos()
        {
            if (KrakenRigs == null)
            {
                return;
            }
            foreach (var rig in KrakenRigs)
            {
                rig.OnDrawGizmos(RadiusMultiplier, RadiusCurve);
            }
        }

        private void OnValidate()
        {
            if (Application.isPlaying || KrakenRigs == null) return;
            foreach (var rig in KrakenRigs)
            {
                rig.Initialize();
            }
        }
    }
}