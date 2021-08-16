using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignalBit : MonoBehaviour {

    private bool _isHigh; // else is low
    private Renderer _rend;





    void Awake()
    {
        _rend = GetComponent<Renderer>();
    }
    void Start()
    {
        RefreshStatusDependents();
    }



    public bool GetStatus()
    {
        return _isHigh;
    }
    public void ToggleStatus()
    {
        _isHigh = !_isHigh;
    }
    public void SetStatus(bool status)
    {
        _isHigh = status;
        RefreshStatusDependents();
    }
    public void SetHigh()
    {
        _isHigh = true;
    }
    public void SetLow()
    {
        _isHigh = false;
    }


    // to change color if is on or not
    private void RefreshStatusDependents()
    {
        // color change...
        // toDO POSITION move
        if(_isHigh)
        {
            _rend.material.SetColor("_Color", Color.green);
        }
        else
        {
            _rend.material.SetColor("_Color", Color.red);
        }
    }

}
