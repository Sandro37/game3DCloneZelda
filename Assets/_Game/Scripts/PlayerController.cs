using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [Header("Config player")]
    [SerializeField] private float hp;


    [Header("Config movement player")]
    [SerializeField] private float movementSpeed = 3f;

    [Header("Attack config")]
    [SerializeField] private ParticleSystem attackFx;
    [SerializeField] private Transform hitBox;
    [SerializeField] private float amoutDamage;
    [Range(0.2f, 1.0f)]
    [SerializeField] private float hitRange = 0.5f;
    [SerializeField] private LayerMask hitMask;


    private Vector3 direction;
    private CharacterController controller;
    private Animator anim;

    private bool isWalk;
    private bool isAttack;
    public bool isDead = false;
    private GameManager _gameManager;
    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        _gameManager = FindObjectOfType<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead) return;

        Inputs();
        Move();
        Attack();
        UpdateAnimations();
    }

    #region MEUS METODOS

    void Move()
    {
        if (direction.magnitude > 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, targetAngle, 0);
            isWalk = true;
        }
        else
        {
            isWalk = false;
        }
        controller.Move(direction * movementSpeed * Time.deltaTime);
    }

    void Inputs()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        direction = new Vector3(horizontal, 0f, vertical).normalized;
    }

    void Attack()
    {
        if (Input.GetButtonDown("Fire1") && !isAttack)
        {
            isAttack = true;
            anim.SetTrigger("Attack");
            attackFx.Emit(1);

            Collider[] hitInfo = Physics.OverlapSphere(hitBox.position, hitRange, hitMask);

            foreach (Collider col in hitInfo)
            {
                col.gameObject.SendMessage("GetHit", amoutDamage, SendMessageOptions.DontRequireReceiver);
            }
            hitInfo = null;
        }
    }
    void UpdateAnimations()
    {
        anim.SetBool("isWalk", isWalk);
    }


    void AttackIsDone()
    {
        isAttack = false;
    }


    void GetHit(int amount)
    {
        hp -= amount;

        if(hp > 0)
        {
            anim.SetTrigger("Hit");
        }
        else
        {
            anim.SetTrigger("Die");
            isDead = true;
        }
    }
    #endregion


    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(hitBox.position, hitRange);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("TakeDamage"))
        {
            GetHit(1);
        }
        if (other.CompareTag("Collectable"))
        {
            _gameManager.SetGem(1);
            other.gameObject.SetActive(false);
        }
    }
}
