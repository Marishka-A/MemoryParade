//using Unity.Framework;
using Assets.MemoryParade.Scripts.Game.GameRoot;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CharacterMove : MonoBehaviour
{
    private Animator _animator;
    private GameObject _gameObject;

    public bool canMove = true;
    
    void Start()
    {
        _animator = GetComponent<Animator>();
        _gameObject = GameObject.Find("BattleCanvas");
        if (_animator == null)
        {
            Debug.LogError("Animator �� ������ �� ������� " + gameObject.name);
        }

        _animator.SetFloat("X", 0);
        _animator.SetFloat("Y", 0);
    }

    void FixedUpdate()
    {
        //if (SceneManager.GetActiveScene().name == Scenes.GAMEPLAY)
        //    _gameObject.SetActive(false);
        if (!canMove)
            return;

        if (_animator == null)
        {
            Debug.LogWarning("Animator ����� null �� ������� " + gameObject.name);
            return;
        }

        var move = GetMove();

        if (move != Vector2.zero)
        {
            _animator.SetFloat("X", move.x);
            _animator.SetFloat("Y", move.y);

            transform.Translate(move * 0.02f); // ��������
        }
        else
        {
            // �������� �������� ���������, ��� ����, ����� �������� �����
            _animator.SetFloat("X", 0);
            _animator.SetFloat("Y", 0);
        }
    }

    private Vector2 GetMove()
    {
        Vector2 move = Vector2.zero;

        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
        {
            move += new Vector2(0, 1);
        }
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
        {
            move += new Vector2(0, -1);
        }
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
        {
            move += new Vector2(-1, 0);
        }
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
        {
            move += new Vector2(1, 0);
        }

        return move == Vector2.zero ? move : move.normalized;
    }
}