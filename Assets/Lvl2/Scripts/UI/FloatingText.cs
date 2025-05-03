using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class FloatingTextManager : MonoBehaviour
{
    public static FloatingTextManager Instance { get; private set; }

    [Header("Base Positions")]
    [SerializeField] private RectTransform _text1Position;
    [SerializeField] private RectTransform _text2Position;
    [SerializeField] private RectTransform _text3Position;

    [Header("Text Prefabs")]
    [SerializeField] private TMP_Text _text1Prefab;
    [SerializeField] private TMP_Text _text2Prefab;
    [SerializeField] private TMP_Text _text3Prefab;

    [Header("Animation Settings")]
    [SerializeField] private float _moveDistance = 50f;
    [SerializeField] private float _duration = 1f;
    [SerializeField] private float _text3Spacing = 30f;

    private Queue<TMP_Text> _text1Pool = new Queue<TMP_Text>();
    private Queue<TMP_Text> _text2Pool = new Queue<TMP_Text>();
    private List<ActiveText3> _activeText3Instances = new List<ActiveText3>();
    private const int POOL_SIZE = 5;
    private const int MAX_TEXT3 = 3;

    private class ActiveText3
    {
        public TMP_Text text;
        public float verticalOffset;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        InitializePools();
    }

    private void InitializePools()
    {
        for (int i = 0; i < POOL_SIZE; i++)
        {
            _text1Pool.Enqueue(CreateText(_text1Prefab, _text1Position));
            _text2Pool.Enqueue(CreateText(_text2Prefab, _text2Position));
        }
    }

    private TMP_Text CreateText(TMP_Text prefab, RectTransform position)
    {
        var text = Instantiate(prefab, position.parent);
        text.gameObject.SetActive(false);
        return text;
    }

    public static void ShowText1(string message) => Instance?.DisplayText1(message);
    public static void ShowText2(string message) => Instance?.DisplayText2(message);
    public static void ShowText3(string message) => Instance?.DisplayText3(message);

    private void DisplayText1(string message)
    {
        if (_text1Pool.Count == 0) return;
        
        var text = _text1Pool.Dequeue();
        SetupText(text, message, _text1Position);
        StartCoroutine(AnimateText(text, _text1Pool));
    }

    private void DisplayText2(string message)
    {
        if (_text2Pool.Count == 0) return;
        
        var text = _text2Pool.Dequeue();
        SetupText(text, message, _text2Position);
        StartCoroutine(AnimateText(text, _text2Pool));
    }

    private void DisplayText3(string message)
    {
        if (_activeText3Instances.Count >= MAX_TEXT3) return;
        
        var text = Instantiate(_text3Prefab, _text3Position.parent);
        float offset = CalculateText3Offset();
        var activeText = new ActiveText3 { text = text, verticalOffset = offset };
        _activeText3Instances.Add(activeText);
        
        SetupText(text, message, _text3Position, offset);
        StartCoroutine(AnimateText3(activeText));
    }

    private float CalculateText3Offset()
    {
        float offset = 0;
        foreach (var instance in _activeText3Instances)
        {
            offset += _text3Spacing;
        }
        return offset;
    }

    private void SetupText(TMP_Text text, string message, RectTransform position, float verticalOffset = 0)
    {
        if (text == null || position == null) return;
        
        text.text = message;
        text.alpha = 1f;
        text.rectTransform.position = position.position + Vector3.up * verticalOffset;
        text.gameObject.SetActive(true);
    }

    private IEnumerator AnimateText(TMP_Text text, Queue<TMP_Text> pool)
    {
        if (text == null) yield break;
        
        float elapsed = 0;
        Vector3 startPosition = text.rectTransform.position;
        Color startColor = text.color;

        while (elapsed < _duration)
        {
            if (text == null) yield break;
            
            float t = elapsed / _duration;
            text.rectTransform.position = startPosition + Vector3.down * _moveDistance * t;
            text.color = new Color(startColor.r, startColor.g, startColor.b, 1 - t);
            
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (text != null)
        {
            text.gameObject.SetActive(false);
            pool?.Enqueue(text);
        }
    }

    private IEnumerator AnimateText3(ActiveText3 activeText)
    {
        yield return AnimateText(activeText.text, null);
        
        if (activeText.text != null)
        {
            Destroy(activeText.text.gameObject);
        }
        _activeText3Instances.Remove(activeText);
    }
}