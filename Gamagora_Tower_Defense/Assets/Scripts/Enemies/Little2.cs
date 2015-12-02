using UnityEngine;
using System.Collections.Generic;

public class Little2 : Enemy
{
    private GameObject _pivot1;
    private GameObject _pivot2;
    private GameObject _pivot3;
    private GameObject _pivot4;

    private float _timeNoShield;
    public float TimeToChargeShield;

    private List<GameObject> _pivots;

    protected override void Awake()
    {
        base.Awake();
        _pivot1 = transform.FindChild("Pivot1").gameObject;
        _pivot2 = transform.FindChild("Pivot2").gameObject;
        _pivot3 = transform.FindChild("Pivot3").gameObject;
        _pivot4 = transform.FindChild("Pivot4").gameObject;
        _pivots = new List<GameObject>();
    }

    private void ChargeShield()
    {
        SetAllPivotsActive(true);
        _pivots.Clear();
        _pivots.Add(_pivot1);
        _pivots.Add(_pivot2);
        _pivots.Add(_pivot3);
        _pivots.Add(_pivot4);
    }

    private void SetAllPivotsActive(bool active)
    {
        _pivot1.SetActive(active);
        _pivot2.SetActive(active);
        _pivot3.SetActive(active);
        _pivot4.SetActive(active);
    }

    protected override void OnEnable()
    {
        SetAllPivotsActive(true);
        ChargeShield();
    }

    protected override void Explode()
    {
        foreach (GameObject obj in _pivots)
            obj.transform.FindChild("Shield").gameObject.SetActive(false);

        _pivots.Clear();
        base.Explode();
    }

    public override void ReceiveDamage(float damage)
    {
        if (_pivots.Count > 0)
        {
            _pivots[_pivots.Count - 1].SetActive(false);
            _pivots.RemoveAt(_pivots.Count - 1);

            if (_pivots.Count == 0)
                _timeNoShield = Time.realtimeSinceStartup;
        }
        else
            base.ReceiveDamage(damage);
    }

    protected override void Update()
    {
        base.Update();

        if(IsDead())
        {
            _pivots.Clear();
            SetAllPivotsActive(false);
        }
        else if (_pivots.Count == 0 && Time.realtimeSinceStartup - _timeNoShield > TimeToChargeShield)
        {
            ChargeShield();
        }
    }
}
