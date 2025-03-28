using System.Collections;
using UnityEngine;

public class Dragon_Script : MonoBehaviour
{
    [SerializeField] private float ascendHeight = 10f;
    [SerializeField] private float flightSpeed = 15f;
    [SerializeField] private float disappearTime = 30f;

    private Animator _animator;

    private void Start()
    {
        _animator = GetComponent<Animator>();
    }

    public void StartFlight()
    {
        StartCoroutine(FlightRoutine());
    }

    private IEnumerator FlightRoutine()
    {
        _animator.SetTrigger("TakeOff");

        Vector3 takeOffTarget = transform.position + Vector3.up * ascendHeight;
        while (Vector3.Distance(transform.position, takeOffTarget) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, takeOffTarget, flightSpeed * Time.deltaTime);
            yield return null;
        }

        _animator.SetBool("isFlying", true);

        Vector3 flightTarget = transform.position + transform.forward * 500f;
        while (Vector3.Distance(transform.position, flightTarget) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, flightTarget, flightSpeed * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(disappearTime);
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Unit unit))
        {
            StartFlight();
        }
    }
}
