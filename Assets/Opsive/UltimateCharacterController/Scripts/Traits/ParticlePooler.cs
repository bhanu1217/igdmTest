﻿/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Traits
{
    using UnityEngine;
    using Opsive.Shared.Game;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER_HOTFIX
    using Opsive.UltimateCharacterController.Networking.Game;
#endif

    /// <summary>
    /// Pools the ParticleSystem after it is done playing.
    /// </summary>
    public class ParticlePooler : MonoBehaviour
    {
        private GameObject m_GameObject;
        private ParticleSystem m_ParticleSystem;

        private ScheduledEventBase m_PoolEvent;

        /// <summary>
        /// Initialize the default variables.
        /// </summary>
        private void Awake()
        {
            m_GameObject = gameObject;
            m_ParticleSystem = GetComponent<ParticleSystem>();
        }

        /// <summary>
        /// Schedules the object to be pooled after the particle system has stopped playing.
        /// </summary>
        private void OnEnable()
        {
            m_PoolEvent = SchedulerBase.Schedule(m_ParticleSystem.main.duration, PoolGameObject);
        }

        /// <summary>
        /// Cancels the pool event if the object is disabled early.
        /// </summary>
        private void OnDisable()
        {
            SchedulerBase.Cancel(m_PoolEvent);
        }

        /// <summary>
        /// Returns the GameObject back to the ObjectPool.
        /// </summary>
        private void PoolGameObject()
        {
            // The particle may be looping so it shouldn't be stopped yet.
            if (m_ParticleSystem.IsAlive(true)) {
                m_PoolEvent = SchedulerBase.Schedule(m_ParticleSystem.main.duration, PoolGameObject);
                return;
            }
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER_HOTFIX
            if (NetworkObjectPool.IsNetworkActive()) {
                // The object may have already been destroyed over the network.
                if (!m_GameObject.activeSelf) {
                    return;
                }
                NetworkObjectPool.Destroy(m_GameObject);
                return;
            }
#endif
            m_PoolEvent = null;
            ObjectPoolBase.Destroy(m_GameObject);
        }
    }
}