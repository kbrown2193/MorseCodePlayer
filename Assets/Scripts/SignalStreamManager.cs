using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SignalStreamManager : MonoBehaviour {

    public SignalBit signalBitPrefab;

    private float _signalTickTime = 0.5f;
    public Text txtSignalDebug;
    private float _timer;

    private SignalBit[] _signalBits;
    private bool _lastBitOverflow; // the 255th signal bit status 1 clock cycle ago...
    private bool _incomingBit; // the first bit to be set


    const int SIGNAL_STREAM_SIZE = 256;
    const float SIGNAL_BIT_X_SPACING = 1.0f;

    void Awake()
    {
        Vector3 POS = new Vector3(0f, 0f, 0f);
        // set size
        _signalBits = new SignalBit[SIGNAL_STREAM_SIZE];
        // generate signal bits...
        for(int i = 0; i < SIGNAL_STREAM_SIZE; i++)
        {
            _signalBits[i] = Instantiate<SignalBit>(signalBitPrefab);
            _signalBits[i].transform.position = POS;
            POS.x += SIGNAL_BIT_X_SPACING;
            // NEED TO PARENT TO this 
            _signalBits[i].SetStatus(false);
        }
    }
	// Use this for initialization
	void Start () {
        _timer = 0.0f;
        _incomingBit = true;
	}
	
	// Update is called once per frame
	void Update () {
        _timer += Time.deltaTime;
        if(_timer >= _signalTickTime)
        {
            // time to refresh bits...
            // pre calculate for better performance?? todo...
            PushSignalBitStatuses();


            // get new incoming bit
            RefreshIncomingBit();





            // reset timer
            _timer -= _signalTickTime;
        }


        RefreshDebugText();
    }


    private void PushSignalBitStatuses()
    {
        /*
        Doing it this way results in all ahving the first status.. need to start from backend
        
        _signalBits[0].SetStatus(_incomingBit);
        for (int i = 1; i < SIGNAL_STREAM_SIZE; i++)
        {
            _signalBits[i].SetStatus( _signalBits[i - 1].GetStatus());
        }
        */
        _lastBitOverflow = _signalBits[SIGNAL_STREAM_SIZE - 1].GetStatus();
        for(int i = SIGNAL_STREAM_SIZE-1; i >0; i--)
        {
            _signalBits[i].SetStatus(_signalBits[i - 1].GetStatus());
        }
        _signalBits[0].SetStatus(_incomingBit);

    }

    private void RefreshDebugText()
    {
        txtSignalDebug.text = "Timer: " + _timer.ToString("0.000") + "\nIncomingBit = " + _incomingBit.ToString() + "\nLast Bit Overflow = " + _lastBitOverflow.ToString();
    }

    private void RefreshIncomingBit()
    {
        // change incoming bit here...

    }



    // public call functions
    public void SetIncomingBitHigh()
    {
        _incomingBit = true;
    }
    public void SetIncomingBitLow()
    {
        _incomingBit = false;
    }
    public void SetIncomingBit(bool incomingBit)
    {
        _incomingBit = incomingBit;
    }


    public bool GetIncomingBit()
    {
        return _incomingBit;
    }
    public bool GetLastBitOverflow()
    {
        return _lastBitOverflow;
    }

    


}
