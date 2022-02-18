using UnityEngine;

public class Parallax : MonoBehaviour
{
    [SerializeField]
    private float m_parallaxEffectSpeed;

    private float m_length;

    private float m_startPos;

    private void Start()
    {
        // Save the initial position on x
        m_startPos = transform.position.x;

        // Save the length using the sprite renderer boundary on x
        m_length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    private void Update()
    {
        // Move the background to the left by the speed
        transform.Translate(Vector3.left * m_parallaxEffectSpeed * Time.deltaTime);

        // If my position is less than the initial position minus lenght -> move background to them right
        if (transform.position.x < m_startPos - m_length)
        {
            transform.position = Vector3.right * m_startPos * m_length;
        }
    }
}
