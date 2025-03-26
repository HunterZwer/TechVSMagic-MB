using System.Collections;
using UnityEngine;

public class Dragon_Script : MonoBehaviour
{
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float speed = 12f;
    [SerializeField] private float rotationSpeed = 2f;
    [SerializeField] private float ascendHeight = 5f;
    [SerializeField] private float descendDistance = 5f;
    [SerializeField] private float descendSpeed = 3f;
    [SerializeField] private float waitTimeIdle = 2f;
    [SerializeField] private float waitTimeAfterLanding = 2.5f;
    [SerializeField] private float waitTimeBeforeTakeOff = 1.5f;
    [SerializeField] private float walkDuration = 3f;

    private Transform _currentTarget;
    private bool _isFlying = false;
    private bool _isTurning = false;
    private bool _isMoving;
    private Animator _animator;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _currentTarget = pointA;
    }

    public void StartFlight()
    {
        if (!_isMoving) 
        {
            _isMoving = true;
            StartCoroutine(DragonRoutine());
        }
    }

    private IEnumerator DragonRoutine()
    {
        transform.LookAt(_currentTarget.position);


        _animator.SetBool("isIdle", true);
        yield return new WaitForSeconds(waitTimeIdle);

     
        _animator.SetBool("isIdle", false);
        _animator.SetTrigger("TakeOff");


        Vector3 takeOffTarget = transform.position + Vector3.up * ascendHeight;
        while (Vector3.Distance(transform.position, takeOffTarget) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, takeOffTarget, (speed / 2) * Time.deltaTime);
            yield return null;
        }

       
        _animator.SetBool("isFlying", true);
        _isFlying = true;

        while (_isFlying)
        {
            float distanceToTarget = Vector3.Distance(transform.position, _currentTarget.position);

            if (distanceToTarget <= descendDistance)
            {
                StartLanding();
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, _currentTarget.position, speed * Time.deltaTime);
            }

            yield return null;
        }

       
        yield return new WaitForSeconds(waitTimeAfterLanding);

        
        _currentTarget = (_currentTarget == pointA) ? pointB : pointA; 
        _isTurning = true;
        _animator.SetBool("isWalking", true); 

        float turnTime = 0f;
        while (_isTurning && turnTime < walkDuration)
        {
            LookAtTarget(_currentTarget.position * Time.deltaTime);
            turnTime += Time.deltaTime;
            yield return null;
        }
        
        _animator.SetBool("isWalking", false);
        _animator.SetBool("isIdle", true);
        yield return new WaitForSeconds(waitTimeBeforeTakeOff);

        
        _isMoving = false;
    }

    private void StartLanding()
    {
        _isFlying = false;
        _animator.SetBool("isFlying", false);
        _animator.SetBool("isLanding", true);
        StartCoroutine(LandingSequence());
    }

    private IEnumerator LandingSequence()
    {
        Vector3 landTarget = _currentTarget.position;

        while (Vector3.Distance(transform.position, landTarget) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, landTarget, descendSpeed * Time.deltaTime);
            yield return null;
        }

        _animator.SetBool("isLanding", false);
        _animator.SetBool("isIdle", true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Unit unit))
        {
            StartFlight();
        }
    }

    private void LookAtTarget(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }
}
