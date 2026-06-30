using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIFade : MonoBehaviour

{
    // [HideInInspector] public CanvasGroup _myCanvasGroup;
 public CanvasGroup _myCanvasGroup;

 public float _speed;
    
public float _delay;

public float _endDelay = 0.5f;

//public Ease _selectEase;
    
public bool InOut;
    
public bool OnStart;
    
public bool FadingIn;
    
public bool FadingOut;

 //public bool ModifyOtherCanvasGroupComponentOnFinished;
        
public Type FadingType;

public UnityEvent FadingInStart;
    
public UnityEvent FadingOutStart;
    
public UnityEvent FadingInEnd;
    
public UnityEvent FadingOutEnd;


    
    private void OnValidate()
    {
        if (GetComponent<CanvasGroup>())
        {
            _myCanvasGroup = GetComponent<CanvasGroup>();
        }
    }

    private void Awake()
    {
        if (OnStart)
        {
            if (FadingIn)
            {
                StartFadingIn();
            }

            if (FadingOut)
            {
                StartFadingOut();
            }
        }
    }
    
    public void StartFadingIn()
    {
        StartCoroutine(CorStartFadingIn());
    }

    IEnumerator CorStartFadingIn()
    {
        yield return new WaitForSeconds(_delay);
                
        FadingInStart.Invoke();

        _myCanvasGroup.alpha = 0;
        
        _myCanvasGroup.DOFade(1, _speed).SetId("Fade In Normal").OnComplete(AfterFadeIn);
    }
    
    private IEnumerator NormalFadingOutRoutine()
    {
        yield return new WaitForSeconds(_endDelay);
        Tween fadeOut = _myCanvasGroup.DOFade(0, _speed).SetId("Fade In Normal");

        StartCoroutine(CompletionCheck(fadeOut));
    }
    
    public void StartFadingOut()
    {
        StartCoroutine(CorStartFadingOut());
    }

    private IEnumerator CorStartFadingOut()
    {
        FadingOutStart.Invoke();
                
        StartCoroutine(NormalFadingOutRoutine());
        return null;
    }
    
    private IEnumerator CompletionCheck(Tween tween)
    {
        bool normalcomplete = false;
        tween.OnComplete(() => { normalcomplete = true; });
        bool b = tween.IsActive();
        while (b)
        {
            b = tween.IsActive();
            yield return null;
        }
        
        if (normalcomplete)
        {
            AfterFadeOut();

            yield break;
        }

        StartFadingOut();
    }
    
    private void AfterFadeOut()
    {
        FadingOutEnd.Invoke();
        
        _myCanvasGroup.interactable = false;
        
        _myCanvasGroup.blocksRaycasts = false;

    }
    
    public void AfterFadeIn()
    {
        if (InOut)
        {
            StartFadingOut();
        }
        else
        {
            FadingInEnd.Invoke();

            _myCanvasGroup.interactable = true;
            
            _myCanvasGroup.blocksRaycasts = true;
        }
        
    }
}
