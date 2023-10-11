using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestScript : MonoBehaviour
{
    [SerializeField] InputField _inputLongitude;
    [SerializeField] InputField _inputLatitude;
    [SerializeField] Text _textOutput;
    public void OnButtonClick()
    {
        float lon, lat;
        float.TryParse(_inputLatitude.text, out lat);
        float.TryParse(_inputLongitude.text, out lon);
        var rg = GetComponent<ReverseGeocoder>();
        string country = rg.ReverseGeocodeCountry(lon, lat);
        string usstate = rg.ReverseGeocodeUSAState(lon, lat);

        _textOutput.text = $"Country: '{country}'\nState: '{usstate}'";
    }
}
