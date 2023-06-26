using System;
using UnityEngine;
using Unity.Netcode;

public class PlayerShipMovement_DEMO : NetworkBehaviour
{
    enum MoveType
    { 
        constant,
        momentum
    }

    enum VerticalMovementType
    {
        none,
        upward,
        downward
    }

    [Serializable]
    public struct PlayerLimits
    {
        public float minLimit;
        public float maxLimit;
    }

    [SerializeField]
    MoveType m_moveType = MoveType.momentum;

    [SerializeField]
    PlayerLimits m_verticalLimits;

    [SerializeField]
    PlayerLimits m_hortizontalLimits;
    
    [SerializeField]
    private float m_speed;

    private float m_inputX;
    private float m_inputY;

    const string k_horizontalAxis = "Horizontal";
    const string k_verticalAxis = "Vertical";

    // Update is called once per frame
    void Update()
    {
        // We're only updating the ship's movements when we're surely updating on the owning
        // instance
        if (!IsOwner)
            return;

        HandleKeyboardInput();

        MovePlayerShip();
    }

    private void HandleKeyboardInput()
    {
        /*
            There two types of movement:
            constant -> linear move, there are no time acceleration
            momentum -> move with acceleration at the start

            Note: feel free to add your own type as well
        */
        if (m_moveType == MoveType.constant)
        {
            HandleMoveTypeConstant();
        }
        else if (m_moveType == MoveType.momentum)
        {
            HandleMoveTypeMomentum();
        }
    }

    private void HandleMoveTypeConstant()
    {
        m_inputX = 0f;
        m_inputY = 0f;

        // Horizontal input
        if (Input.GetKey(KeyCode.D))
        {
            m_inputX = 1f;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            m_inputX = -1f;
        }

        // Vertical input and set the ship sprite
        if (Input.GetKey(KeyCode.W))
        {
            m_inputY = 1f;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            m_inputY = -1f;
        }
    }

    private void HandleMoveTypeMomentum()
    {
        m_inputX = Input.GetAxis(k_horizontalAxis);
        m_inputY = Input.GetAxis(k_verticalAxis);
    }

    private void MovePlayerShip()
    {
        // Take the value from the input and multiply by speed and time
        float speedTimesDeltaTime = m_speed * Time.deltaTime;

        float newYposition = m_inputY * speedTimesDeltaTime;
        float newXposition = m_inputX * speedTimesDeltaTime;

        // move the ship
        transform.Translate(newXposition, newYposition, 0f);
    }
}