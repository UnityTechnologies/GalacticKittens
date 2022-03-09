using TMPro;
using UnityEngine;

public class PlayerShipScore : MonoBehaviour
{
    [SerializeField]
    GameObject m_vfxSmoke;

    [SerializeField]
    GameObject m_vfxJet;

    [SerializeField]
    TextMeshPro m_scoreText;

    [SerializeField]
    TextMeshPro m_enemiesDestroyedText;
    
    [SerializeField]
    TextMeshPro m_powerUpsUsedText;

    [SerializeField]
    GameObject m_ship;

    [SerializeField]
    GameObject m_crown;

    [SerializeField]
    AnimationCurve m_moveCurve;

    float m_curveDeltaTime;

    void Update()
    {
        // Move the ship on an curve movement
        Vector2 currentPosition = m_ship.transform.localPosition;
        m_curveDeltaTime += Time.deltaTime;
        currentPosition.y = m_moveCurve.Evaluate(m_curveDeltaTime);
        m_ship.transform.localPosition = currentPosition;
    }

    public void SetShip(bool victory, int enemiesDestroyed, int powerUpsUsed, int score)
    {
        // Set vfx depending on the scene we are loading 
        m_vfxSmoke.SetActive(!victory);
        m_vfxJet.SetActive(victory);

        // Set UI data base on the character data
        m_enemiesDestroyedText.text = enemiesDestroyed.ToString();
        m_powerUpsUsedText.text = powerUpsUsed.ToString();
        m_scoreText.text = score.ToString();
    }

    // Turn on the crown because I'm the best ship
    public void BestShip()
    {
        m_crown.SetActive(true);
    }
}
