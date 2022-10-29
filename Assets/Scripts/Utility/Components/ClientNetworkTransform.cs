using Unity.Netcode.Components;
using UnityEngine;

namespace Unity.Netcode.Samples
{
    /// <summary>
    /// Used for syncing a transform with client side changes. This includes host. Pure server as
    /// owner isn't supported by this. Please use NetworkTransform for transforms that'll always
    /// be owned by the server.
    /// </summary>
    [DisallowMultipleComponent]
    public class ClientNetworkTransform : NetworkTransform
    {

        public delegate void StateUpdatedDelegate(bool positionUpdated, bool rotationUpdated, bool scaleUpdated);
        public StateUpdatedDelegate NonAuthoritativeTransformStasteUpdated;

        protected override void OnNonAuthoritativeStateUpdated(bool positionUpdated, bool rotationUpdated, bool scaleUpdated)
        {
            NonAuthoritativeTransformStasteUpdated?.Invoke(positionUpdated, rotationUpdated, scaleUpdated);
        }

        public delegate void AuthoritativeStateUpdatedDelegate(bool positionUpdated, bool rotationUpdated, bool scaleUpdated);
        public AuthoritativeStateUpdatedDelegate AuthoritativeStateCommitted;

        protected override void OnAuthoritativeStateUpdated(bool positionUpdated, bool rotationUpdated, bool scaleUpdated)
        {
            AuthoritativeStateCommitted?.Invoke(positionUpdated, rotationUpdated, scaleUpdated);
        }

        /// <summary>
        /// Used to determine who can write to this transform. Owner client only.
        /// This imposes state to the server. This is putting trust on your clients. Make sure no
        /// security-sensitive features use this transform
        /// </summary>
        protected override bool OnIsServerAuthoritative()
        {
            return false;
        }
    }
}