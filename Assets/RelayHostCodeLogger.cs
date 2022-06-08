using UnityEngine;

public class RelayHostCodeLogger : MonoBehaviour
{
    private float m_currentTime = 0f;

    // Update is called once per frame
    async void Update()
    {
        m_currentTime += Time.deltaTime;

        if (m_currentTime >= 5f)
        {
            m_currentTime = 0f;

            var joinCodeAsyncTask = RelayManager.Instance.JoinCodeAsyncTask;

            if (joinCodeAsyncTask != null)
            {
                string joinCode = await joinCodeAsyncTask;

                Debug.Log($"Host Join Code: {joinCode}");
            }
        }
    }
}