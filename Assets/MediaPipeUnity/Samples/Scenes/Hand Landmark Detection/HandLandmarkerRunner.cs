// Copyright (c) 2023 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections;
using Mediapipe.Tasks.Vision.HandLandmarker;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Mediapipe.Unity.Sample.HandLandmarkDetection
{
  public class HandLandmarkerRunner : VisionTaskApiRunner<HandLandmarker>
  {

    [SerializeField] private HandLandmarkerResultAnnotationController _handLandmarkerResultAnnotationController;
    [SerializeField] private MenuGestureController menuGestureController;
     [SerializeField] private YAGesturePauseController yaGesturePauseController;
[SerializeField] private HandCursorController handCursorController;
[SerializeField] private PauseGestureController pauseGestureController;
[SerializeField] private HandUIController handUIController;
[SerializeField] private CinematicTrailerController cinematicTrailerController;
[SerializeField] private LevelIntroTutorialController levelIntroTutorialController;


[SerializeField] private HandUseItemController handUseItemController;

    private Experimental.TextureFramePool _textureFramePool;
    private bool shouldRefreshSceneControllers = true;
    private float refreshSceneControllersUntilTime = 0f;

    public readonly HandLandmarkDetectionConfig config = new HandLandmarkDetectionConfig();

    public override void Stop()
    {
      base.Stop();
      _textureFramePool?.Dispose();
      _textureFramePool = null;
    }

    private void OnEnable()
    {
      SceneManager.sceneLoaded += OnSceneLoaded;
      RequestSceneControllerRefresh();
    }

    private void OnDisable()
    {
      SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
      RequestSceneControllerRefresh();
    }

    private void Update()
    {
      RefreshSceneControllersIfNeeded();
    }

    private void RequestSceneControllerRefresh()
    {
      shouldRefreshSceneControllers = true;
      refreshSceneControllersUntilTime = Time.unscaledTime + 3f;
    }

    protected override IEnumerator Run()
    {
      Debug.Log($"Delegate = {config.Delegate}");
      Debug.Log($"Image Read Mode = {config.ImageReadMode}");
      Debug.Log($"Running Mode = {config.RunningMode}");
      Debug.Log($"NumHands = {config.NumHands}");
      Debug.Log($"MinHandDetectionConfidence = {config.MinHandDetectionConfidence}");
      Debug.Log($"MinHandPresenceConfidence = {config.MinHandPresenceConfidence}");
      Debug.Log($"MinTrackingConfidence = {config.MinTrackingConfidence}");

      yield return AssetLoader.PrepareAssetAsync(config.ModelPath);

      var options = config.GetHandLandmarkerOptions(config.RunningMode == Tasks.Vision.Core.RunningMode.LIVE_STREAM ? OnHandLandmarkDetectionOutput : null);
      taskApi = HandLandmarker.CreateFromOptions(options, GpuManager.GpuResources);
      var imageSource = ImageSourceProvider.ImageSource;

      yield return imageSource.Play();

      if (!imageSource.isPrepared)
      {
        Debug.LogError("Failed to start ImageSource, exiting...");
        yield break;
      }

      // Use RGBA32 as the input format.
      // TODO: When using GpuBuffer, MediaPipe assumes that the input format is BGRA, so maybe the following code needs to be fixed.
      _textureFramePool = new Experimental.TextureFramePool(imageSource.textureWidth, imageSource.textureHeight, TextureFormat.RGBA32, 10);

      // NOTE: The screen will be resized later, keeping the aspect ratio.
      if (screen != null)
      {
        screen.Initialize(imageSource);
      }

      if (_handLandmarkerResultAnnotationController != null)
      {
        SetupAnnotationController(_handLandmarkerResultAnnotationController, imageSource);
      }

      var transformationOptions = imageSource.GetTransformationOptions();
      var flipHorizontally = transformationOptions.flipHorizontally;
      var flipVertically = transformationOptions.flipVertically;
      var imageProcessingOptions = new Tasks.Vision.Core.ImageProcessingOptions(rotationDegrees: (int)transformationOptions.rotationAngle);

      AsyncGPUReadbackRequest req = default;
      var waitUntilReqDone = new WaitUntil(() => req.done);
      var waitForEndOfFrame = new WaitForEndOfFrame();
      var result = HandLandmarkerResult.Alloc(options.numHands);

      // NOTE: we can share the GL context of the render thread with MediaPipe (for now, only on Android)
      var canUseGpuImage = SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3 && GpuManager.GpuResources != null;
      using var glContext = canUseGpuImage ? GpuManager.GetGlContext() : null;

      while (true)
      {
        RefreshSceneControllersIfNeeded();

        if (isPaused)
        {
          yield return new WaitWhile(() => isPaused);
        }

        if (!_textureFramePool.TryGetTextureFrame(out var textureFrame))
        {
          yield return new WaitForEndOfFrame();
          continue;
        }

        // Build the input Image
        Image image;
        switch (config.ImageReadMode)
        {
          case ImageReadMode.GPU:
            if (!canUseGpuImage)
            {
              throw new System.Exception("ImageReadMode.GPU is not supported");
            }
            textureFrame.ReadTextureOnGPU(imageSource.GetCurrentTexture(), flipHorizontally, flipVertically);
            image = textureFrame.BuildGPUImage(glContext);
            // TODO: Currently we wait here for one frame to make sure the texture is fully copied to the TextureFrame before sending it to MediaPipe.
            // This usually works but is not guaranteed. Find a proper way to do this. See: https://github.com/homuler/MediaPipeUnityPlugin/pull/1311
            yield return waitForEndOfFrame;
            break;
          case ImageReadMode.CPU:
            yield return waitForEndOfFrame;
            textureFrame.ReadTextureOnCPU(imageSource.GetCurrentTexture(), flipHorizontally, flipVertically);
            image = textureFrame.BuildCPUImage();
            textureFrame.Release();
            break;
          case ImageReadMode.CPUAsync:
          default:
            req = textureFrame.ReadTextureAsync(imageSource.GetCurrentTexture(), flipHorizontally, flipVertically);
            yield return waitUntilReqDone;

            if (req.hasError)
            {
              Debug.LogWarning($"Failed to read texture from the image source");
              continue;
            }
            image = textureFrame.BuildCPUImage();
            textureFrame.Release();
            break;
        }

        switch (taskApi.runningMode)
        {
          case Tasks.Vision.Core.RunningMode.IMAGE:
            if (taskApi.TryDetect(image, imageProcessingOptions, ref result))
            {
              if (_handLandmarkerResultAnnotationController != null)
              {
                _handLandmarkerResultAnnotationController.DrawNow(result);
              }
            }
            else
            {
              if (_handLandmarkerResultAnnotationController != null)
              {
                _handLandmarkerResultAnnotationController.DrawNow(default);
              }
            }
            break;
          case Tasks.Vision.Core.RunningMode.VIDEO:
            if (taskApi.TryDetectForVideo(image, GetCurrentTimestampMillisec(), imageProcessingOptions, ref result))
            {
              if (_handLandmarkerResultAnnotationController != null)
              {
                _handLandmarkerResultAnnotationController.DrawNow(result);
              }
            }
            else
            {
              if (_handLandmarkerResultAnnotationController != null)
              {
                _handLandmarkerResultAnnotationController.DrawNow(default);
              }
            }
            break;
          case Tasks.Vision.Core.RunningMode.LIVE_STREAM:
            taskApi.DetectAsync(image, GetCurrentTimestampMillisec(), imageProcessingOptions);
            break;
        }
      }
    }

    private void RefreshSceneControllersIfNeeded()
    {
      bool hasCriticalControllers =
        yaGesturePauseController != null &&
        handCursorController != null &&
        pauseGestureController != null;

      if (!shouldRefreshSceneControllers &&
          hasCriticalControllers &&
          Time.unscaledTime > refreshSceneControllersUntilTime)
      {
        return;
      }

      menuGestureController = FindObjectOfType<MenuGestureController>();
      yaGesturePauseController = FindObjectOfType<YAGesturePauseController>();
      handCursorController = FindObjectOfType<HandCursorController>();
      pauseGestureController = FindObjectOfType<PauseGestureController>();
      handUIController = FindObjectOfType<HandUIController>();
      cinematicTrailerController = FindObjectOfType<CinematicTrailerController>();
      levelIntroTutorialController = FindObjectOfType<LevelIntroTutorialController>();
      handUseItemController = FindObjectOfType<HandUseItemController>();

      hasCriticalControllers =
        yaGesturePauseController != null &&
        handCursorController != null &&
        pauseGestureController != null;

      if (hasCriticalControllers || Time.unscaledTime > refreshSceneControllersUntilTime)
      {
        shouldRefreshSceneControllers = false;
      }
    }

    private void OnHandLandmarkDetectionOutput(
      HandLandmarkerResult result,
      Image image,
      long timestamp)
    {
      if (_handLandmarkerResultAnnotationController != null)
      {
        _handLandmarkerResultAnnotationController.DrawLater(result);
      }

      if (result.handLandmarks == null || result.handLandmarks.Count == 0)
      {
        if (levelIntroTutorialController != null)
        {
          levelIntroTutorialController.ClearHands();
        }

        if (cinematicTrailerController != null)
        {
          cinematicTrailerController.ClearHand();
        }

        return;
      }

      if (menuGestureController != null)
        menuGestureController.CheckHand(result.handLandmarks[0]);

      if (yaGesturePauseController != null)
      {
        for (int i = 0; i < result.handLandmarks.Count; i++)
        {
          yaGesturePauseController.CheckHand(result.handLandmarks[i]);
        }
      }

      if (handCursorController != null)
        handCursorController.CheckHand(result.handLandmarks[0]);

      if (handUIController != null)
        handUIController.CheckHand(result.handLandmarks[0]);

      if (levelIntroTutorialController != null)
        levelIntroTutorialController.CheckHands(result.handLandmarks);

      if (result.handedness == null)
      {
        if (cinematicTrailerController != null)
        {
          cinematicTrailerController.ClearHand();
        }

        return;
      }

      int handCount = Mathf.Min(result.handLandmarks.Count, result.handedness.Count);
      bool checkedCinematicSkipHand = false;

      for (int i = 0; i < handCount; i++)
      {
        if (result.handedness[i].categories == null || result.handedness[i].categories.Count == 0)
        {
          continue;
        }

        string handLabel = result.handedness[i].categories[0].categoryName;

        Debug.Log("目前手：" + handLabel);

        if (cinematicTrailerController != null && cinematicTrailerController.IsSkipHandLabel(handLabel))
        {
          cinematicTrailerController.CheckHand(result.handLandmarks[i], handLabel);
          checkedCinematicSkipHand = true;
        }

        // 實際右手
        if (handLabel == "Left" && handUseItemController != null)
        {
          handUseItemController.CheckHand(result.handLandmarks[i]);
        }
      }

      if (cinematicTrailerController != null && !checkedCinematicSkipHand)
      {
        cinematicTrailerController.ClearHand();
      }
    }
  }
 }
