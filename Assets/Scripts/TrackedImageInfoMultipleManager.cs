using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARTrackedImageManager))]
public class TrackedImageInfoMultipleManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI text;

     private List<string> CardNames;

    private ARTrackedImageManager _aRTrackedImageManager;

    private void Awake(){
        _aRTrackedImageManager = GetComponent<ARTrackedImageManager>();
    }

    private void Start(){
        _aRTrackedImageManager.trackedImagesChanged += OnImagesChanged;
        CardNames = new List<string>();
    }

    private void OnDestroy(){
        _aRTrackedImageManager.trackedImagesChanged += OnImagesChanged;
    }

    private void OnImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    { 
        foreach(ARTrackedImage trackedImage in eventArgs.added){
            UpdateTrackedImage(trackedImage);
        }

        foreach(ARTrackedImage trackedImage in eventArgs.updated){
            UpdateTrackedImage(trackedImage);
        }

        foreach(ARTrackedImage trackedImage in eventArgs.removed){
            if (CardNames.Contains(trackedImage.referenceImage.name)){
                CardNames.Remove(trackedImage.referenceImage.name);
            }
        }
        text.text = string.Join("\n", CardNames);
    }

    private void UpdateTrackedImage(ARTrackedImage trackedImage){
        if(trackedImage.trackingState is TrackingState.Limited or TrackingState.None){
            return;
        }

        if (!CardNames.Contains(trackedImage.referenceImage.name)){
            CardNames.Add(trackedImage.referenceImage.name);
        }
    }
}