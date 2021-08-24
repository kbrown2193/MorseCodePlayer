﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MorseCodePlayer : MonoBehaviour {
    
    #region Public / SerializeField Variables
    public Text _txtMorseDebug;
    //public Text _txtMorseDotDash;
    //public Text _txtMorseBinary;
    public InputField _inputfieldMorseDotDash; // output field
    public InputField _inputfieldMorseBinary; // output field
    public InputField _inputfieldMorseCodeMessage; // input field
    public Button[] PlaybackSpeedButtons;

    public AudioSource morseSpeaker;
    public AudioClip morseNote_1;
    public AudioClip morseNote_3;
    //public AudioClip morseNote_click;
    public AudioClip morseNote_silence_0;
    public AudioClip morseNote_silence_000;
    public AudioClip morseNote_silence_0000000;

    public Animator theTelegraphKeyAnimator;
    #endregion

    #region Private Logic Variables
    private TextEditor _textEditor = new TextEditor(); // for copying and pasting into clipboard

    private string _enteredMorse = "Type message here";

    private int _currentMorse;
    private string _currentMorseMessage;
    private string _currentMorseDotDashMessage;
    private string _currentMorseBinaryMessage;
    private bool _isPlayingSymbol; // Like playing A so 103
    private bool _isPlayingMessage; // Like AB  1-303-1-1-1 000          1030003010101 0000000

    private float _playbackSpeedFactor = 1.0f; // a fraction, 1 = each beep is one second... 0.5 = each beep .5 seconds
    private int _currentPlaybackSetting = 0;

    private float[] _playbackSpeeds = new float
    []
    {
        1.0f,
        0.5f,
        0.25f,
        0.125f,
        0.0625f,
        0.03125f
    };
    #endregion

    #region Constants
    // colors ~ constants but cannot declare
    // Change these to change color scheme if customizable
    private Color PLAYBACKBUTTON_COLOR_SELECTED = Color.red;
    private Color PLAYBACKBUTTON_COLOR_UNSELECTED = Color.white;

    const int MORSE_ASCII_LETTER_OFS = 65;
    const int MORSE_ASCII_NUMBER_OFS = 22; // 48 -  26 (for alphabet codes) 

    const int MORSE_ASCII_SPACE = 32;
    const int MORSE_ASCII_NUM_0 = 48;
    const int MORSE_ASCII_NUM_1 = 49;
    const int MORSE_ASCII_NUM_2 = 50;
    const int MORSE_ASCII_NUM_3 = 51;
    const int MORSE_ASCII_SIGN_NEGATIVE = 45;
    /*
     * * ASCI RANGE
     * A = 65;
     * B = 66
     *  ..
     * Z = 90
     *  
     * space = 32
     * 
     * 0 = 48
     * 1 = 49
     *
     * 9 = 57
     */
    private int[] _morseCodeSymbols = new int[39]
        {
            13, 3111, 3131, 311, 1, 1131, 331, 1111, 11, 1333, 313,
            1311, 33, 31, 333, 1331, 3313, 131, 111, 3, 113, 1113, 133, 3113, 3133, 3311,
            33333,13333,11333,11133,11113,11111,
            31111,33111,33311,33331,
            0,-1,-2
        };

    private string[] _morseCodeDotDashes = new string[39]
       {
            ".-", "-...", "-.-.", "-..", ".", "..-.", "--.", "....", "..", ".---", "-.-",
            ".-..", "--", "-.", "---", ".--.", "--.-", ".-.", "...", "-", "..-", "...-", ".--", "-..-", "-.--", "--..",
            "-----",".----","..---","...--","....-",".....",
            "-....","--...","---..","----.",
            " ","   ","       "
       };
    #endregion

    #region Enumerations

    public enum MorseCodeNotes
    {
        morse_silence_0,
        morse_1,
        morse_silence_000,
        morse_3,
        morse_silence_0000000,

        morse_click,
    }

    // silence_0 is an implied in this encoding process. seperation of symbols indicates a 0.  i.e. a 113  is really 101030 and 113-1331 is really 1010300030301
    public enum MorseCodeSymbols
    {
        A_13,
        B_3111,
        C_3131,
        D_311,
        E_1,
        F_1131,
        G_331,
        H_1111,
        I_11,
        J_1333,
        K_313,
        L_1311,
        M_33,
        N_31,
        O_333,
        P_1331,
        Q_3313,
        R_131,
        S_111,
        T_3,
        U_113,
        V_1113,
        W_133,
        X_3113,
        Y_3133,
        Z_3311,
        num0_33333,num1_13333,num2_11333,num3_11133,num4_11113,num5_11111,num6_31111,num7_33111,num8_33311,num9_33331,
        silence_0, silence_000, silence_000000,
        punc_period, punc_comma, punc_question, punc_apostrophe, punc_exclaimation, punc_forwardslash, punc_parenopen, punc_parenclose,
        punc_ampersand, punc_colon, punc_semicolon, punc_equals, punc_plus, punc_minus, punc_underscore, punc_quotation, punc_dollar, punc_at,
        pros_endOfWork, pros_error, pros_invitationToTransmit, pros_startingSignal, pros_newpageSignal, pros_understood, pros_wait,
        none_aGrave, none_aUml
    }

    private enum PlaybackSpeedSetting
    {
        PB_00_One,
        PB_01_Half,
        PB_02_Quarter,
        PB_03_Eighth,
        PB_04_Sixteenth,
        PB_05_ThirtySecond
    }
    #endregion
 
    void Start () {
        ChangePlaybackSpeed((int)PlaybackSpeedSetting.PB_03_Eighth);

        _currentMorseMessage = ConvertStringToMorseCodeSymbols(_enteredMorse);

        //Debug.Log("SAMPLE MORSE = " + _enteredMorse);

        // Testing functions
        // StartCoroutine(PlayMorseCodeSymbolCo(_morseCodeSymbols[(int)MorseCodeSymbols.A_13]) );
        // PlayMorseCodeSymbol((int)MorseCodeSymbols.P_1331);
    }

    #region Input Text, Speed and Playing Functions
    // symbol is the index of MorseCodeSymbols... the  code is stored in the _morseCodeSymbols
    public void PlayMorseCodeSymbol(int symbol)
    {
        //Debug.Log( _morseCodeSymbols[symbol].ToString());
        // StartCoroutine(PlayMorseCodeSymbolCo(_morseCodeSymbols[symbol])); // no longer used... can be used to covert single simmple and test
    }
    public void PlayMorseCodeNote(int note)
    {
        if(note == (int)MorseCodeNotes.morse_1)
        {
            morseSpeaker.PlayOneShot(morseNote_1);
        }
        else if(note == (int)MorseCodeNotes.morse_3)
        {
            morseSpeaker.PlayOneShot(morseNote_3);
        }
        else if (note == (int)MorseCodeNotes.morse_silence_0)
        {
            morseSpeaker.PlayOneShot(morseNote_silence_0);
        }
        else if (note == (int)MorseCodeNotes.morse_silence_000)
        {
            morseSpeaker.PlayOneShot(morseNote_silence_000);
        }
        else if (note == (int)MorseCodeNotes.morse_silence_0000000)
        {
            morseSpeaker.PlayOneShot(morseNote_silence_0000000);
        }
    }

    public void RefreshDebugText()
    {
        _txtMorseDebug.text = "Current Symbol = M(" + _currentMorse.ToString("00")+")";
    }

    private void SetMorseDotDashText(string morseDotDash)
    {
        _inputfieldMorseDotDash.text = morseDotDash;
        //_txtMorseDotDash.text = morseDotDash;
    }

    private void SetMorseBinaryText(string binaryDash)
    {
        _inputfieldMorseBinary.text = binaryDash;
        //_txtMorseBinary.text = binaryDash;
    }

    // Playback speed functions
    public void ChangePlaybackSpeed(int playbackSetting)
    {
        _currentPlaybackSetting = playbackSetting;
        _playbackSpeedFactor = _playbackSpeeds[playbackSetting];
        morseSpeaker.pitch = 1.0f / _playbackSpeedFactor;

        // set all others to White?
        for (int i = 0; i < PlaybackSpeedButtons.Length; i ++)
        {
            PlaybackSpeedButtons[i].image.color = PLAYBACKBUTTON_COLOR_UNSELECTED;
        }

        // set current to red
        PlaybackSpeedButtons[playbackSetting].image.color = PLAYBACKBUTTON_COLOR_SELECTED;

    }

    // Only call to coroutine
    public void PlayCurrentMorseCodeMessage()
    {
        StopAllCoroutines(); // to prevent multiple playing messages
        StartCoroutine(PlayMorseCodeMessageCo());
    }

    // Only a call to convert function using the input field to reduce redundancy of code.  
    public void  EnterMorseCodeString( )
    {
        _currentMorseMessage = ConvertStringToMorseCodeSymbols(_inputfieldMorseCodeMessage.text); // dot dash is assigned in this function... could change to string[] return but nah
    }
    #endregion

    #region Morse Conversion Functions
    public string ConvertStringToMorseCodeSymbols(string messageToMorse)
    {
        messageToMorse = messageToMorse.ToUpper();
        // messageToMorse = messageToMorse.ToUpper();
        string morsedMessage = "";
        string dotdashedMessage = "";
        //Debug.Log("Message entered: " + messageToMorse);


        int i = 0;
        int tmpASCII = 0;
        for (i = 0; i < messageToMorse.Length; i++)
        {
           //Debug.Log("M[" + i + "] = " + messageToMorse[i]);
           //Debug.Log("ASCII = " + messageToMorse[i].GetHashCode());
            tmpASCII = messageToMorse[i].GetHashCode();

            // Evaluate based on ASCII code
            // Letter value (range inclusive)...
            if (tmpASCII >= 65 && tmpASCII <= 90)
            {
                // Letters A - Z
                morsedMessage += _morseCodeSymbols[tmpASCII - MORSE_ASCII_LETTER_OFS];
                dotdashedMessage += _morseCodeDotDashes[tmpASCII - MORSE_ASCII_LETTER_OFS];

                // CAN ELIMINATE EXTRA SPACE TODO:
                // Check if is next to last symbol, else can ignore?
                // Check next symbol if it is a space, ignore adding the -1?

                // So NOT next to last symbol so can check index +1
                if(i < messageToMorse.Length -1)
                {
                   // Debug.Log(messageToMorse[i + 1].GetHashCode());
                   // Debug.Log(MORSE_ASCII_SPACE);
                    if (messageToMorse[i+1].GetHashCode() == MORSE_ASCII_SPACE)
                    {
                       // is space so do not add end of symbol silence?
                       //Debug.Log("SPACE FOUND AT " + i);
                    }
                    else
                    {
                        // end of symbol space
                        morsedMessage += _morseCodeSymbols[(int)MorseCodeSymbols.silence_000];
                        dotdashedMessage += _morseCodeDotDashes[(int)MorseCodeSymbols.silence_000];
                    }
                }
            }
            else if (tmpASCII == 32)
            {
                // space...
                morsedMessage += _morseCodeSymbols[(int)MorseCodeSymbols.silence_000000];
                dotdashedMessage += _morseCodeDotDashes[(int)MorseCodeSymbols.silence_000000];

                // adds a word space to code
            }
            else if (tmpASCII >= 48 && tmpASCII <= 57)
            {
                // numbers 0 - 9
                morsedMessage += _morseCodeSymbols[tmpASCII - MORSE_ASCII_NUMBER_OFS];
                dotdashedMessage += _morseCodeDotDashes[tmpASCII - MORSE_ASCII_NUMBER_OFS];

                // to add end of symbol silence or no...
                // SO NOT next to last symbol so can check index +1
                if (i < messageToMorse.Length - 1)
                {
                    // Debug.Log(messageToMorse[i + 1].GetHashCode());
                    // Debug.Log(MORSE_ASCII_SPACE);
                    if (messageToMorse[i + 1].GetHashCode() == MORSE_ASCII_SPACE)
                    {
                        // is space so do not add end of symbol silence?
                       //Debug.Log("SPACE FOUND AT " + i+1);
                    }
                    else
                    {
                        // end of symbol space
                        morsedMessage += _morseCodeSymbols[(int)MorseCodeSymbols.silence_000];
                        dotdashedMessage += _morseCodeDotDashes[(int)MorseCodeSymbols.silence_000];
                    }
                }
            }
            else
            {
               //Debug.Log(messageToMorse[i] + " is not a recognized morse symbol: ignored");
            }

        }

        //_currentMorseMessage = morsedMessage; // currently in  EnterConvert function above...
        SetMorseDotDashText(dotdashedMessage);
        _currentMorseDotDashMessage = dotdashedMessage;
        ConvertDotDashToBinary();
        SetMorseBinaryText(_currentMorseBinaryMessage);
        return morsedMessage;
        //Debug.Log("Morse Code: " + morsedMessage);
        //Debug.Log("Morse Code: " + dotdashedMessage);
        // SetMorseDotDashText(dotdashedMessage);
    }

    private void ConvertDotDashToBinary()
    {
        _currentMorseBinaryMessage = "";
        int i = 0;
        for (i =0; i < _currentMorseDotDashMessage.Length; i++)
        {
            if (_currentMorseDotDashMessage[i] == '.')
            {
                _currentMorseBinaryMessage += "1";

                // add end of symbol space
                if (i < _currentMorseDotDashMessage.Length - 1)
                {
                    if(_currentMorseDotDashMessage[i+1] != ' ')
                    {
                        // only dont add 0 if next symbol is a space
                        _currentMorseBinaryMessage += "0";
                    }
                    else
                    {

                    }
                }
            }

            else if (_currentMorseDotDashMessage[i] == '-')
            {
                _currentMorseBinaryMessage += "111";

                // add end of symbol space
                if (i < _currentMorseDotDashMessage.Length - 1)
                {
                    if (_currentMorseDotDashMessage[i + 1] != ' ')
                    {
                        // only dont add 0 if next symbol is a space
                        _currentMorseBinaryMessage += "0";
                    }
                    else
                    {

                    }

                }
            }
            else if (_currentMorseDotDashMessage[i] == ' ')
            {
                _currentMorseBinaryMessage += "0";
            }
        }
    }
    #endregion

    #region Clipboard / Copy Paste Functions
    // Clipboard functions (copying morse to clipboard)
    // thanks to this forum post: https://forum.unity3d.com/threads/copy-textfield-or-textarea-text-to-clipboard.24101/ 
    // THIS IS OUTDATED IN WEB PLAYER NOW?...
    public void CopyMorseDotDashToClipboard()
    {
        _textEditor.text = _currentMorseDotDashMessage;
        _textEditor.SelectAll();
        _textEditor.Copy();
    }

    public void CopyMorseBinaryToClipboard()
    {
        _textEditor.text = _currentMorseBinaryMessage;
        _textEditor.SelectAll();
        _textEditor.Copy();
    }
    #endregion

    #region General Numerical Functions
    //  Could be a static helper class...
    private int CalculateIntDigits(int number)
    {
        // ex 120 
        int numDigits = 0;
        while (number > 0)
        {
            number = number / 10;
            numDigits += 1;
            // 120 iteration 1
            // 12 = 120/10      
            // numdigits = 1
            // iteration 2
            // 1 = 12/
            // numdigits = 2
            // iteration 3
            // 0 = 1/10
            // num digits = 3
        }

        return numDigits;
    }
    #endregion

    #region Coroutines: Playing the morse code message    
    private IEnumerator PlayMorseCodeMessageCo()
    {
        // ex 3-13133
        //_currentMorseMessage;
        int messageLength = _currentMorseMessage.Length;

        int i = 0;
        int j = 0; // j is the index of the dot-dash, so does not increment when sees a -
        while (i < messageLength)
        {
            if(_currentMorseMessage[i].GetHashCode() == MORSE_ASCII_NUM_1)
            {
                morseSpeaker.PlayOneShot(morseNote_1);
                //Debug.Log("Single Beep");
                theTelegraphKeyAnimator.SetTrigger("trigCloseCircuit1Second");
                //theTelegraphKeyAnimator.speed = _playbackSpeeds[_currentPlaybackSetting]/1.0f;
                theTelegraphKeyAnimator.speed = (1.0f / _playbackSpeeds[_currentPlaybackSetting]) ;
                //Debug.Log((1.0f / _playbackSpeeds[_currentPlaybackSetting]));
                yield return new WaitForSeconds(1.0f *_playbackSpeedFactor);

                // to ad end of symbol silence or no...
                // SO NOT next to last symbol so can check index +1
                if (i < messageLength - 1)
                {
                    // Debug.Log(messageToMorse[i + 1].GetHashCode());
                    // Debug.Log(MORSE_ASCII_SPACE);
                    if (_currentMorseMessage[i + 1].GetHashCode() == MORSE_ASCII_SPACE)
                    {
                        // is space so do not add end of symbol silence?
                        //Debug.Log("SPACE FOUND AT " + i + 1);

                    }
                    else
                    {
                        // end of symbol space
                        yield return new WaitForSeconds(1.0f * _playbackSpeedFactor);

                        // morsedMessage += _morseCodeSymbols[(int)MorseCodeSymbols.silence_000];
                        // dotdashedMessage += _morseCodeDotDashes[(int)MorseCodeSymbols.silence_000];
                    }
                }
                i++;
                j++;

            }
            else if(_currentMorseMessage[i].GetHashCode() == MORSE_ASCII_NUM_3)
            {
                morseSpeaker.PlayOneShot(morseNote_3);
                //Debug.Log("Triple Beep");

                theTelegraphKeyAnimator.SetTrigger("trigCloseCircuit3Second");
                theTelegraphKeyAnimator.speed = (1.0f / _playbackSpeeds[_currentPlaybackSetting]); // move this for optimizations, only needs to be called when changing playback speed

                yield return new WaitForSeconds(3.0f * _playbackSpeedFactor);

                if (i < messageLength - 1)
                {
                    //Debug.Log(messageToMorse[i + 1].GetHashCode());
                    //Debug.Log(MORSE_ASCII_SPACE);
                    if (_currentMorseMessage[i + 1].GetHashCode() == MORSE_ASCII_SPACE)
                    {
                        // is space so do not add end of symbol silence?
                        //Debug.Log("SPACE FOUND AT " + i + 1);

                    }
                    else
                    {
                        // end of symbol space
                        yield return new WaitForSeconds(1.0f * _playbackSpeedFactor);

                        // morsedMessage += _morseCodeSymbols[(int)MorseCodeSymbols.silence_000];
                        // dotdashedMessage += _morseCodeDotDashes[(int)MorseCodeSymbols.silence_000];
                    }
                }
                i++;
                j++;
            }
            else if (_currentMorseMessage[i].GetHashCode() == MORSE_ASCII_SIGN_NEGATIVE)
            {
                // NEGATIVE SHOULD NEVER BE LAST SYMBOL SO SHOULD BE OK, ADD CHECK?
                i++;
                if(_currentMorseMessage[i].GetHashCode() == MORSE_ASCII_NUM_1)
                {
                    //Debug.Log("Symbol Silence 000");

                    // Symbol  space    000
                    yield return new WaitForSeconds(3.0f * _playbackSpeedFactor);
                }
                else if (_currentMorseMessage[i].GetHashCode() == MORSE_ASCII_NUM_2)
                {
                    // word space   0000000
                    //Debug.Log("Word Silence 0000000");
                    yield return new WaitForSeconds(7.0f * _playbackSpeedFactor);
                }
                i++;
                j++;
            }

            // TODO: move cursor around highlighted symbol... 
        }
        // end ....
        yield break;
    }
    #endregion
}
